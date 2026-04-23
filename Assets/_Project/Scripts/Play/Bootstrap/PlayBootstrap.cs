using UnityEngine;

public class PlayBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeManager.SetMode(GameMode.Play);

        GameStateManager.SetState(GameState.Gameplay);

        //Debug.Log("[GameMode] Play.");
    }
}
