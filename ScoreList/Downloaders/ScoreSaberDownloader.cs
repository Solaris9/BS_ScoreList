using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ScoreList.Utils;
using SiraUtil.Tools;
using UnityEngine;

namespace ScoreList.Downloaders
{
    public class ScoreSaberDownloader : Downloader
    {
        private const string CDN_URL = "https://cdn.scoresaber.com/";
        private const string COVERS = "covers/";

        private static Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        
        private readonly SiraLog _siraLog;
        
        public ScoreSaberDownloader(SiraLog siraLog) : base(siraLog)
        {
            _siraLog = siraLog;
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