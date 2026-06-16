using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace IT.Core.Config
{
    // Android/WebGL config source. File.ReadAllText won't work on StreamingAssets on those platforms
    // (it's a compressed bundle path, not a real filesystem path). UnityWebRequest handles both.
    //
    // IGameConfigSource.Load() is synchronous; it cannot block on a web request, so it returns
    // GameConfig.Default with a warning. Call LoadAsync() from a MonoBehaviour async entry point
    // (async void / async Awaitable) and pass destroyCancellationToken per §11.
    //
    // TODO (Story 2.3): per-field validation that logs the offending field by name.
    public class AsyncJsonGameConfigSource : IGameConfigSource
    {
        const string RelativePath = "Config/game-settings.json";

        // Sync fallback — satisfies IGameConfigSource but cannot issue a web request synchronously.
        public GameConfig Load()
        {
            Debug.LogWarning("[GameConfig] Sync Load() called on AsyncJsonGameConfigSource; " +
                             "use LoadAsync() from a MonoBehaviour entry point on this platform. Using defaults.");
            return GameConfig.Default;
        }

        // Real async load path for Android/WebGL. Returns GameConfig.Default (+ one warning) on any
        // network or parse failure. OperationCanceledException propagates to the caller per §11.
        public async Awaitable<GameConfig> LoadAsync(CancellationToken token)
        {
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, RelativePath);
            using var req = UnityWebRequest.Get(url);

            // Abort the in-flight request if the owning MonoBehaviour is destroyed mid-load.
            using var _ = token.Register(() => req.Abort());

            await req.SendWebRequest();
            token.ThrowIfCancellationRequested();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[GameConfig] AsyncLoad failed ({req.error}). Using defaults.");
                return GameConfig.Default;
            }

            try
            {
                GameSettings settings = JsonUtility.FromJson<GameSettings>(req.downloadHandler.text);
                if (settings == null)
                {
                    Debug.LogWarning("[GameConfig] AsyncLoad: could not parse JSON. Using defaults.");
                    return GameConfig.Default;
                }
                return new GameConfig(settings);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameConfig] AsyncLoad: parse error ({ex.Message}). Using defaults.");
                return GameConfig.Default;
            }
        }
    }
}
