using UnityEngine;

public class LevelEditorSessionDebugUI : MonoBehaviour
{
    private bool visible = true;
    private Rect windowRect = new(20, 20, 300, 150);

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }    

        windowRect = GUI.Window(12345, windowRect, DrawWindow, "Level Editor Session Debug");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        if (GameSettings.Instance == null || GameSettings.Instance.CurrentLevelEditorSession == null)
        {
            GUILayout.Label("LevelEditorSession is null.");
        }
        else
        {
            LevelEditorSession levelEditorSession = GameSettings.Instance.CurrentLevelEditorSession;

            GUILayout.Label($"LevelName: {levelEditorSession.LevelName ?? "null"}");
            GUILayout.Label($"SelectedLocationId: {levelEditorSession.SelectedLocationId ?? "null"}");
            GUILayout.Label($"SelectedScenarioId: {levelEditorSession.SelectedScenarioId ?? "null"}");
            GUILayout.Label($"SelectedLevelFilePath: {levelEditorSession.SelectedLevelFilePath ?? "null"}");
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
