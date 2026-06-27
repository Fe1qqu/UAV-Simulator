using UnityEngine;
using UnityEngine.InputSystem;

public class UAVInput : MonoBehaviour
{
    private Input input;
    private UAVControlAuthority uavControlAuthority;
    private UAVCameraSwitcher cameraSwitcher;
    private UAVDebugUI debugUI;

    // Ссылка на контроллер квадрокоптера для смены мода
    private QuadcopterController quadcopterController;

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

        // QuadcopterController может быть дочерним объектом — ищем в иерархии
        quadcopterController = GetComponentInChildren<QuadcopterController>();
        if (quadcopterController == null)
        {
            Debug.LogWarning($"[UAVInput] QuadcopterController not found on '{gameObject.name}' or its children. FlightMode switching will not work.");
        }
    }

    private void OnEnable()
    {
        input.UAVControl.DebugUI.performed += OnDebugUI;
        input.UAVControl.SwitchCamera.performed += OnSwitchCamera;
        input.UAVControl.FlightMode.performed += OnFlightMode;
    }

    private void OnDisable()
    {
        input.UAVControl.DebugUI.performed -= OnDebugUI;
        input.UAVControl.SwitchCamera.performed -= OnSwitchCamera;
        input.UAVControl.FlightMode.performed -= OnFlightMode;
    }

    private void OnDebugUI(InputAction.CallbackContext _)
    {
        debugUI.ToggleVisibility();
    }

    private void OnSwitchCamera(InputAction.CallbackContext _)
    {
        cameraSwitcher.NextCamera();
    }

    private void OnFlightMode(InputAction.CallbackContext _)
    {
        if (quadcopterController == null) return;
        quadcopterController.CycleFlightMode();
    }

    void Update()
    {
        Vector2 throttleAndYaw = input.UAVControl.ThrottleAndYaw.ReadValue<Vector2>();
        Vector2 pitchAndRoll = input.UAVControl.PitchAndRoll.ReadValue<Vector2>();

        uavControlAuthority.ApplyManual(throttleAndYaw.y, throttleAndYaw.x, pitchAndRoll.y, pitchAndRoll.x);
    }
}
