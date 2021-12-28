using IPA.Loader;
using ScoreList.Downloaders;
using ScoreList.Scores;
using Zenject;

namespace ScoreList.Installers
{
    internal class AppInstaller : MonoInstaller 
    {
        public override void InstallBindings()
        {
            Container.Bind<ScoreSaberDownloader>().AsSingle();
            Container.Bind<ScoreManager>().AsSingle();
        }
    }
}