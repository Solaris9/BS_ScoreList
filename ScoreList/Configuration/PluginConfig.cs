using System.IO;
using Newtonsoft.Json;

namespace ScoreList.Configuration
{
    public class PluginConfig
    {
        private static readonly string _path = Path.Combine(Plugin.ModFolder, "config.json");

        public bool Complete = false;
        public int? MaxPages = null;
        public int? LoadedPages = null;

        public void Save()
        {
            var content = JsonConvert.SerializeObject(this);
            File.WriteAllText(_path, content);
        }
        
        public static PluginConfig Load()
        {
            if (!File.Exists(_path)) return new PluginConfig();

            var content = File.ReadAllText(_path);
            return JsonConvert.DeserializeObject<PluginConfig>(content);
        }
    }
}