using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScoreList.Configuration;
using ScoreList.Scores;
using ScoreList.Utils;
using SiraUtil.Logging;
using UnityEngine;

#pragma warning disable CS0649

namespace ScoreList.Downloaders
{
    internal class ScoreSaberDownloader : Downloader
    {
        const string API_URL = "https://scoresaber.com/api/";
        const string PLAYER = "player/";
        const string SCORES = "/scores";

        const string CDN_URL = "https://cdn.scoresaber.com/";
        const string COVERS = "covers/";

        static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        
        private readonly SiraLog _siraLog;
        private readonly ScoreManager _scoreManager;
        private readonly IPlatformUserModel _user;
        private readonly PluginConfig _config;

        private bool _wait;
        private int _duration;

        internal ScoreSaberDownloader(
            SiraLog siraLog,
            IPlatformUserModel user,
            ScoreManager scoreManager,
            PluginConfig config
        ) : base(siraLog) {
            _siraLog = siraLog;
            _scoreManager = scoreManager;
            _user = user;
            _config = config;
        }

        async Task<string> GetUserID()
        {
            var userID = await _user.GetUserInfo();
            return userID.platformUserId;
        }

        public async Task<Sprite> GetCoverImageAsync(string hash, CancellationToken cancellationToken)
        {
            hash = hash.ToUpper();
            if (SpriteCache.ContainsKey(hash))
            {
                return SpriteCache[hash];
            }
            
            string url = CDN_URL + COVERS + hash + ".png";
            
            var sprite = await MakeImageRequestAsync(url, cancellationToken);
            if (sprite != null)
            {
                SpriteCache.Add(hash, sprite);
            }

            return sprite;
        }
        
        private async Task<T> MakeScoreRequest<T>(string url, CancellationToken cancellationToken)
        {
            if (_wait)
            {
                await Task.Delay(TimeSpan.FromSeconds(_duration), cancellationToken);
                _wait = false;
            }

            var request = await MakeRequestAsync(url, cancellationToken, new Action<float>(_ => { }));

            var remaining = int.Parse(request.GetResponseHeader("x-ratelimit-remaining"));
            if (remaining == 0)
            {
                _wait = true;
                var resetAt = int.Parse(request.GetResponseHeader("x-ratelimit-reset"));
                _duration = (int) DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalSeconds - resetAt;
            }
            
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);;
        }

        private async Task<ScoreSaberUtils.ScoreSaberScoresMetadata> GetMetadata(CancellationToken cancellationToken)
        {
            var id = await GetUserID();
            var url = API_URL + PLAYER + id + SCORES + $"?page=1&sort=recent&limit=100";
            var data = await MakeScoreRequest<ScoreSaberUtils.ScoreSaberScores>(url, cancellationToken);
            return data.Metadata;
        }

        public async Task CacheScores(CancellationToken cancellationToken, Action<int, int> pageCachedCallback)
        {
            var id = await GetUserID();
            var metadata = await GetMetadata(cancellationToken);
            var pages = (int) Math.Ceiling((double) metadata.Total / metadata.ItemsPerPage);

            for (var page = 1; page != pages; page++)
            {
                var url = API_URL + PLAYER + id + SCORES + $"?page={page}&sort=recent&limit=100";
                var data = await MakeScoreRequest<ScoreSaberUtils.ScoreSaberScores>(url, cancellationToken);
        
                var leaderboardScores = data.PlayerScores.Select(LeaderboardScore.Create);
                
                var scores = leaderboardScores as LeaderboardScore[] ?? leaderboardScores.ToArray();
                var maps = data.PlayerScores.Select(e => LeaderboardMapInfo.Create(e.Leaderboard));
                var leaderboards = data.PlayerScores.Select(LeaderboardInfo.Create);
        
                await _scoreManager.InsertScores(scores.ToArray());
                await _scoreManager.InsertLeaderboards(leaderboards.ToArray());
                await _scoreManager.InsertMaps(maps.ToArray());

                await _scoreManager.Write();

                if (scores.Any(s => s.TimeSet == _config.LastCachedScore)) break;

                pageCachedCallback.Invoke(pages, page);
                _config.LastCachedScore = scores.Last().TimeSet;
            }
        }
    }
}