using System;
using System.Threading;
using System.Threading.Tasks;
using IPA.Loader;
using Newtonsoft.Json;
using ScoreList.Utils;
using SiraUtil.Logging;
using SiraUtil.Tools;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.Networking;

namespace ScoreList.Downloaders
{
    public abstract class Downloader
    {
        internal string USER_AGENT { get; set; }
        internal ConcurrentHashSet<UnityWebRequest> _ongoingWebRequests = new ConcurrentHashSet<UnityWebRequest>();
        internal readonly UBinder<Plugin, PluginMetadata> _metadata;
        private readonly SiraLog _siraLog;

        public Downloader(UBinder<Plugin, PluginMetadata> metadata, SiraLog siraLog)
        {
            _metadata = metadata;
            _siraLog = siraLog;
            USER_AGENT = $"SongList/v{_metadata.Value.HVersion}";
        }

        protected Downloader(SiraLog siraLog)
        {
            _siraLog = siraLog;
        }

        ~Downloader()
        {
            foreach (var webRequest in _ongoingWebRequests)
            {
                webRequest.Abort();
            }
        }

        public void CancelAllDownloads()
        {
            foreach (var webRequest in _ongoingWebRequests)
            {
                webRequest.Abort();
            }
        }
        internal async Task<T> MakeJsonRequestAsync<T>(string url, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            var www = await MakeRequestAsync(url, cancellationToken, progressCallback);

            if (www == null)
            {
                return default(T);
            }

            try
            {
                T response = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);

                return response;
            }
            catch (Exception e)
            {
                _siraLog.Warn($"Error parsing response: {e.Message}");
                return default(T);
            }
        }
        
        internal async Task<Sprite> MakeImageRequestAsync(string url, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            var www = await MakeRequestAsync(url, cancellationToken, progressCallback);

            if (www == null)
            {
                return null;
            }

            try
            {
                Sprite sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(www.downloadHandler.data);
                return sprite;
            }
            catch (Exception e)
            {
                _siraLog.Warn($"Error parsing image: {e.Message}");
                return null;
            }
        }

        private async Task<UnityWebRequest> MakeRequestAsync(string url, CancellationToken cancellationToken,
            Action<float> progressCallback = null)
        {
            var www = UnityWebRequest.Get(url);
            www.SetRequestHeader("User-Agent", USER_AGENT);
            www.timeout = 15;
#if DEBUG
            _siraLog.Debug($"Making web request: {url}");
#endif
            _ongoingWebRequests.Add(www);

            while (!www.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    www.Abort();
                    throw new TaskCanceledException();
                }
                progressCallback?.Invoke(www.downloadProgress);
                await Task.Yield();
            }
#if DEBUG
            _siraLog.Debug("Web request {url} finished");
#endif
            _ongoingWebRequests.Remove(www);

            if (www.isNetworkError || www.isHttpError)
            {
                _siraLog.Warn($"Error making request: {www.error}");
                return null;
            }

            return www;
        }
    }
}