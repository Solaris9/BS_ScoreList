using IPA;
using ScoreList.UI;
using System.IO;
using IPA.Utilities;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace ScoreList
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static readonly string ModFolder = Path.Combine(UnityGame.UserDataPath, "ScoreList");

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        public Plugin()
        {
            
        }

        [Init]
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            if (!Directory.Exists(ModFolder)) Directory.CreateDirectory(ModFolder);
            
            zenjector.OnApp<Installers.CoreInstallers>();
            zenjector.OnGame<Installers.MenuInstallers>();

            Instance = this;
            Log = logger;
            Log.Info("ScoreList initialized.");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            ScoreListUI.instance.Setup();

            // DownloadImages();
        }
    }
}
