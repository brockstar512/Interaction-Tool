using System.IO;
using UnityEngine;

namespace IT.Core.Config
{
    // Reads the minimal JSON config from StreamingAssets, falling back to defaults on ANY failure.
    // Desktop/Editor only: File.ReadAllText requires a real filesystem path (not available on
    // Android/WebGL). AsyncJsonGameConfigSource handles those platforms via UnityWebRequest.
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
