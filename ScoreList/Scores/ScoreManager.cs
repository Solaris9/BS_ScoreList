﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScoreList.Utils;

namespace ScoreList.Scores
{
    public class ScoreManager
    {
        private static LeaderboardData _data;
        
        private readonly string _filePath = Path.Combine(Plugin.ModFolder, "data.json");

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
            
            using (var sw = new StreamWriter(_filePath)) 
                await sw.WriteAsync(json);

            _data = null;
        }

        private async Task<LeaderboardData> Read()
        {
            if (!File.Exists(_filePath)) return new LeaderboardData();

            using (var sw = new StreamReader(_filePath))
            {
                var content = await sw.ReadToEndAsync();
                return JsonConvert.DeserializeObject<LeaderboardData>(content);
            }
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
        public List<LeaderboardScore> Scores = new List<LeaderboardScore>();
        public List<LeaderboardInfo> Leaderboards = new List<LeaderboardInfo>();
        public List<LeaderboardMapInfo> Maps = new List<LeaderboardMapInfo>();
    }
    
    public class LeaderboardScore
    {
        public int ScoreId { get; set; }
        public int LeaderboardId { get; set; }
        public int Rank { get; set; }
        public int BaseScore { get; set; }
        public int ModifiedScore { get; set; }
        public double PP { get; set; }
        public int Modifiers { get; set; }
        public int MissedNotes { get; set; }
        public DateTime TimeSet { get; set; }

        public static LeaderboardScore Create(ScoreSaberUtils.ScoreSaberLeaderboardEntry entry) => 
            new LeaderboardScore {
                ScoreId = entry.Score.Id,
                LeaderboardId = entry.Leaderboard.Id,
                Rank = entry.Score.Rank,
                BaseScore = entry.Score.BaseScore,
                ModifiedScore = entry.Score.ModifiedScore,
                PP = entry.Score.PP,
                Modifiers = SongUtils.ParseModifiers(entry.Score.Modifiers),
                MissedNotes = entry.Score.MissedNotes,
                TimeSet = DateTime.Parse(entry.Score.TimeSet)
            };
    }

    public class LeaderboardInfo
    {
        public int LeaderboardId { get; set; }
        public bool Ranked { get; set; }
        public string SongHash { get; set; }
        public int Difficultly { get; set; }
        public string GameMode { get; set; }
        public DateTime? RankedDate { get; set; }
        public double MaxPP { get; set; }
        public int MaxScore { get; set; }
        public double Stars { get; set; }
        public bool PositiveModifiers { get; set; }

        public static LeaderboardInfo Create(ScoreSaberUtils.ScoreSaberLeaderboardEntry entry)
        {
            DateTime? rankedDate = null;
            if (entry.Leaderboard.RankedDate != null) rankedDate = DateTime.Parse(entry.Leaderboard.RankedDate);
                
            double maxPp = entry.Leaderboard.MaxPP;
            if (maxPp == -1) maxPp = entry.Score.PP * entry.Leaderboard.MaxScore / entry.Score.BaseScore;
            
            return new LeaderboardInfo
            {
                LeaderboardId = entry.Leaderboard.Id,
                Ranked = entry.Leaderboard.Ranked,
                SongHash = entry.Leaderboard.SongHash,
                Difficultly = entry.Leaderboard.Difficulty.Difficulty,
                GameMode = entry.Leaderboard.Difficulty.GameMode,
                RankedDate = rankedDate,
                MaxPP = maxPp,
                MaxScore = entry.Leaderboard.MaxScore,
                Stars = entry.Leaderboard.Stars,
                PositiveModifiers = entry.Leaderboard.PositiveModifiers,
            };
        }
    }

    public class LeaderboardMapInfo
    {
        public string SongHash { get; set; }
        public string SongName { get; set; }
        public string SongSubName { get; set; }
        public string SongAuthorName { get; set; }
        public string LevelAuthorName { get; set; }
        public DateTime CreatedDate { get; set; }

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