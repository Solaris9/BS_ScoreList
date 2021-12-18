using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaverSharp;
using ScoreList.Scores;
using System.Threading.Tasks;
using Zenject;
using SongCore;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using BeatSaberMarkupLanguage;
using System.Linq;
using ScoreList.Utils;
using SiraUtil.Logging;
using SiraUtil.Tools;

namespace ScoreList.UI
{
    [HotReload(RelativePathToLayout = @"Views\ScoreDetail.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreDetail.bsml")]
    class DetailViewController : BSMLAutomaticViewController
    {
        private readonly LevelSelectionUtils _levelSelectionUtils;
        private readonly SiraLog _siraLog;
        private readonly ScoreManager _scoreManager;

        private bool canPlay = true;

        private LeaderboardInfo _leaderboard;
        private LeaderboardMapInfo _info;

        [UIComponent("actionButton")]
        public Button actionButton;

        [UIComponent("title")]
        public TextMeshProUGUI title;

        [Inject]
        public DetailViewController(
            LevelSelectionUtils levelSelectionUtils,
            SiraLog siraLog,
            ScoreManager scoreManager
        )
        {
            _levelSelectionUtils = levelSelectionUtils;
            _siraLog = siraLog;
            _scoreManager = scoreManager;
        }

        public async Task Load(LeaderboardScore score)
        {
            _leaderboard = await _scoreManager.GetLeaderboard(score.LeaderboardId);
            _info = await _scoreManager.GetMapInfo(_leaderboard.SongHash);

            title.text = _info.SongName;

            actionButton.SetButtonText("Play");

            if (!SongDataCore.Plugin.Songs.Data.Songs.ContainsKey(_info.SongHash))
            {
                actionButton.SetButtonText("Download");
                canPlay = false;
            }
        }

        public void Reset()
        {
            canPlay = true;
            _leaderboard = null;
            _info = null;
        }

        
        //TODO: Need to fix this shit too.
        [UIAction("StartLevel")]
        public async Task StartLevel()
        {
            /*if (!canPlay)
            {
                await beatsaver.BeatmapByHash(info.SongHash);
                actionButton.SetButtonText("Play");
                return;
            }*/
            
            await _levelSelectionUtils.StartSoloLevel(
                _info.SongHash,
                _leaderboard.Difficultly,
                () => { },
                (so, results) => { }
            );
            
        }
    }
}