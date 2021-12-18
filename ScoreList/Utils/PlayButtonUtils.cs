using System;
using System.Collections.Generic;
using SongCore;

namespace ScoreList.Utils
{
    public class PlayButtonUtils
    {
        public string hash = "";
        
        private MenuTransitionsHelper _transitions;

        public PlayButtonUtils(MenuTransitionsHelper transitions)
        {
            _transitions = transitions;
        }
        
        private void StartSoloLevel(Action beforeSceneSwitchCallback)
        {
            
            
            List<string> level = SongCore.Collections.levelIDsForHash(hash);
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(level.ToString());
            
            _transitions.StartStandardLevel("SoloStandard", beatmapLevel, null, null, null, null, null, null, null, null, null);
        }
    }
}