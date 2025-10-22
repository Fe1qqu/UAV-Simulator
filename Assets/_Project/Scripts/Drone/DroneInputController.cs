using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DroneController))]
public class DroneInput : MonoBehaviour
{
    private Input input;
    private IControllable controllable;
    private DroneDebugUI debugUI;

    void Awake()
    {
        input = new Input();
        //input.Enable();
        controllable = GetComponent<IControllable>();
        debugUI = GetComponent<DroneDebugUI>();

        if (controllable == null)
        {
            Debug.LogError($"[DroneInput] There is no IControllable component on the object {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        input.Enable();
        input.DroneControl.DebugUI.performed += OnDebugUIToggle;
    }

    private void OnDisable()
    {
        input.DroneControl.DebugUI.performed -= OnDebugUIToggle;
        input.Disable();
    }

    private void OnDebugUIToggle(InputAction.CallbackContext context)
    {
        if (debugUI != null)
        {
            debugUI.ToggleVisibility();
        }
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
