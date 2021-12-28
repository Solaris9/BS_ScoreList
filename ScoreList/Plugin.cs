using IPA;
using System.IO;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using ScoreList.Configuration;
using SiraUtil.Zenject;
using ScoreList.Installers;
using ScoreList.UI;
using IPALogger = IPA.Logging.Logger;

namespace ScoreList
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");

        private static Plugin Instance { get; set; }

        [Init]
        public void Init(Zenjector zenjector, IPALogger logger, Config config)
        {
            zenjector.UseLogger(logger);
            zenjector.Install<AppInstaller>(Location.App);
            zenjector.Install<MenuInstaller>(Location.Menu);
            
            Instance = this;
        }
    }
}
