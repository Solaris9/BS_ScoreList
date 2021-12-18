using ScoreList.Downloaders;
using ScoreList.Scores;
using ScoreList.Utils;
using IPALogger = IPA.Logging.Logger;
using Zenject;

namespace ScoreList.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<ScoreSaberDownloader>().AsSingle();
            Container.Bind<ScoreManager>().AsSingle();
            Container.Bind<LevelSelectionUtils>().AsSingle();
        }
    }
}