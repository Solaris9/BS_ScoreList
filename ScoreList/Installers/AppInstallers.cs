using System;
using System.IO;
using ScoreList.Configuration;
using ScoreList.Downloaders;
using ScoreList.Scores;
using SiraUtil.Logging;
using Zenject;

namespace ScoreList.Installers
{
    internal class AppInstaller : Installer
    {
        private readonly SiraLog _logger;
        
        public AppInstaller(SiraLog logger)
        {
            _logger = logger;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_logger);
        }
    }
}