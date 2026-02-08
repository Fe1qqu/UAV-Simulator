using UnityEngine;
using System;

//public enum PauseMode
//{
//    None,
//    Soft,   // UI + input off, timeScale = 1
//    Hard    // timeScale = 0
//}
//public static void SetPause(PauseMode mode)

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    public static event Action<bool> PauseStateChanged;

    public static void SetPaused(bool paused)
    {
        if (IsPaused == paused)
        {
            return; 
        }

        IsPaused = paused;

        Time.timeScale = paused ? 0f : 1f;

        PauseStateChanged?.Invoke(paused);
    }
}
