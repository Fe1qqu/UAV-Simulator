using UnityEngine;

public class PlayBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeManager.SetMode(GameMode.Play);

        GameStateManager.SetState(GameState.Gameplay);

        InputModeController.Instance.SetMode(InputMode.Play);

        //Debug.Log("[GameMode] Play.");
    }
}
