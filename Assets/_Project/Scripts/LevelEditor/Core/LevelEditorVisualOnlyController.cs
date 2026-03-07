using UnityEngine;

public class LevelEditorVisualOnlyController : MonoBehaviour
{
    private void Awake()
    {
        if (GameModeManager.Current != GameMode.LevelEditor)
        {
            DisableVisuals();
        }
    }

    private void DisableVisuals()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }
    }
}
