using ScoreList.Configuration;
using ScoreList.Downloaders;
using ScoreList.Scores;
using Zenject;

namespace ScoreList.Installers
{
    internal class AppInstaller : Installer
    {
        private readonly PluginConfig _config;
        
        public AppInstaller(PluginConfig config)
        {
            _config = config;
        }
        
        public override void InstallBindings()
        {
            Container.Bind<PluginConfig>().FromInstance(_config).AsSingle();
            Container.Bind<ScoreSaberDownloader>().AsSingle();
            Container.Bind<ScoreManager>().AsSingle();
        }
    }
}