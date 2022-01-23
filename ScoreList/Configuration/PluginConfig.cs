using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using ScoreList.Scores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ScoreList.Configuration
{
    public class PluginConfig
    {
        public virtual bool Complete { get; set; }
        // public virtual Dictionary<string, Dictionary<string, BaseFilter>> Presets { get; set;  } = new Dictionary<string, Dictionary<string, BaseFilter>>();
        
        [UseConverter]
        public virtual List<FilterConfig> Presets { get; set;  } = new List<FilterConfig>();
    }

    public class FilterConfig
    {
        public string Name { get; set; }

        [UseConverter]
        public Dictionary<string, BaseFilter> Filters { get; set; } = new Dictionary<string, BaseFilter>();
    }
}