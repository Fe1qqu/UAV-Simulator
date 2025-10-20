using UnityEngine;

[RequireComponent(typeof(DroneController))]
public class DroneDebugUI : MonoBehaviour
{
    private DroneController droneController;

    private bool visible = true;
    private Rect windowRectangle = new Rect(20, 20, 250, 240);

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
        GUILayout.Space(5);

        GUILayout.Label("<b>Blade RPMs</b>");

        if (droneController != null)
        {
            foreach (var (pos, blade) in droneController.Blades)
            {
                if (blade != null)
                {
                    GUILayout.Label($"{pos}: {blade.CurrentRPM:F0} RPM");
                }
                else
                {
                    GUILayout.Label($"{pos}: (missing)");
                }
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
