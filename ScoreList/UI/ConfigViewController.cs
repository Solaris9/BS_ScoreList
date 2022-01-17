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
        [UIComponent("cache-button")] readonly Button cacheButton;

        [UIAction("#post-parse")]
        async void SetupUI()
        {
            if (_config.Complete)
            {
                var total = await _scoreManager.Total();
                cacheStatus.text = $"{total} scores cached";
                cacheButton.SetButtonText("Refresh");
            }
            else
            {
                cacheStatus.text = "Not updated";
                cacheButton.SetButtonText("Update");
            }
        }

        // TODO: still improve this for updating existing scores
        [UIAction("CacheScores")]
        async void CacheScores()
        {
            var cancellationToken = new CancellationToken();
            
            await _downloader.CacheScores(cancellationToken,(pages, page) =>
            {
                cacheStatus.text = $"{page + 1}/{pages}";
            });

            var total = await _scoreManager.Total();
            _scoreManager.Clean();

            cacheStatus.text = $"Updated scores\n{total} scores cached";
            cacheButton.SetButtonText("Refresh");

            _config.Complete = true;
        }
    }
}