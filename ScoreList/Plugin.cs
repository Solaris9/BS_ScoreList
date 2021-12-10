using IPA;
using ScoreList.UI;
using System.IO;
using IPA.Utilities;
using ScoreList.Scores;
using SiraUtil.Zenject;
using ScoreList.Installers;
using SiraUtil.Logging;
using SiraUtil.Tools;

namespace ScoreList
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");

        private static Plugin Instance { get; set; }
        private static SiraLog _siraLog { get; set; }

        public Plugin(SiraLog siraLog)
        {
            _siraLog = siraLog;
        }

        [Init]
        public async void Init(SiraLog logger, Zenjector zenjector)
        {
            zenjector.Install<MenuInstallers>(Location.Menu);

            Instance = this;
            _siraLog.Info("ScoreList initialized.");
            
            await DatabaseManager.Connect();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            // DownloadImages();
        }
    }
}
