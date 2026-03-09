using UnityEngine;
using UnityEngine.InputSystem;

public class DroneInput : MonoBehaviour
{
    private Input input;
    private DroneControlAuthority droneControlAuthority;
    private DroneCameraSwitcher cameraSwitcher;
    private DroneDebugUI debugUI;

    void Awake()
    {
        input = InputModeController.Instance.Input;

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
        input.DroneControl.DebugUI.performed += OnDebugUI;
        input.DroneControl.SwitchCamera.performed += OnSwitchCamera;
    }

    private void OnDisable()
    {
        input.DroneControl.DebugUI.performed -= OnDebugUI;
        input.DroneControl.SwitchCamera.performed -= OnSwitchCamera;
    }

    private void OnDebugUI(InputAction.CallbackContext _)
    {
        debugUI.ToggleVisibility();
    }

    private void OnSwitchCamera(InputAction.CallbackContext _)
    {
        cameraSwitcher.NextCamera();
    }

    void Update()
    {
        Vector2 throttleAndYaw = input.DroneControl.ThrottleAndYaw.ReadValue<Vector2>();
        Vector2 pitchAndRoll = input.DroneControl.PitchAndRoll.ReadValue<Vector2>();

        droneControlAuthority.ApplyManual(throttleAndYaw.y, throttleAndYaw.x, pitchAndRoll.y, pitchAndRoll.x);
    }
}
