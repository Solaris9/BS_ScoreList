using System.Collections.Generic;
using System.Linq;

namespace ScoreList.Scores 
{
    public enum Modifers
    {
        NF = 1,
        OL = 2,
        FL = 4,
        NB = 8,
        NW = 16,
        NA = 32,
        GN = 64,
        DA = 128,
        SN = 256,
        PM = 512,
        SA = 1024,
        ZM = 2048,
        SS = 4096,
        FS = 8192,
        SF = 16384
    }
    
    public class SongUtils
    {
        //TODO: Rewrite the bitmapping to use the new enum.
        public static int ParseModifiers(string value) => 0;
        
        //TODO: Rewrite this to use the new enum
        public static List<string> FormatModifiers(int bitfield) => null;

        public static string GetDifficultyDisplay(int diff)
        {
            switch (diff)
            {
                case 1:
                    return "Easy";
                case 3:
                    return "Normal";
                case 5:
                    return "Hard";
                case 7:
                    return "Expert";
                case 9:
                    return "Expert+";
            }

            return null;
        }
        public static BeatmapDifficulty GetBeatmapDifficulty(int diff)
        {
            switch (diff)
            {
                case 1:
                    return BeatmapDifficulty.Easy;
                case 3:
                    return BeatmapDifficulty.Normal;
                case 5:
                    return BeatmapDifficulty.Hard;
                case 7:
                    return BeatmapDifficulty.Expert;
                case 9:
                    return BeatmapDifficulty.ExpertPlus;
            }

            return BeatmapDifficulty.Easy;
        }
    }
}
