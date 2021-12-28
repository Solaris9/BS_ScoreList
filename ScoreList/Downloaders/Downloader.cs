using System;
using System.Threading;
using System.Threading.Tasks;
using IPA.Loader;
using Newtonsoft.Json;
using ScoreList.Utils;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0649

namespace ScoreList.Downloaders
{
    public abstract class Downloader
    {
        internal readonly string USER_AGENT;
        internal ConcurrentHashSet<UnityWebRequest> _ongoingWebRequests = new ConcurrentHashSet<UnityWebRequest>();
        private readonly SiraLog _siraLog;
        
        protected Downloader(SiraLog siraLog)
        {
            _siraLog = siraLog;
            USER_AGENT = $"SongList/v{Plugin.Version}";
        }

        ~Downloader() => CancelAllDownloads();

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
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(www.downloadHandler.text);;
            }
            catch (Exception e)
            {
                _siraLog.Warn($"Error parsing response: {e.Message}");
                return default;
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

        private async Task<UnityWebRequest> MakeRequestAsync(
            string url, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            var www = UnityWebRequest.Get(url);
            www.SetRequestHeader("User-Agent", USER_AGENT);
            www.timeout = 15;
#if DEBUG
            _siraLog.Debug($"Making web request: {url}");
#endif
            _ongoingWebRequests.Add(www);

            www.SendWebRequest();

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
            _siraLog.Debug($"Web request {url} finished");
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