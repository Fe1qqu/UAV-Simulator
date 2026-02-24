using UnityEngine;

public class PlayBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.Play;

        QualitySettings.SetQualityLevel(1, false);
        GameSettings.Instance.ApplyCurrentGraphicsSettings();
        //Debug.Log("[GameMode] Play.");
    }
}
