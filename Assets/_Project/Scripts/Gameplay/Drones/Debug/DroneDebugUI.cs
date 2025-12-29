using UnityEngine;

public class DroneDebugUI : MonoBehaviour
{
    private DroneControllerBase droneController;
    private bool visible = true;
    private Rect windowRectangle = new Rect(20, 20, 250, 270);

    private void Awake()
    {
        droneController = GetComponent<DroneControllerBase>();
        if (droneController == null)
        {
            Debug.LogError($"[DroneDebugUI] No DroneControllerBase found on {gameObject.name}");
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

        windowRectangle = GUI.Window(0, windowRectangle, DrawWindow, "Drone Debug Info");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        GUILayout.Label("<b>Input</b>");
        GUILayout.Label($"Throttle: {droneController.ThrottleInput:F2}");
        GUILayout.Label($"Yaw: {droneController.YawInput:F2}");
        GUILayout.Label($"Pitch: {droneController.PitchInput:F2}");
        GUILayout.Label($"Roll: {droneController.RollInput:F2}");
        GUILayout.Space(5);

        var rotors = droneController.DebugRotorRPMs;
        if (rotors != null)
        {
            GUILayout.Label("<b>Rotors</b>");
            foreach (var kvp in rotors)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value:F0} RPM");
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
