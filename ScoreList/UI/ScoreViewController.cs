using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ScoreList.Scores;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ScoreList.Configuration;
using SiraUtil.Logging;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace ScoreList.UI {
    public class ScoreInfoCellWrapper
    {
        public int ScoreId;
        
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

        public ScoreInfoCellWrapper(LeaderboardScore score, LeaderboardInfo leaderboard, LeaderboardMapInfo info)
        {
            ScoreId = score.ScoreId;
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
        public event Action<int> DidSelectSong;
        private readonly ScoreManager _scoreManager;
        private readonly SiraLog _logger;
        private readonly PluginConfig _config;

        [Inject]
        public ScoreViewController(ScoreManager scoreManager, SiraLog logger, PluginConfig config)
        {
            _scoreManager = scoreManager;
            _logger = logger;
            _config = config;
        }

        [UIComponent("list")]
        public CustomCellListTableData scoreList;

        [UIAction("#post-parse")]
        internal void SetupUI()
        {
            // if (!_config.Complete) return;

            var filters = new List<BaseFilter>
            {
                new SortPpFilter(),
                new OrderFilter("DESC")
            };

            FilterScores(filters);
        }

        [UIAction("SongSelect")]
        public void SongSelect(TableView _, object song) => DidSelectSong?.Invoke(((ScoreInfoCellWrapper)song).ScoreId);

        public async void FilterScores(List<BaseFilter> filters) {
            scoreList.data.Clear();

            var scores = await _scoreManager.Query(filters);
            if (scores.Count == 0) return;

            foreach (var score in scores)
            {
                var leaderboard = await _scoreManager.GetLeaderboard(score.LeaderboardId);
                var map = await _scoreManager.GetMapInfo(leaderboard.SongHash);
                
                var scoreCell = new ScoreInfoCellWrapper(score, leaderboard, map);
                scoreList.data.Add(scoreCell);
            }
            
            scoreList.tableView.ReloadData();
            _scoreManager.Clean();
    
            // select first song when new filters are applied
            if (scoreList.data.Count > 0) DidSelectSong?.Invoke(((ScoreInfoCellWrapper)scoreList.data[0]).ScoreId);
        }
    }
}
