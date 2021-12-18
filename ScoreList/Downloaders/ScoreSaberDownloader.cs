using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScoreList.Scores;
using ScoreList.Utils;
using SiraUtil.Logging;
using SiraUtil.Tools;
using UnityEngine;

namespace ScoreList.Downloaders
{
    public class ScoreSaberDownloader : Downloader
    {
        private const string API_URL = "https://scoresaber.com/api/";
        private const string PLAYER = "player/";
        private const string SCORES = "scores";

        private const string CDN_URL = "https://cdn.scoresaber.com/";
        private const string COVERS = "covers/";

        private static Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        
        private readonly SiraLog _siraLog;
        private readonly ScoreManager _scoreManager;
        private readonly IPlatformUserModel _user;
        
        public ScoreSaberDownloader(SiraLog siraLog, IPlatformUserModel user, ScoreManager scoreManager) : base(siraLog)
        {
            _siraLog = siraLog;
            _scoreManager = scoreManager;
            _user = user;
        }

        private int GetUserID()
        {
            var userID = _user.GetUserInfo().Id;
            return userID;
        }

        public async Task<Sprite> GetCoverImageAsync(string hash, CancellationToken cancellationToken)
        {
            hash = hash.ToUpper();
            if (_spriteCache.ContainsKey(hash))
            {
                return _spriteCache[hash];
            }
            
            string url = CDN_URL + COVERS + hash + ".png";
            
            var sprite = await MakeImageRequestAsync(url, cancellationToken);
            if (sprite != null)
            {
                _spriteCache.Add(hash, sprite);
            }

            return sprite;
        }
        
        private async Task<ScoreSaberUtils.ScoreSaberScores> GetScores(
            int page, string sort, CancellationToken cancellationToken)
        {
            var id = GetUserID();
            string url = API_URL + PLAYER + id + SCORES + $"?page{page}&sort={sort}&limit=100";
            return await MakeJsonRequestAsync<ScoreSaberUtils.ScoreSaberScores>(url, cancellationToken);
        }

        public async Task CacheScores(int current, CancellationToken cancellationToken)
        {
            var data = await GetScores(current + 1, "recent",  cancellationToken);

            var maps = data.PlayerScores.Select(e => LeaderboardMapInfo.Create(e.Leaderboard));
            var leaderboards = data.PlayerScores.Select(LeaderboardInfo.Create);
            var scores = data.PlayerScores.Select(LeaderboardScore.Create);

            await _scoreManager.InsertScores(scores.ToArray());
            await _scoreManager.InsertLeaderboards(leaderboards.ToArray());
            await _scoreManager.InsertMaps(maps.ToArray());
                
            // delay so there's a max of 400 requests within a minute
            await Task.Delay(200, cancellationToken);
        }
    }
}