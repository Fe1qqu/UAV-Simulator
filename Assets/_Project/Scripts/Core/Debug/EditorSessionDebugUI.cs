using UnityEngine;

public class EditorSessionDebugUI : MonoBehaviour
{
    private bool visible = true;
    private Rect windowRect = new(20, 20, 300, 150);

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }    

        windowRect = GUI.Window(12345, windowRect, DrawWindow, "EditorSession Debug");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        if (GameSettings.Instance == null || GameSettings.Instance.CurrentEditorSession == null)
        {
            GUILayout.Label("EditorSession is null.");
        }
        else
        {
            EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;

            GUILayout.Label($"LevelName: {editorSession.LevelName ?? "null"}");
            GUILayout.Label($"SelectedLocationId: {editorSession.SelectedLocationId ?? "null"}");
            GUILayout.Label($"SelectedScenarioId: {editorSession.SelectedScenarioId ?? "null"}");
            GUILayout.Label($"SelectedLevelFilePath: {editorSession.SelectedLevelFilePath ?? "null"}");
        }

        GUILayout.Space(5);

        if (GUILayout.Button(visible ? "Hide" : "Show"))
        {
            ToggleVisibility();
        }

        GUILayout.EndVertical();

        if (Event.current.type == EventType.Repaint)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            windowRect.height = lastRect.yMax + 10f; // padding
        }

        GUI.DragWindow();
    }

    public void ToggleVisibility()
    {
        visible = !visible;
    }
}
