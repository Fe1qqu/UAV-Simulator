using UnityEngine;

[RequireComponent(typeof(DroneController))]
public class DroneDebugUI : MonoBehaviour
{
    private DroneController droneController;

    private bool visible = true;
    private Rect windowRectangle = new Rect(20, 20, 250, 220);

    private void Awake()
    {
        droneController = GetComponent<DroneController>();

        if (droneController == null)
        {
            Debug.LogError($"[DroneDebugUI] No DroneController found on {gameObject.name}");
            enabled = false;
            return;
        }
    }

    private void OnGUI()
    {
        if (!visible || droneController == null)
        {
            return;
        }

        windowRectangle = GUI.Window(0, windowRectangle, DrawWindow, "Drone Debug Info");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        GUILayout.Label($"Throttle: {droneController.ThrottleInput:F2}");
        GUILayout.Label($"Yaw: {droneController.YawInput:F2}");
        GUILayout.Label($"Pitch: {droneController.PitchInput:F2}");
        GUILayout.Label($"Roll: {droneController.RollInput:F2}");

        if (droneController.blades != null)
        {
            for (int i = 0; i < droneController.blades.Length; i++)
            {
                GUILayout.Label($"Blade {i + 1}: {droneController.blades[i].CurrentRPM:F1} RPM");
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
