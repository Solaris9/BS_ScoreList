using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ScoreList.Scores;
using System;
using System.IO;
using ScoreList.Configuration;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace ScoreList.UI {
    public class ScoreInfoCellWrapper
    {
        public LeaderboardScore score;

        [UIValue("icon")] public string icon;

        [UIValue("title")] public string title;
        [UIValue("artist")] public string artist;
        [UIValue("mapper")] public string mapper;

        [UIValue("rank")] public string rank;
        [UIValue("modifiers")] public string modifiers;
        [UIValue("missed-notes")] public string missedNotes;
        [UIValue("difficulty")] public string difficulty;

        [UIComponent("stars")] public TextMeshProUGUI stars;
        
        [UIComponent("accuracy-layout")] public LayoutElement accuracyLayout;
        [UIComponent("pp-layout")] public LayoutElement ppLayout;
        
        [UIValue("accuracy")] public string accuracy;
        [UIValue("max-pp")] public string maxPP;
        [UIValue("pp")] public string pp;

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

            difficulty = SongUtils.GetDifficultyDisplay(leaderboard.Difficultly);
            rank = score.Rank.ToString();
            modifiers = string.Join(", ", SongUtils.FormatModifiers(score.Modifiers));
            missedNotes = score.MissedNotes.ToString();

            if (leaderboard.Ranked)
            {
                ppLayout.enabled = true;
                accuracyLayout.enabled = true;
                
                stars.text = leaderboard.Stars.ToString("#.00★");
                accuracy = (100f * score.BaseScore / leaderboard.MaxScore).ToString("0.##");
                maxPP = leaderboard.MaxPP.ToString("#.00");
                pp = score.PP.ToString("#.00");
            }
            else
            {
                stars.enabled = false;
            }
        }
    }

    [HotReload(RelativePathToLayout = @"Views\ScoreList.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreList.bsml")]
    public class ScoreViewController : BSMLAutomaticViewController {
        public event Action<LeaderboardScore> didSelectSong;
        private SQLiteClient _db;
        private PluginConfig _config;

        [Inject]
        public ScoreViewController(SQLiteClient db, PluginConfig config)
        {
            _db = db;
            _config = config;
        }

        [UIComponent("list")]
        public CustomCellListTableData scoreList;

        [UIAction("#post-parse")]
        internal void SetupUI()
        {
            if (!_config.Complete) return;
            
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

            var scores = await _db.Query<LeaderboardScore>(query.ToString());
            if (scores.Count == 0) return;

            foreach (var score in scores) {
                var scoreCell = new ScoreInfoCellWrapper(score);
                scoreList.data.Add(scoreCell);
            }

            didSelectSong?.Invoke(((ScoreInfoCellWrapper)scoreList.data[0]).score);
            scoreList.tableView.ReloadData();
        }
    }
}
