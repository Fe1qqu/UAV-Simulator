//using UnityEngine;
using System;

public static class GameStateManager
{
    public static GameState CurrentState { get; private set; } = GameState.Boot;

    public static event Action<GameState> StateChanged;

    public static void SetState(GameState state)
    {
        if (CurrentState == state)
        {
            return;
        }

        //GameState previous = CurrentState;
        CurrentState = state;

        //Debug.Log($"[GameState] {previous} -> {state}.");

        StateChanged?.Invoke(state);
    }
}
