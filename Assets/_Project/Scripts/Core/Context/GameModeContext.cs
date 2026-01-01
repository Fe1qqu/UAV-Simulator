public enum GameMode
{
    Play,
    LevelEditor
}

public static class GameModeContext
{
    public static GameMode Current { get; set; }
}
