namespace IT.Core.Config
{
    // Seam over "where the config comes from". Story 2.1 ships JsonGameConfigSource; the swappable
    // interface lets Story 2.2 add an async UnityWebRequest source for Android/WebGL without touching
    // consumers.
    public interface IGameConfigSource
    {
        GameConfig Load();
    }
}
