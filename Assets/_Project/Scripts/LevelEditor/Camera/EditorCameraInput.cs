using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class EditorCameraInput : MonoBehaviour
{
    public static EditorCameraInput Instance { get; private set; }

    // Main input action asset created via the Input System
    private Input input;

    // Action that controls mouse look input
    private InputAction lookAction;

    // Action that controls horizontal movement input
    private InputAction moveAction;

    // Action that controls vertical movement input
    private InputAction upDownAction;

    // Action that controls possibility of movement input
    private InputAction enableMovementAction;

    // Current look delta value
    public Vector2 Look => lookAction.ReadValue<Vector2>();

    // Current movement vector value
    public Vector2 Move => moveAction.ReadValue<Vector2>();

    // Current vertical movement value
    public float UpDown => upDownAction.ReadValue<float>();

    // Movement enabled flag
    public bool IsMovementEnabled { get; private set; }
    public event Action<bool> MovementEnabledChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[EditorCameraInput] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        input = new Input();
        
        lookAction = input.EditorCamera.Look;
        moveAction = input.EditorCamera.Move;
        upDownAction = input.EditorCamera.UpDown;
        enableMovementAction = input.EditorCamera.EnableMovement;
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
