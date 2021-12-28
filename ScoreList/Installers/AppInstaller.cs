using ScoreList.Downloaders;
using ScoreList.Scores;
using ScoreList.Utils;
using Zenject;

namespace ScoreList.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            // Container.Bind<LevelSelectionUtils>().AsSingle().NonLazy();
            Container.Bind<ScoreSaberDownloader>().AsSingle();
            Container.Bind<ScoreManager>().AsSingle();
        }
    }
}