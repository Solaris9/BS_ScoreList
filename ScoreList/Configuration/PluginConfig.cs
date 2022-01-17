using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ScoreList.Configuration
{
    public class PluginConfig
    {
        public virtual bool Complete { get; set; }
        [UseConverter(typeof(DictionaryConverter<Dictionary<string, object>>))]
        public virtual Dictionary<string, Dictionary<string, object>> Presets { get; set;  } = new Dictionary<string, Dictionary<string, object>>();
    }
}