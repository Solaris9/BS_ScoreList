using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScoreList.Scores
{
    public class HttpManger
    {
        public static HttpClient client;

        public static void Init()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "ScoreList (Solaris#1969)");
        }

        public static async Task<List<ScoreSaberLeaderboardEntry>> GetScores(string id, int page)
        {
            var url = $"https://scoresaber.com/api/player/{id}/scores?limit=100&sort=top&page={page}";
            var res = await client.GetAsync(url);
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ScoreSaberLeaderboardEntry>>(content);
        }

        public static async Task<ScoreSaberProfileStats> GetScoreCount(string id)
        {
            var url = $"https://scoresaber.com/api/player/{id}/full";
            var res = await client.GetAsync(url);
            var content = await res.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ScoreSaberProfile>(content);
            return json.ScoreStats;
        }
    }

    #region models

    public class ScoreSaberProfile
    {
        [JsonProperty("scoreStats")]
        public ScoreSaberProfileStats ScoreStats;
    }

    public class ScoreSaberProfileStats
    {
        [JsonProperty("totalPlayCount")]
        public int TotalPlayCount;
        [JsonProperty("rankedPlayCount")]
        public int RankedPlayCount;
    }

    public class ScoreSaberLeaderboardEntry
    {
        [JsonProperty("score")]
        public ScoreSaberLeaderboardScore Score;
        [JsonProperty("leaderboard")]
        public ScoreSaberLeaderboardInfo Leaderboard;
    }

    public class ScoreSaberLeaderboardScore
    {
        [JsonProperty("id")]
        public int Id;
        [JsonProperty("rank")]
        public int Rank;
        [JsonProperty("baseScore")]
        public int BaseScore;
        [JsonProperty("modifiedScore")]
        public int ModifiedScore;
        [JsonProperty("pp")]
        public double PP;
        [JsonProperty("weight")]
        public double Weight;
        [JsonProperty("modifiers")]
        public string Modifiers;
        [JsonProperty("multiplier")]
        public double Multiplier;
        [JsonProperty("badCuts")]
        public int BadCuts;
        [JsonProperty("missedNotes")]
        public int MissedNotes;
        [JsonProperty("maxCombo")]
        public int MaxCombo;
        [JsonProperty("fullCombo")]
        public bool FullCombo;
        [JsonProperty("hmd")]
        public int HMD;
        [JsonProperty("timeSet")]
        public string TimeSet;
        [JsonProperty("hasReplay")]
        public bool HasReply;
    }

    public class ScoreSaberLeaderboardInfo
    {
        [JsonProperty("id")]
        public int Id;
        [JsonProperty("songHash")]
        public string SongHash;
        [JsonProperty("songName")]
        public string SongName;
        [JsonProperty("songSubName")]
        public string SongSubName;
        [JsonProperty("songAuthorName")]
        public string SongAuthorName;
        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName;
        [JsonProperty("maxScore")]
        public int MaxScore;
        [JsonProperty("difficulty")]
        public ScoreSaberLeaderboardInfoDifficulty Difficulty;
        [JsonProperty("createdDate")]
        public string CreatedDate;
        [JsonProperty("rankedDate")]
        public string RankedDate;
        [JsonProperty("qualifiedDate")]
        public string QualifiedDate;
        [JsonProperty("lovedDate")]
        public string LovedDate;
        [JsonProperty("ranked")]
        public bool Ranked;
        [JsonProperty("qualified")]
        public bool Qualified;
        [JsonProperty("loved")]
        public bool Loved;
        [JsonProperty("maxPP")]
        public int MaxPP;
        [JsonProperty("stars")]
        public double Stars;
        [JsonProperty("plays")]
        public int Plays;
        [JsonProperty("positiveModifiers")]
        public bool PositiveModifiers;
        [JsonProperty("playerScore")]
        public object PlayerScore;
        [JsonProperty("difficulties")]
        public object Difficultlies;
    }

    public class ScoreSaberLeaderboardInfoDifficulty
    {
        [JsonProperty("difficulty")]
        public int Difficulty;
        [JsonProperty("difficultyRaw")]
        public string DifficultyRaw;
        [JsonProperty("gameMode")]
        public string GameMode;
    }

    #endregion models
}