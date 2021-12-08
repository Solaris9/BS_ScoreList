using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using ScoreList.Configuration;
using ScoreList.Downloaders;
using ScoreList.Scores;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace ScoreList.UI
{
    [HotReload(RelativePathToLayout = @"Views\Config.bsml")]
    [ViewDefinition("ScoreList.UI.Views.Config.bsml")]
    public class ConfigViewController : BSMLAutomaticViewController
    {
        private SQLiteClient _db;
        private ScoreSaberDownloader _downloader;
        private IPlatformUserModel _user;
        private PluginConfig _config;
        
        [UIComponent("cache-status")] public TextMeshProUGUI cacheStatus;
        [UIComponent("cache-button")] public Button cacheButton;

        [Inject]
        public ConfigViewController(
            SQLiteClient db,
            ScoreSaberDownloader downloader,
            IPlatformUserModel user,
            PluginConfig config
        )
        {
            _db = db;
            _downloader = downloader;
            _user = user;
            _config = config;
        }

        [UIAction("#post-parse")]
        internal void SetupUI()
        {
            if (_config.Complete)
            {
                cacheStatus.text = "Updated";
                cacheButton.interactable = false;
            }
            else
            {
                cacheStatus.text = "Not updated";
                cacheButton.SetButtonText("Not updated");
            }
        }

        [UIAction("CacheScores")]
        internal async void CacheScores()
        {
            var cancellationToken = new CancellationToken();
            var info = await _user.GetUserInfo();
            var id = info.platformUserId;
            
            var max = 1;
            var current = 0;

            cacheStatus.text = $"0/{max}";

            for (; current < max; current++)
            {
                var scores = await _downloader.FetchScores(id, current + 1, cancellationToken, sort: "recent");

                foreach (var entry in scores)
                {
                    await LeaderboardMapInfo.Create(entry.Leaderboard);
                    await LeaderboardInfo.Create(entry);
                    await LeaderboardScore.Create(entry);

                    await Task.Delay(1000, cancellationToken);
                }

                cacheStatus.text = $"{current + 1}/{max}";
            }

            cacheStatus.text = "Updated scores";
            cacheButton.interactable = false;

            _config.Complete = true;
            _config.Save();
        }
    }
}