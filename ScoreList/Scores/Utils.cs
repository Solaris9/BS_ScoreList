using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoreList.Scores 
{
    public enum Modifiers
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
    
    public static class SongUtils
    {
        public static int ParseModifiers(string value) {
            if (value.Equals("")) return 0;

            int bitfield = 0;
            string[] modifiers = value.Split(',');

            foreach (string modifier in modifiers) {
                var modifierBits = (int) Enum.Parse(typeof(Modifiers), modifier);
                if ((bitfield & modifierBits) != modifierBits) {
                    bitfield += modifierBits;
                }
            }

            return bitfield;
        }

        public static List<string> FormatModifiers(int bitfield) =>
            Enum.GetNames(typeof(Modifiers))
                .Where(name => {
                    var bit = (int) Enum.Parse(typeof(Modifiers), name);
                    return (bitfield & bit) == bit;
                })
                .ToList();

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
