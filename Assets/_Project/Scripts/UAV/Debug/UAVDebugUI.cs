using UnityEngine;

public class UAVDebugUI : MonoBehaviour
{
    private UAVControllerBase uavController;
    private bool visible = true;
    private Rect windowRectangle = new(20, 20, 250, 270);

    private void Awake()
    {
        uavController = GetComponent<UAVControllerBase>();
        if (uavController == null)
        {
            Debug.LogError($"[UAVDebugUI] No UAVControllerBase found on {gameObject.name}.");
            enabled = false;
            return;
        }
    }

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        windowRectangle = GUI.Window(0, windowRectangle, DrawWindow, "UAV Debug Info");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        GUILayout.Label("<b>Input</b>");
        GUILayout.Label($"Throttle: {uavController.ThrottleInput:F2}");
        GUILayout.Label($"Yaw: {uavController.YawInput:F2}");
        GUILayout.Label($"Pitch: {uavController.PitchInput:F2}");
        GUILayout.Label($"Roll: {uavController.RollInput:F2}");
        GUILayout.Space(5);

        var rotors = uavController.DebugRotorRPMs;
        if (rotors != null)
        {
            GUILayout.Label("<b>Rotors</b>");
            foreach (var keyValuePair in rotors)
            {
                GUILayout.Label($"{keyValuePair.Key}: {keyValuePair.Value:F0} RPM");
            }
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    public void ToggleVisibility()
    {
        visible = !visible;
    }
}
