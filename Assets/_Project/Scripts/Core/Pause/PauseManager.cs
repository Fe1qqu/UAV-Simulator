using UnityEngine;
using System;

//public enum PauseMode
//{
//    None,
//    Soft,   // UI + input off, timeScale = 1
//    Hard    // timeScale = 0
//}
//public static void SetPause(PauseMode mode)

public static class PauseManager
{
    public static bool IsPaused { get; private set; }

    public static event Action<bool> PauseStateChanged;

    private const int PauseFpsLimit = 60;
    private static int previousTargetFps;
    private static bool hasStoredFps;

    public static void SetPaused(bool paused)
    {
        if (IsPaused == paused)
        {
            return; 
        }

        IsPaused = paused;

        if (paused)
        {
            if (!hasStoredFps)
            {
                previousTargetFps = Application.targetFrameRate;
                hasStoredFps = true;
            }

            Application.targetFrameRate = PauseFpsLimit;

            Time.timeScale = 0f;

            GameStateManager.SetState(GameState.Pause);
        }
        else
        {
            Time.timeScale = 1f;

            if (hasStoredFps)
            {
                Application.targetFrameRate = previousTargetFps;
                hasStoredFps = false;
            }

            GameStateManager.SetState(GameState.Gameplay);
        }

        PauseStateChanged?.Invoke(paused);
    }
}
