using UnityEngine;

public class DroneInput : MonoBehaviour
{
    private Input input;
    private DroneControlAuthority droneControlAuthority;
    private DroneCameraSwitcher cameraSwitcher;
    private DroneDebugUI debugUI;

    void Awake()
    {
        input = new Input();

        droneControlAuthority = GetComponent<DroneControlAuthority>();
        if (droneControlAuthority == null)
        {
            Debug.LogError($"[DroneInput] There is no DroneControlAuthority component on the object {gameObject.name}.");
        }

        cameraSwitcher = GetComponent<DroneCameraSwitcher>();
        if (cameraSwitcher == null)
        {
            Debug.LogError($"[DroneInput] There is no DroneCameraSwitcher component on the object {gameObject.name}.");
        }

        debugUI = GetComponent<DroneDebugUI>();
        if (debugUI == null)
        {
            Debug.LogError($"[DroneInput] There is no DroneDebugUI component on the object {gameObject.name}.");
        }
    }

    private void OnEnable()
    {
        input.Enable();
        input.DroneControl.DebugUI.performed += _ => debugUI.ToggleVisibility();
        input.DroneControl.SwitchCamera.performed += _ => cameraSwitcher.NextCamera();
    }

    private void OnDisable()
    {
        input.DroneControl.DebugUI.performed -= _ => debugUI.ToggleVisibility();
        input.DroneControl.SwitchCamera.performed -= _ => cameraSwitcher.NextCamera();

        input.Disable();
    }

    void Update()
    {
        Vector2 throttleAndYaw = input.DroneControl.ThrottleAndYaw.ReadValue<Vector2>();
        Vector2 pitchAndRoll = input.DroneControl.PitchAndRoll.ReadValue<Vector2>();

        droneControlAuthority.ApplyManual(throttleAndYaw.y, throttleAndYaw.x, pitchAndRoll.y, pitchAndRoll.x);
    }
}
