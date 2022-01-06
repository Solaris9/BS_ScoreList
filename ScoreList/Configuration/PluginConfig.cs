using System;

namespace ScoreList.Configuration
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public virtual bool Complete { get; set; }
        public virtual DateTime LastCachedScore { get; set; }
    }
}