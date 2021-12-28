using System;
using System.Threading.Tasks;
using ScoreList.Scores;
using SongCore;
using UnityEngine;

namespace ScoreList.Utils
{
    public class LevelSelectionUtils
    {
        private MenuTransitionsHelper _transitions;
        private PlayerDataModel _playerDataModel;

        public LevelSelectionUtils(MenuTransitionsHelper transitions, PlayerDataModel playerDataModel)
        {
            _transitions = transitions;
            _playerDataModel = playerDataModel;
        }
        
        public async Task StartSoloLevel(
            string hash,
            int difficulty,
            Action beforeSceneSwitchCallback,
            Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> levelFinishedCallback
        ) {
            var token = new System.Threading.CancellationToken();
            var beatmapDifficulty = SongUtils.GetBeatmapDifficulty(difficulty);
            var beatmapCharacteristic = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
            var beatmapLevelResult = await Loader.BeatmapLevelsModelSO.GetBeatmapLevelAsync($"custom_level_{hash}", token);
            var beatmapLevelData = beatmapLevelResult.beatmapLevel.beatmapLevelData;
            var difficultyBeatmap = beatmapLevelData.GetDifficultyBeatmap(beatmapCharacteristic, beatmapDifficulty);

            var beatmapModifiers = new GameplayModifiers();
            
            var playerSpecificSettings = _playerDataModel.playerData.playerSpecificSettings;
            var practiceSettings = _playerDataModel.playerData.practiceSettings;
            var overrideEnvironmentSettings = _playerDataModel.playerData.overrideEnvironmentSettings;
            
            var colorSchemesSettings = _playerDataModel.playerData.colorSchemesSettings;
            var overrideColorScheme = colorSchemesSettings.overrideDefaultColors ? colorSchemesSettings.GetSelectedColorScheme() : null;
            
            _transitions.StartStandardLevel(
                "SoloStandard",
                difficultyBeatmap,
                null,
                overrideEnvironmentSettings,
                overrideColorScheme,
                beatmapModifiers,
                playerSpecificSettings,
                practiceSettings,
                "Back",
                false,
                beforeSceneSwitchCallback,
                levelFinishedCallback
            );
        }
    }
}