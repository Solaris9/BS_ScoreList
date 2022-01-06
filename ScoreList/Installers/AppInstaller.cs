using IPA.Loader;
using ScoreList.Configuration;
using ScoreList.Downloaders;
using ScoreList.Scores;
using Zenject;

namespace ScoreList.Installers
{
    internal class AppInstaller : MonoInstaller
    {
        private PluginConfig _config;
        
        public AppInstaller(PluginConfig config)
        {
            _config = config;
        }
        
        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.Bind<ScoreSaberDownloader>().AsSingle();
            Container.Bind<ScoreManager>().AsSingle();
        }
    }
}