using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ScoreList.Scores;
using System;
using System.Collections.Generic;
using System.IO;
using ScoreList.Configuration;
using SiraUtil.Logging;
using TMPro;
using UnityEngine.UI;
using Zenject;

#pragma warning disable CS0649

namespace ScoreList.UI
{
    public class ScoreInfoCellWrapper
    {
        public readonly int ScoreId;
        readonly LeaderboardScore _score;
        readonly LeaderboardInfo _leaderboard;
        readonly LeaderboardMapInfo _info;
        
        [UIValue("icon")] readonly string icon;

        [UIValue("title")] readonly string title;
        [UIValue("artist")] readonly string artist;
        [UIValue("mapper")] readonly string mapper;

        [UIValue("rank")] readonly string rank;
        [UIValue("modifiers")] readonly string modifiers;
        [UIValue("missed-notes")] readonly string missedNotes;
        [UIValue("difficulty")] readonly string difficulty;

        [UIComponent("stars")] readonly TextMeshProUGUI stars;
        [UIComponent("accuracy-layout")] readonly LayoutElement accuracyLayout;
        [UIComponent("pp-layout")] readonly LayoutElement ppLayout;

        [UIComponent("max-pp")] readonly TextMeshProUGUI maxPP;
        [UIComponent("pp")] readonly TextMeshProUGUI pp;
        [UIValue("accuracy")] string accuracy;

        public ScoreInfoCellWrapper(LeaderboardScore score, LeaderboardInfo leaderboard, LeaderboardMapInfo info)
        {
            _score = score;
            _leaderboard = leaderboard;
            _info = info;
                
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
        }

        [UIAction("#post-parse")]
        internal void SetupUI()
        {
            ppLayout.gameObject.SetActive(_leaderboard.Ranked);
            accuracyLayout.gameObject.SetActive(_leaderboard.Ranked);
            stars.gameObject.SetActive(_leaderboard.Ranked);
            
            if (_leaderboard.Ranked)
            {
                stars.text = _leaderboard.Stars.ToString("#.00★");
                accuracy = (100f * _score.BaseScore / _leaderboard.MaxScore).ToString("0.##");
                maxPP.text = _leaderboard.MaxPp.ToString("#.00");
                pp.text = _score.Pp.ToString("#.00");
            }
        }
        
    }

    [HotReload(RelativePathToLayout = @"Views\ScoreList.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreList.bsml")]
    public class ScoreViewController : BSMLAutomaticViewController
    {
        public event Action<int> DidSelectSong;
        
        [Inject] private readonly ScoreManager _scoreManager;
        [Inject] private readonly SiraLog _siraLog;

        [UIComponent("list")]
        public CustomCellListTableData scoreList;

        [UIAction("#post-parse")]
        internal void SetupUI()
        {
            var filters = new List<BaseFilter>
            {
                new SortPpFilter(),
                new OrderFilter("DESC")
            };

            FilterScores(filters);
        }

        [UIAction("SongSelect")]
        public void SongSelect(TableView _, object song) => DidSelectSong?.Invoke(((ScoreInfoCellWrapper)song).ScoreId);

        public async void FilterScores(List<BaseFilter> filters)
        {
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
