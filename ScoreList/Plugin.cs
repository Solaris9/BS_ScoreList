using IPA;
using System.IO;
using IPA.Config;
using IPA.Loader;
using IPA.Utilities;
using SiraUtil.Zenject;
using ScoreList.Installers;
using IPALogger = IPA.Logging.Logger;

namespace ScoreList
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static readonly string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");
        public static string Version { private set; get; } 

        [Init]
        public void Init(Zenjector zenjector, IPALogger logger, PluginMetadata metadata, Config config)
        {
            Version = metadata.HVersion.ToString();
            
            zenjector.UseLogger(logger);
            zenjector.Install<AppInstaller>(Location.App);
            zenjector.Install<MenuInstaller>(Location.Menu);
        }
    }
}
