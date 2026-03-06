using UnityEngine;

public class LevelEditorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.LevelEditor;

        GameStateManager.SetState(GameState.Gameplay);

        //Debug.Log("[GameMode] LevelEditor.");
    }
}
