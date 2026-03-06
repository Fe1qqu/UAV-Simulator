using UnityEngine;

public class PlayBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.Play;

        GameStateManager.SetState(GameState.Gameplay);

        //Debug.Log("[GameMode] Play.");
    }
}
