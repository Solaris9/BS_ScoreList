using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using ScoreList.Scores;
using System.Threading.Tasks;
using Zenject;
using UnityEngine.UI;
using TMPro;
using BeatSaberMarkupLanguage;
using ScoreList.Utils;
using SiraUtil.Logging;

#pragma warning disable CS0414
#pragma warning disable CS0169
#pragma warning disable CS0649

namespace ScoreList.UI
{
    [HotReload(RelativePathToLayout = @"Views\ScoreDetail.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreDetail.bsml")]
    class DetailViewController : BSMLAutomaticViewController
    {
        bool _canPlay = true;

        LeaderboardInfo _leaderboard;
        LeaderboardMapInfo _info;
        
        [Inject] readonly LevelSelectionUtils _levelSelectionUtils;
        [Inject] readonly SiraLog _siraLog;
        [Inject] readonly ScoreManager _scoreManager;

        [UIComponent("actionButton")] readonly Button actionButton;
        [UIComponent("title")] readonly TextMeshProUGUI title;

        public async Task Load(LeaderboardScore score)
        {
            _leaderboard = await _scoreManager.GetLeaderboard(score.LeaderboardId);
            _info = await _scoreManager.GetMapInfo(_leaderboard.SongHash);

            title.text = _info.SongName;

            actionButton.SetButtonText("Play");

            if (!SongDataCore.Plugin.Songs.Data.Songs.ContainsKey(_info.SongHash))
            {
                actionButton.SetButtonText("Download");
                _canPlay = false;
            }
        }

        public void Reset()
        {
            _canPlay = true;
            _leaderboard = null;
            _info = null;
        }

        //TODO: Need to fix this shit too.
        [UIAction("StartLevel")]
        async Task StartLevel()
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