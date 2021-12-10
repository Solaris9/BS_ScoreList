using System;
using System.IO;
using ScoreList.Configuration;
using ScoreList.Downloaders;
using ScoreList.Scores;
using Zenject;

namespace ScoreList.Installers
{
    public class CoreInstallers : Installer
    {

        public override void InstallBindings()
        {
            var config = PluginConfig.Load();
            Container.Bind<PluginConfig>().FromInstance(config).AsSingle();
            Container.Bind<ScoreSaberDownloader>().AsSingle();
            Container.Bind<ScoreManager>().AsSingle();
        }
    }
}