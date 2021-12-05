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

namespace ScoreList
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");
        public Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

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

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");

            ScoreListUI.instance.Setup();

            // DownloadImages();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
        }

        private async void DownloadImages()
        {
            var maps = await DatabaseManager.Client.Query<LeaderboardMapInfo>("SELECT * FROM maps");

            foreach (var map in maps) {
                SharedCoroutineStarter.instance.StartCoroutine(DownloadImage(map.SongHash));
            }
        }

        private IEnumerator DownloadImage(string hash)
        {
            var url = $"https://cdn.scoresaber.com/covers/{hash}.png";
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

                var texture = DownloadHandlerTexture.GetContent(uwr);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

                Sprites.Add(hash, sprite);
            }
        }
    }
}
