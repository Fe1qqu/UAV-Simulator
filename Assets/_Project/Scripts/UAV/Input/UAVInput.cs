using UnityEngine;
using UnityEngine.InputSystem;

public class UAVInput : MonoBehaviour
{
    private Input input;
    private UAVControlAuthority uavControlAuthority;
    private UAVCameraSwitcher cameraSwitcher;
    private UAVDebugUI debugUI;

    void Awake()
    {
        input = InputModeController.Instance.Input;

        uavControlAuthority = GetComponent<UAVControlAuthority>();
        if (uavControlAuthority == null)
        {
            Debug.LogError($"[UAVInput] There is no UAVControlAuthority component on the object {gameObject.name}.");
        }

        cameraSwitcher = GetComponent<UAVCameraSwitcher>();
        if (cameraSwitcher == null)
        {
            Debug.LogError($"[UAVInput] There is no UAVCameraSwitcher component on the object {gameObject.name}.");
        }

        debugUI = GetComponent<UAVDebugUI>();
        if (debugUI == null)
        {
            Debug.LogError($"[UAVInput] There is no UAVDebugUI component on the object {gameObject.name}.");
        }
    }

    private void OnEnable()
    {
        input.UAVControl.DebugUI.performed += OnDebugUI;
        input.UAVControl.SwitchCamera.performed += OnSwitchCamera;
    }

    private void OnDisable()
    {
        input.UAVControl.DebugUI.performed -= OnDebugUI;
        input.UAVControl.SwitchCamera.performed -= OnSwitchCamera;
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
        Vector2 throttleAndYaw = input.UAVControl.ThrottleAndYaw.ReadValue<Vector2>();
        Vector2 pitchAndRoll = input.UAVControl.PitchAndRoll.ReadValue<Vector2>();

        uavControlAuthority.ApplyManual(throttleAndYaw.y, throttleAndYaw.x, pitchAndRoll.y, pitchAndRoll.x);
    }
}
