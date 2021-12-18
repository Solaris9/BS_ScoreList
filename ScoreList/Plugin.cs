using IPA;
using ScoreList.UI;
using System.IO;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using ScoreList.Configuration;
using ScoreList.Scores;
using SiraUtil.Zenject;
using ScoreList.Installers;
using SiraUtil.Logging;
using SiraUtil.Tools;
using IPALogger = IPA.Logging.Logger;

namespace ScoreList
{
    [Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
    public class Plugin
    {
        public static string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");

        private static Plugin Instance { get; set; }
        private static SiraLog _siraLog { get; set; }

        [Init]
        public void Init(Zenjector zenjector, IPALogger logger, Config config)

        {
            _siraLog.Info("Initializing ScoreList..");
            zenjector.UseLogger(logger);
            zenjector.Install<AppInstaller>(Location.App, config.Generated<PluginConfig>());
            zenjector.Install<MenuInstallers>(Location.Menu);

            Instance = this;
        }
    }
}
