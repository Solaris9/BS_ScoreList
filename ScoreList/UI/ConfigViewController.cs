using System.Threading;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using ScoreList.Configuration;
using ScoreList.Downloaders;
using ScoreList.Scores;
using TMPro;
using UnityEngine.UI;
using Zenject;

#pragma warning disable CS0649

namespace ScoreList.UI
{
    [HotReload(RelativePathToLayout = @"Views\Config.bsml")]
    [ViewDefinition("ScoreList.UI.Views.Config.bsml")]
    public class ConfigViewController : BSMLAutomaticViewController
    {
        [Inject] readonly ScoreManager _scoreManager;
        [Inject] readonly ScoreSaberDownloader _downloader;
        [Inject] readonly PluginConfig _config;
        
        [UIComponent("cache-status")] readonly TextMeshProUGUI cacheStatus;
        [UIComponent("current-status")] readonly TextMeshProUGUI currentStatus;
        [UIComponent("cache-button")] readonly Button cacheButton;

        [UIAction("#post-parse")]
        void SetupUI()
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

        // TODO: still improve this for updating existing scores
        [UIAction("CacheScores")]
        async void CacheScores()
        {
            var cancellationToken = new CancellationToken();
            
            await _downloader.CacheScores(cancellationToken,(pages, page) =>
            {
                currentStatus.text = $"{page}/{pages}";
            });

            _scoreManager.Clean();

            cacheStatus.text = "Updated scores";
            
            cacheButton.SetButtonText("Up to date");
            cacheButton.interactable = false;

            _config.Complete = true;
        }
    }
}