using UnityEngine;

public class DroneInput : MonoBehaviour
{
    private Input input;
    private IControllable controllable;
    private DroneCameraSwitcher cameraSwitcher;
    private DroneDebugUI debugUI;

    void Awake()
    {
        input = new Input();
        
        controllable = GetComponent<IControllable>();
        if (controllable == null)
        {
            Debug.LogError($"[DroneInput] There is no IControllable component on the object {gameObject.name}");
        }

        cameraSwitcher = GetComponent<DroneCameraSwitcher>();
        if (cameraSwitcher == null)
        {
            Debug.LogError($"[DroneInput] There is no DroneCameraSwitcher component on the object {gameObject.name}");
        }

        debugUI = GetComponent<DroneDebugUI>();
        if (debugUI == null)
        {
            Debug.LogError($"[DroneInput] There is no DroneDebugUI component on the object {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        input.Enable();
        input.DroneControl.DebugUI.performed += context => debugUI.ToggleVisibility();
        input.DroneControl.SwitchCamera.performed += context => cameraSwitcher.NextCamera();
    }

    private void OnDisable()
    {
        input.DroneControl.DebugUI.performed -= context => debugUI.ToggleVisibility();
        input.DroneControl.SwitchCamera.performed -= context => cameraSwitcher.NextCamera();

        input.Disable();
    }

    void Update()
    {
        Vector2 throttleAndYaw = input.DroneControl.ThrottleAndYaw.ReadValue<Vector2>();
        Vector2 pitchAndRoll = input.DroneControl.PitchAndRoll.ReadValue<Vector2>();

        controllable.ApplyThrottle(throttleAndYaw.y);
        controllable.ApplyYaw(throttleAndYaw.x);
        controllable.ApplyPitch(pitchAndRoll.y);
        controllable.ApplyRoll(pitchAndRoll.x);
    }
}
