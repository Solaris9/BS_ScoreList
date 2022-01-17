using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ScoreList.Configuration
{
    public class PluginConfig
    {
        public virtual bool Complete { get; set; }
        public virtual List<FilterPreset> Presets { get; } = new List<FilterPreset>();
    }

    public class FilterPreset
    {
        public string Name { get; set; }
        public Dictionary<string, object> Filters { get; set; }
    }
}