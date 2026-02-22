using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class DroneCameraInput : MonoBehaviour
{
    public static DroneCameraInput Instance { get; private set; }

    private Input input;

    private InputAction lookAction;
    private InputAction enableMovementAction;

    public Vector2 Look => lookAction.ReadValue<Vector2>();

    public bool IsMovementEnabled { get; private set; }
    public event Action<bool> MovementEnabledChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[DroneCameraInput] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        input = new Input();

        lookAction = input.DroneCamera.Look;
        enableMovementAction = input.DroneCamera.EnableMovement;
        enableMovementAction.performed += _ => SetMovementEnabled(true);
        enableMovementAction.canceled += _ => SetMovementEnabled(false);
    }

    private void OnEnable()
    {
        PauseManager.PauseStateChanged += OnPauseChanged;

        if (!PauseManager.IsPaused)
        {
            input.Enable();
        }
    }

    private void OnDisable()
    {
        PauseManager.PauseStateChanged -= OnPauseChanged;
        input.Disable();
    }

    private void OnPauseChanged(bool paused)
    {
        if (paused)
        {
            input.Disable();
            SetMovementEnabled(false);
        }
        else
        {
            input.Enable();
        }
    }

    private void SetMovementEnabled(bool enabled)
    {
        if (IsMovementEnabled == enabled)
        {
            return;
        }

        IsMovementEnabled = enabled;
        MovementEnabledChanged?.Invoke(enabled);
    }
}
