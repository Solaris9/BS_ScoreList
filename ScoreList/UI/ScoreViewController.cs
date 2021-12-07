using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ScoreList.Scores;
using System;
using System.IO;

namespace ScoreList.UI {
    public class ScoreInfoCellWrapper
    {
        public LeaderboardScore score;

        [UIValue("icon")] public string icon;

        [UIValue("title")] public string title;
        [UIValue("artist")] public string artist;
        [UIValue("mapper")] public string mapper;

        [UIValue("stars")] public string stars;
        [UIValue("difficulty")] public string difficulty;
        [UIValue("max-pp")] public string maxPP;

        [UIValue("rank")] public string rank;
        [UIValue("pp")] public string pp;
        [UIValue("modifiers")] public string modifiers;
        [UIValue("missed-notes")] public string missedNotes;
        [UIValue("accuracy")] public string accuracy;

        public ScoreInfoCellWrapper(LeaderboardScore score)
        {
            this.score = score;
            var leaderboard = score.GetLeaderboard().GetAwaiter().GetResult();
            var info = score.GetMapInfo().GetAwaiter().GetResult();

            icon = Path.Combine(Plugin.ModFolder, "icons", info.SongHash);

            title = info.SongName;
            if (title.Length > 25) title = title.Substring(0, 25) + "...";

            artist = info.SongAuthorName;
            mapper = info.LevelAuthorName;

            stars = leaderboard.Stars.ToString("#.00★");
            difficulty = SongUtils.GetDifficultyDisplay(leaderboard.Difficultly);
            maxPP = leaderboard.MaxPP.ToString("#.00");

            rank = score.Rank.ToString();
            pp = score.PP.ToString("#.00");
            // TODO: add this back when modifiers are fixed
            modifiers = ""; //string.Join(", ", SongUtils.FormatModifiers(score.Modifiers));
            missedNotes = score.MissedNotes.ToString();

            accuracy = (100f * score.BaseScore / leaderboard.MaxScore).ToString("0.##");
        }
    }

    [HotReload(RelativePathToLayout = @"Views\ScoreList.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreList.bsml")]
    public class ScoreViewController : BSMLAutomaticViewController {
        public event Action<LeaderboardScore> didSelectSong;

        [UIComponent("list")]
        public CustomCellListTableData scoreList;

        [UIAction("#post-parse")]
        internal void SetupUI() {
            var query = new SearchQuery {
                SortBy = "PP",
                Order = "DESC"
            };

            FilterScores(query);
        }

        [UIAction("SongSelect")]
        public void SongSelect(TableView _, object song) => didSelectSong?.Invoke(((ScoreInfoCellWrapper)song).score);

        public async void FilterScores(SearchQuery query) {
            scoreList.data.Clear();

            var scores = await DatabaseManager.Client.Query<LeaderboardScore>(query.ToString());

            foreach (LeaderboardScore score in scores) {
                var scoreCell = new ScoreInfoCellWrapper(score);
                scoreList.data.Add(scoreCell);
            }

            didSelectSong?.Invoke(((ScoreInfoCellWrapper)scoreList.data[0]).score);
            scoreList.tableView.ReloadData();
        }
    }
}
