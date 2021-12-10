using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IPlatformUserModel _user;
        
        public ScoreSaberDownloader(SiraLog siraLog, IPlatformUserModel user) : base(siraLog)
        {
            _siraLog = siraLog;
            _user = user;
        }

        private int GetUserID()
        {
            var userID = _user.GetUserInfo().Id;
            return userID;
        }

        public async Task<List<ScoreSaberUtils.ScoreSaberLeaderboardEntry>> GetScores(int id, CancellationToken cancellationToken)
        {
            id = GetUserID();
            
            string url = API_URL + PLAYER + id;
            return await MakeJsonRequestAsync<List<ScoreSaberUtils.ScoreSaberLeaderboardEntry>>(url, cancellationToken);
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
    }
}