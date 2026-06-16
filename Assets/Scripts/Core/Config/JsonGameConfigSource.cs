using System.IO;
using UnityEngine;

namespace IT.Core.Config
{
    // Reads the minimal JSON config from StreamingAssets, falling back to defaults on ANY failure.
    // TODO (Story 2.2): async UnityWebRequest path — on Android/WebGL, StreamingAssets is not a real
    //                   file path, so File.ReadAllText won't work there. Desktop/editor are fine.
    // TODO (Story 2.3): per-field validation that logs the offending field by name (one clear error).
    public class JsonGameConfigSource : IGameConfigSource
    {
        const string RelativePath = "Config/game-settings.json";

        public GameConfig Load()
        {
            string path = Path.Combine(Application.streamingAssetsPath, RelativePath);
            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[GameConfig] No config file at '{path}'. Using defaults.");
                    return GameConfig.Default;
                }

                GameSettings settings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(path));
                if (settings == null)
                {
                    Debug.LogWarning($"[GameConfig] Could not parse '{path}'. Using defaults.");
                    return GameConfig.Default;
                }

                return new GameConfig(settings);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameConfig] Failed to load '{path}': {ex.Message}. Using defaults.");
                return GameConfig.Default;
            }
        }
    }
}
