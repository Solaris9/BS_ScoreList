using System;
using IPA;
using ScoreList.UI;
using System.IO;
using System.Security.Policy;
using System.Threading;
using IPA.Utilities;
using IPALogger = IPA.Logging.Logger;
using ScoreList.Scores;
using SiraUtil.Zenject;
using ScoreList.Installers;
using ScoreList.Utils;

namespace ScoreList
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal Downloader _downloader;

        public Plugin(Downloader downloader)
        {
            _downloader = downloader;
        }

        [Init]
        public async void Init(IPALogger logger, Zenjector zenjector)
        {
            zenjector.OnGame<MenuInstallers>();

            Instance = this;
            Log = logger;
            Log.Info("ScoreList initialized.");

            HttpManger.Init();
            await DatabaseManager.Connect();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            ScoreListUI.instance.Setup();

            // DownloadImages();
        }

        private async void DownloadImages(string url, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            await _downloader.MakeImageRequestAsync(url, cancellationToken, progressCallback);
        }
    }
}
