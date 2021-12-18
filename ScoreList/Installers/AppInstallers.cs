using IPALogger = IPA.Logging.Logger;
using Zenject;

namespace ScoreList.Installers
{
    internal class AppInstaller : Installer
    {
        private readonly IPALogger _logger;
        
        public AppInstaller(IPALogger logger)
        {
            _logger = logger;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_logger);
        }
    }
}