using IPA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScoreList.UI;
using UnityEngine.Networking;
using System.IO;
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

        public Plugin()
        { }

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

        private async void DownloadImages()
        {
            var maps = await DatabaseManager.Client.Query<LeaderboardMapInfo>("SELECT * FROM maps");
            
        }
        
        
    }
}
