public class PlaySession
{
    public string LevelFilePath { get; private set; }

    public void SetLevelFilePath(string path)
    {
        LevelFilePath = path;
    }

    public void Clear()
    {
        LevelFilePath = null;
    }
}
