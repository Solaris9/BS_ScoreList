using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ScoreList.Configuration
{
    internal class PluginConfig
    {
        public virtual bool Complete { get; set; }
        public virtual DateTime LastCachedScore { get; set; }
    }
}