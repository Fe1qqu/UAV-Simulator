using UnityEngine;

public class LevelEditorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.LevelEditor;
        Debug.Log("[GameMode] LevelEditor");
    }
}
