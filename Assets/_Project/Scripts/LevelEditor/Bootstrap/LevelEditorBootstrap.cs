using UnityEngine;

public class LevelEditorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.LevelEditor;
        GameSettings.Instance.EnterGameplay();
        //Debug.Log("[GameMode] LevelEditor.");
    }
}
