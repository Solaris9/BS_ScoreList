using System;
using IPA;
using ScoreList.UI;
using System.IO;
using IPA.Utilities;
using IPALogger = IPA.Logging.Logger;
using ScoreList.Scores;
using SiraUtil.Zenject;
using ScoreList.Installers;

namespace ScoreList
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        public Plugin()
        {
            
        }

        [Init]
        public async void Init(IPALogger logger, Zenjector zenjector)
        {
            zenjector.OnGame<MenuInstallers>();

            Instance = this;
            Log = logger;
            Log.Info("ScoreList initialized.");
            
            await DatabaseManager.Connect();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            ScoreListUI.instance.Setup();

            // DownloadImages();
        }
    }
}
