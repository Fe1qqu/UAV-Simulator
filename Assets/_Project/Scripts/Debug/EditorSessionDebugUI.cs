using UnityEngine;

/// <summary>
/// Отладочный UI для EditorSession. Показывает LevelName, SelectedLocationId и SelectedLevelFile.
/// </summary>
public class EditorSessionDebugUI : MonoBehaviour
{
    private bool visible = true;
    private Rect windowRect = new Rect(20, 20, 300, 150);

    private void OnGUI()
    {
        if (!visible) return;

        windowRect = GUI.Window(12345, windowRect, DrawWindow, "EditorSession Debug");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        if (GameSettings.Instance == null || GameSettings.Instance.CurrentEditorSession == null)
        {
            GUILayout.Label("EditorSession отсутствует!");
        }
        else
        {
            var session = GameSettings.Instance.CurrentEditorSession;

            GUILayout.Label("<b>EditorSession</b>");
            GUILayout.Label($"LevelName: {session.LevelName ?? "null"}");
            GUILayout.Label($"SelectedLocationId: {session.SelectedLocationId ?? "null"}");
            GUILayout.Label($"SelectedLevelFilePath: {session.SelectedLevelFilePath ?? "null"}");
        }

        GUILayout.Space(5);

        if (GUILayout.Button(visible ? "Hide" : "Show"))
        {
            ToggleVisibility();
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    public void ToggleVisibility()
    {
        visible = !visible;
    }
}
