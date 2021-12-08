using System;
using System.Threading.Tasks;
using System.IO;
using ScoreList.Utils;

namespace ScoreList.Scores
{
    [Table("scores")]
    public class LeaderboardScore : Model<LeaderboardScore>
    {
        [PrimaryKey]
        public int ScoreId { get; set; }
        public int LeaderboardId { get; set; }
        [Indexed]
        public int Rank { get; set; }
        public int BaseScore { get; set; }
        public int ModifiedScore { get; set; }
        [Indexed]
        public double PP { get; set; }
        [Indexed]
        public int Modifiers { get; set; }
        [Indexed]
        public int MissedNotes { get; set; }
        [Indexed]
        public DateTime TimeSet { get; set; }
        public Task<LeaderboardInfo> GetLeaderboard() => LeaderboardInfo.Get(this.LeaderboardId);
        public async Task<LeaderboardMapInfo> GetMapInfo()
        {
            var leaderboard = await GetLeaderboard();

            return await LeaderboardMapInfo.Get(leaderboard.SongHash);
        }

        public static async Task Create(ScoreSaberUtils.ScoreSaberLeaderboardEntry entry)
        {
            try
            {
                var score = new LeaderboardScore
                {
                    ScoreId = entry.Score.Id,
                    LeaderboardId = entry.Leaderboard.Id,
                    Rank = entry.Score.Rank,
                    BaseScore = entry.Score.BaseScore,
                    ModifiedScore = entry.Score.ModifiedScore,
                    PP = entry.Score.PP,
                    Modifiers = SongUtils.ParseModifiers(entry.Score.Modifiers),
                    MissedNotes = entry.Score.MissedNotes + entry.Score.BadCuts,
                    TimeSet = DateTime.Parse(entry.Score.TimeSet)
                };

                await score.Save();
            }
            catch (Exception err)
            {
                Console.WriteLine("Failed to create score record for: {0} on: {1} because:", entry.Score.Id, entry.Leaderboard.Id);
                Console.WriteLine(err.ToString());
            }
        }
    }

    [Table("leaderboards")]
    public class LeaderboardInfo : Model<LeaderboardInfo>
    {
        [PrimaryKey]
        public int LeaderboardId { get; set; }
        public bool Ranked { get; set; }
        public string SongHash { get; set; }
        public int Difficultly { get; set; }
        public string GameMode { get; set; }
        [Indexed]
        public DateTime? RankedDate { get; set; }
        public double MaxPP { get; set; }
        public int MaxScore { get; set; }
        [Indexed]
        public double Stars { get; set; }
        public bool PositiveModifiers { get; set; }
        public Task<LeaderboardMapInfo> GetMapInfo() => LeaderboardMapInfo.Get(this.SongHash);

        public static async Task Create(ScoreSaberUtils.ScoreSaberLeaderboardEntry entry)
        {
            try
            {
                DateTime? rankedDate = null;
                if (entry.Leaderboard.RankedDate != null) rankedDate = DateTime.Parse(entry.Leaderboard.RankedDate);

                var maxPP = entry.Score.PP * entry.Leaderboard.MaxScore / entry.Score.BaseScore;

                var info = new LeaderboardInfo
                {
                    LeaderboardId = entry.Leaderboard.Id,
                    Ranked = entry.Leaderboard.Ranked,
                    SongHash = entry.Leaderboard.SongHash,
                    Difficultly = entry.Leaderboard.Difficulty.Difficulty,
                    GameMode = entry.Leaderboard.Difficulty.GameMode,
                    RankedDate = rankedDate,
                    MaxPP = maxPP,
                    MaxScore = entry.Leaderboard.MaxScore,
                    Stars = entry.Leaderboard.Stars,
                    PositiveModifiers = entry.Leaderboard.PositiveModifiers,
                };

                await info.Save();
            }
            catch (Exception err)
            {
                Console.WriteLine("Failed to create leaderboard record for: {0} because:", entry.Leaderboard.Id);
                Console.WriteLine(err.ToString());
            }
        }
    }

    [Table("maps")]
    public class LeaderboardMapInfo : Model<LeaderboardMapInfo>
    {
        [PrimaryKey]
        public string SongHash { get; set; }
        public string SongName { get; set; }
        public string SongSubName { get; set; }
        public string SongAuthorName { get; set; }
        public string LevelAuthorName { get; set; }
        public DateTime CreatedDate { get; set; }

        public static async Task Create(ScoreSaberUtils.ScoreSaberLeaderboardInfo leaderboard)
        {
            try
            {
                var info = new LeaderboardMapInfo
                {
                    SongHash = leaderboard.SongHash,
                    SongName = leaderboard.SongName,
                    SongSubName = leaderboard.SongSubName,
                    SongAuthorName = leaderboard.SongAuthorName,
                    LevelAuthorName = leaderboard.LevelAuthorName,
                    CreatedDate = DateTime.Parse(leaderboard.CreatedDate)
                };

                await info.Save();
            }
            catch (Exception err)
            {
                Console.WriteLine("Failed to create info record for: {0} because:", leaderboard.SongHash);
                Console.WriteLine(err.ToString());
            }
        }
    }
}