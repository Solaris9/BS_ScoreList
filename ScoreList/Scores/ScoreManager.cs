using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScoreList.Utils;
using SiraUtil.Logging;

namespace ScoreList.Scores
{
    public class ScoreManager
    {
        static LeaderboardData _data;
        readonly SiraLog _logger;

        public ScoreManager(SiraLog logger)
        {
            _logger = logger;
        }
        
        readonly string _filePath = Path.Combine(Plugin.ModFolder, "data.json");

        public async Task InsertScores(params LeaderboardScore[] scores)
        {
            if (_data == null) _data = await Read();
            
            var newScores = scores.Where(s =>
                !_data.Scores.Exists(e => e.ScoreId == s.ScoreId)
            ).ToList();
            
            _data.Scores.AddRange(newScores);
        }

        public async Task InsertLeaderboards(params LeaderboardInfo[] leaderboards)
        {
            if (_data == null) _data = await Read();
            
            var newLeaderboards = leaderboards.Where(l =>
                !_data.Leaderboards.Exists(e => e.LeaderboardId == l.LeaderboardId)
            ).ToList();
            
            _data.Leaderboards.AddRange(newLeaderboards);
        }

        public async Task InsertMaps(params LeaderboardMapInfo[] maps)
        {
            if (_data == null) _data = await Read();
            
            var newMaps = maps.Where(m =>
                !_data.Maps.Exists(e => e.SongHash == m.SongHash)
            ).ToList();

            _data.Maps.AddRange(newMaps);
        }

        public async Task Write()
        {
            if (_data == null) return;
            
            var json = JsonConvert.SerializeObject(_data);

            if (!Directory.Exists(Plugin.ModFolder))
                Directory.CreateDirectory(Plugin.ModFolder);
            
            using (var sw = new StreamWriter(_filePath)) 
                await sw.WriteAsync(json);

            _data = null;
        }

        public async Task<LeaderboardData> Read()
        {
            if (!File.Exists(_filePath)) return new LeaderboardData();
            
            /*using (var sw = new StreamReader(_filePath))
            {
                var content = await sw.ReadToEndAsync();
                return JsonConvert.DeserializeObject<LeaderboardData>(content);
            }*/

            return JsonConvert.DeserializeObject<LeaderboardData>(File.ReadAllText(_filePath));
        }

        public async Task<List<LeaderboardScore>> Query(List<BaseFilter> filters)
        {
            if (_data == null) _data = await Read();
            List<LeaderboardScore> scores = _data.Scores;

            foreach (var filter in filters) filter.Apply(ref scores, _data);
            
            return scores;
        }

        public void Clean() => _data = null;

        public async Task<LeaderboardScore> GetScore(int id)
        {
            if (_data == null) _data = await Read();
            return _data.Scores.First(s => s.ScoreId == id);
        }

        public async Task<LeaderboardMapInfo> GetMapInfo(string hash)
        {
            if (_data == null) _data = await Read();
            return _data.Maps.First(m => m.SongHash == hash);
        }

        public async Task<LeaderboardInfo> GetLeaderboard(int id)
        {
            if (_data == null) _data = await Read();
            return _data.Leaderboards.First(m => m.LeaderboardId == id);
        }
    }

    public class LeaderboardData
    {
        public readonly List<LeaderboardScore> Scores = new List<LeaderboardScore>();
        public readonly List<LeaderboardInfo> Leaderboards = new List<LeaderboardInfo>();
        public readonly List<LeaderboardMapInfo> Maps = new List<LeaderboardMapInfo>();
    }
    
    public class LeaderboardScore
    {
        public int ScoreId;
        public int LeaderboardId;
        public int Rank;
        public int BaseScore;
        public int ModifiedScore;
        public double Pp;
        public int Modifiers;
        public int MissedNotes;
        public DateTime TimeSet;

        public static LeaderboardScore Create(ScoreSaberUtils.ScoreSaberLeaderboardEntry entry) => 
            new LeaderboardScore {
                ScoreId = entry.Score.Id,
                LeaderboardId = entry.Leaderboard.Id,
                Rank = entry.Score.Rank,
                BaseScore = entry.Score.BaseScore,
                ModifiedScore = entry.Score.ModifiedScore,
                Pp = entry.Score.Pp,
                Modifiers = SongUtils.ParseModifiers(entry.Score.Modifiers),
                MissedNotes = entry.Score.MissedNotes,
                TimeSet = DateTime.Parse(entry.Score.TimeSet)
            };
    }

    public class LeaderboardInfo
    {
        public int LeaderboardId;
        public bool Ranked;
        public string SongHash;
        public int Difficultly;
        public string GameMode;
        public DateTime? RankedDate;
        public double MaxPp;
        public int MaxScore;
        public double Stars;
        public bool PositiveModifiers;

        public static LeaderboardInfo Create(ScoreSaberUtils.ScoreSaberLeaderboardEntry entry)
        {
            DateTime? rankedDate = null;
            if (entry.Leaderboard.RankedDate != null) rankedDate = DateTime.Parse(entry.Leaderboard.RankedDate);

            double maxPp = entry.Leaderboard.MaxPp;
            if (maxPp == -1) maxPp = 42.114296 * entry.Leaderboard.Stars * 1.5;
            
            return new LeaderboardInfo
            {
                LeaderboardId = entry.Leaderboard.Id,
                Ranked = entry.Leaderboard.Ranked,
                SongHash = entry.Leaderboard.SongHash,
                Difficultly = entry.Leaderboard.Difficulty.Difficulty,
                GameMode = entry.Leaderboard.Difficulty.GameMode,
                RankedDate = rankedDate,
                MaxPp = maxPp,
                MaxScore = entry.Leaderboard.MaxScore,
                Stars = entry.Leaderboard.Stars,
                PositiveModifiers = entry.Leaderboard.PositiveModifiers,
            };
        }
    }

    public class LeaderboardMapInfo
    {
        public string SongHash;
        public string SongName;
        public string SongSubName;
        public string SongAuthorName;
        public string LevelAuthorName;
        public DateTime CreatedDate;

        public static LeaderboardMapInfo Create(ScoreSaberUtils.ScoreSaberLeaderboardInfo info) => 
            new LeaderboardMapInfo {
                SongHash = info.SongHash,
                SongName = info.SongName,
                SongSubName = info.SongSubName,
                SongAuthorName = info.SongAuthorName,
                LevelAuthorName = info.LevelAuthorName,
                CreatedDate = DateTime.Parse(info.CreatedDate),
            };
    }
}