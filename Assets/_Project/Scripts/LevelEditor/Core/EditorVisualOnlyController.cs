using UnityEngine;

public class EditorVisualOnlyController : MonoBehaviour
{
    private void Awake()
    {
        if (GameModeContext.Current != GameMode.LevelEditor)
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
