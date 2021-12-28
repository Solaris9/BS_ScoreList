using System.Linq;
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
        [Inject] private ScoreManager _scoreManager;
        [Inject] private ScoreSaberDownloader _downloader;
        // [Inject] private PluginConfig _config;
        
        [UIComponent("cache-status")] public TextMeshProUGUI cacheStatus;
        [UIComponent("cache-button")] public Button cacheButton;

        [UIAction("#post-parse")]
        internal void SetupUI()
        {
            /*if (_config.Complete)
            {
                cacheStatus.text = "Updated";
                cacheButton.interactable = false;
            }
            else
            {*/
                cacheStatus.text = "Not updated";
                cacheButton.SetButtonText("Not updated");
            //}
        }

        [UIAction("CacheScores")]
        internal async void CacheScores()
        {
            var cancellationToken = new CancellationToken();
            
            var max = 1;
            var current = 0;

            cacheStatus.text = $"0/{max}";

            for (; current < max; current++)
            {
                await _downloader.CacheScores(current + 1, cancellationToken);
                cacheStatus.text = $"{current + 1}/{max}";
            }
            
            _scoreManager.Clean();

            cacheStatus.text = "Updated scores";
            cacheButton.SetButtonText("Up to date");
            cacheButton.interactable = false;

            /*_config.Complete = true;
            _config.Save();*/
        }
    }
}