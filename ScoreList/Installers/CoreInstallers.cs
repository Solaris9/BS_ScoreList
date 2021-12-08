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
            
            var client = new SQLiteClient(Path.Combine(Plugin.ModFolder, "scores.db"));
            client.Connect();

            client.CreateTable(
                typeof(LeaderboardMapInfo),
                typeof(LeaderboardInfo),
                typeof(LeaderboardScore)
            );
            
            Container.Bind<SQLiteClient>().FromInstance(client).AsSingle();
            Container.Bind<PluginConfig>().FromInstance(config).AsSingle();
            Container.Bind<ScoreSaberDownloader>().AsSingle();
        }
    }
}