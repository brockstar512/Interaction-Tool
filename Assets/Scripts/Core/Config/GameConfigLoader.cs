namespace IT.Core.Config
{
    // Selects the right IGameConfigSource per target platform. Desktop/Editor use the synchronous
    // File.ReadAllText path (fine: StreamingAssets is a real directory). Android/WebGL require
    // UnityWebRequest because StreamingAssets is a compressed bundle there — call
    // AsyncJsonGameConfigSource.LoadAsync() from a MonoBehaviour entry point on those platforms;
    // the sync IGameConfigSource.Load() path returns defaults with a warning.
    // v1 ships desktop only; the non-desktop seam is wired but untested until a build target is added.
    public static class GameConfigLoader
    {
        public static IGameConfigSource GetSource()
        {
#if UNITY_ANDROID || UNITY_WEBGL
            return new AsyncJsonGameConfigSource();
#else
            return new JsonGameConfigSource();
#endif
        }
    }
}
