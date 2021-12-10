using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ScoreList.Utils
{
    public static class ScoreSaberUtils
    {
        public class ScoreSaberImageUtil
        {
            public List<ScoreSaberImages> image;
        }

        public class ScoreSaberImages
        {
            public Sprite sprite;
            public string downloadURL;
        }
        
        public class ScoreSaberScores
        {
            [JsonProperty("playerScores")] public List<ScoreSaberLeaderboardEntry> PlayerScores;
            [JsonProperty("metadata")] public ScoreSaberScoresMetadata Metadata;
        }

        public class ScoreSaberScoresMetadata
        {
            [JsonProperty("total")] public int Total;
            [JsonProperty("page")] public int Page;
            [JsonProperty("itemsPerPage")] public int ItemsPerPage;
        }

        public class ScoreSaberLeaderboardEntry
        {
            [JsonProperty("score")] public ScoreSaberLeaderboardScore Score;
            [JsonProperty("leaderboard")] public ScoreSaberLeaderboardInfo Leaderboard;
        }

        public class ScoreSaberLeaderboardScore
        {
            [JsonProperty("id")] public int Id;
            [JsonProperty("rank")] public int Rank;
            [JsonProperty("baseScore")] public int BaseScore;
            [JsonProperty("modifiedScore")] public int ModifiedScore;
            [JsonProperty("pp")] public double PP;
            [JsonProperty("weight")] public double Weight;
            [JsonProperty("modifiers")] public string Modifiers;
            [JsonProperty("multiplier")] public double Multiplier;
            [JsonProperty("badCuts")] public int BadCuts;
            [JsonProperty("missedNotes")] public int MissedNotes;
            [JsonProperty("maxCombo")] public int MaxCombo;
            [JsonProperty("fullCombo")] public bool FullCombo;
            [JsonProperty("hmd")] public int HMD;
            [JsonProperty("timeSet")] public string TimeSet;
            [JsonProperty("hasReplay")] public bool HasReply;
        }

        public class ScoreSaberLeaderboardInfo
        {
            [JsonProperty("id")] public int Id;
            [JsonProperty("songHash")] public string SongHash;
            [JsonProperty("songName")] public string SongName;
            [JsonProperty("songSubName")] public string SongSubName;
            [JsonProperty("songAuthorName")] public string SongAuthorName;
            [JsonProperty("levelAuthorName")] public string LevelAuthorName;
            [JsonProperty("maxScore")] public int MaxScore;
            [JsonProperty("difficulty")] public ScoreSaberLeaderboardInfoDifficulty Difficulty;
            [JsonProperty("createdDate")] public string CreatedDate;
            [JsonProperty("rankedDate")] public string RankedDate;
            [JsonProperty("qualifiedDate")] public string QualifiedDate;
            [JsonProperty("lovedDate")] public string LovedDate;
            [JsonProperty("ranked")] public bool Ranked;
            [JsonProperty("qualified")] public bool Qualified;
            [JsonProperty("loved")] public bool Loved;
            [JsonProperty("maxPP")] public int MaxPP;
            [JsonProperty("stars")] public double Stars;
            [JsonProperty("plays")] public int Plays;
            [JsonProperty("positiveModifiers")] public bool PositiveModifiers;
            [JsonProperty("playerScore")] public object PlayerScore;
            [JsonProperty("difficulties")] public object Difficultlies;
        }

        public class ScoreSaberLeaderboardInfoDifficulty
        {
            [JsonProperty("difficulty")] public int Difficulty;
            [JsonProperty("difficultyRaw")] public string DifficultyRaw;
            [JsonProperty("gameMode")] public string GameMode;
        }
    }
}
