using System;

public static class GameModeManager
{
    public static GameMode Current { get; private set; }

    public static event Action<GameMode> ModeChanged;

    public static void SetMode(GameMode mode)
    {
        if (Current == mode)
        {
            return;
        }

        Current = mode;
        ModeChanged?.Invoke(mode);
    }
}
