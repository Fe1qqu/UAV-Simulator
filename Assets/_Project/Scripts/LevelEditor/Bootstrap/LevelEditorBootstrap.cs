using UnityEngine;

public class LevelEditorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeManager.SetMode(GameMode.LevelEditor);

        GameStateManager.SetState(GameState.Gameplay);

        //Debug.Log("[GameMode] LevelEditor.");
    }
}
