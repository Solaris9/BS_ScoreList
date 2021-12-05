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

namespace ScoreList.UI
{
    [HotReload(RelativePathToLayout = @"Views\ScoreDetail.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreDetail.bsml")]
    class DetailViewController : BSMLAutomaticViewController
    {
        private MenuTransitionsHelper menuTransitionsHelper;
        private BeatmapLevelsModel beatmapLevelsModel;
        private BeatSaver beatsaver;

        private bool canPlay = true;

        private LeaderboardScore score;
        private LeaderboardInfo leaderboard;
        private LeaderboardMapInfo info;

        [UIComponent("actionButton")]
        public Button actionButton;

        [UIComponent("title")]
        public TextMeshProUGUI title;

        [Inject]
        public DetailViewController(MenuTransitionsHelper menuTransitionsHelper, BeatmapLevelsModel beatmapLevelsModel)
        {
            this.menuTransitionsHelper = menuTransitionsHelper;
            this.beatmapLevelsModel = beatmapLevelsModel;

            var options = new BeatSaverOptions("ScoreList (Solaris#1969)", "0.0.1");
            beatsaver = new BeatSaver(options);
        }

        public async Task Load(LeaderboardScore score)
        {
            leaderboard = await score.GetLeaderboard();
            info = await score.GetMapInfo();

            title.text = info.SongName;

            actionButton.SetButtonText("Play");

            if (!SongDataCore.Plugin.Songs.Data.Songs.ContainsKey(info.SongHash))
            {
                actionButton.SetButtonText("Download");
                canPlay = false;
            }
        }

        public void Reset()
        {
            canPlay = true;
            leaderboard = null;
            info = null;
        }

        
        //TODO: Need to fix this shit too.
        [UIAction("StartLevel")]
        public async Task StartLevel()
        {
            if (!canPlay)
            {
                await beatsaver.BeatmapByHash(info.SongHash);
                actionButton.SetButtonText("Play");
                return;
            }

            var token = new System.Threading.CancellationToken();
            var beatmapDifficulty = SongUtils.GetBeatmapDifficulty(leaderboard.Difficultly);
            var beatmapCharacteristic = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
            var beatmapLevelResult = await beatmapLevelsModel.GetBeatmapLevelAsync($"custom_level_{info.SongHash}", token);
            var beatmapLevelData = beatmapLevelResult.beatmapLevel.beatmapLevelData;
            var difficultyBeatmap = beatmapLevelData.GetDifficultyBeatmap(beatmapCharacteristic, beatmapDifficulty);

            menuTransitionsHelper.StartStandardLevel("SoloStandard", difficultyBeatmap, null, null, null, null, null, null, null, false, null, null);
        }
    }
}