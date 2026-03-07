using UnityEngine;

public class LevelEditorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeManager.SetMode(GameMode.LevelEditor);

        GameStateManager.SetState(GameState.Gameplay);

        InputModeController.Instance.SetMode(InputMode.LevelEditor);

        //Debug.Log("[GameMode] LevelEditor.");
    }
}
