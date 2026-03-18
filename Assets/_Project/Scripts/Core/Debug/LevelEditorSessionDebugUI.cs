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

        if (GameSession.Instance == null || GameSession.Instance.LevelEditor == null)
        {
            GUILayout.Label("LevelEditorSession is null.");
        }
        else
        {
            LevelEditorSession levelEditorSession = GameSession.Instance.LevelEditor;

            GUILayout.Label($"LevelName: {levelEditorSession.LevelName ?? "null"}");
            GUILayout.Label($"LocationId: {levelEditorSession.LocationId ?? "null"}");
            GUILayout.Label($"ScenarioId: {levelEditorSession.ScenarioId ?? "null"}");
            GUILayout.Label($"LevelFilePath: {levelEditorSession.LevelFilePath ?? "null"}");
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
