using System;
using System.Linq;
using System.Threading.Tasks;
using ScoreList.Scores;
using SiraUtil.Logging;
using SongCore;

namespace ScoreList.Utils
{
    public class LevelSelectionUtils
    {
        private MenuTransitionsHelper _transitions;
        private PlayerDataModel _playerDataModel;
        private SiraLog _siraLog;

        public LevelSelectionUtils(MenuTransitionsHelper transitions, PlayerDataModel playerDataModel, SiraLog siraLog)
        {
            _transitions = transitions;
            _playerDataModel = playerDataModel;
            _siraLog = siraLog;
        }
        
        public async Task StartSoloLevel(
            string hash,
            int difficulty,
            Action beforeSceneSwitchCallback,
            Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> levelFinishedCallback
        ) {
            var token = new System.Threading.CancellationToken();
            var beatmapDifficulty = SongUtils.GetBeatmapDifficulty(difficulty);
            
            _siraLog.Info($"Starting song {hash}:{beatmapDifficulty.Name()}");

            var beatmapLevelResult = await Loader.BeatmapLevelsModelSO.GetBeatmapLevelAsync($"custom_level_{hash}", token);

            var difficultyBeatmapSetArray = beatmapLevelResult.beatmapLevel.beatmapLevelData.difficultyBeatmapSets;
            var beatmapCharacteristic = difficultyBeatmapSetArray.First(s => s.difficultyBeatmaps.Any(d => d.difficulty == beatmapDifficulty)).beatmapCharacteristic;

            var beatmapLevelData = beatmapLevelResult.beatmapLevel.beatmapLevelData;
            var difficultyBeatmap = beatmapLevelData.GetDifficultyBeatmap(beatmapCharacteristic, beatmapDifficulty);
            
            var beatmapModifiers = new GameplayModifiers();
            
            var playerSpecificSettings = _playerDataModel.playerData.playerSpecificSettings;
            var practiceSettings = new PracticeSettings();
            var overrideEnvironmentSettings = _playerDataModel.playerData.overrideEnvironmentSettings;
            
            var colorSchemesSettings = _playerDataModel.playerData.colorSchemesSettings;
            var overrideColorScheme = colorSchemesSettings.overrideDefaultColors ? colorSchemesSettings.GetSelectedColorScheme() : null;
            
            _transitions.StartStandardLevel(
                "SoloStandard",
                difficultyBeatmap,
                beatmapLevelResult.beatmapLevel,
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