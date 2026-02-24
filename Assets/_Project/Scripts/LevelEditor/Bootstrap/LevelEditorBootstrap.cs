using UnityEngine;

public class LevelEditorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.LevelEditor;

        QualitySettings.SetQualityLevel(1, false);
        GameSettings.Instance.ApplyCurrentGraphicsSettings();
        //Debug.Log("[GameMode] LevelEditor.");
    }
}
