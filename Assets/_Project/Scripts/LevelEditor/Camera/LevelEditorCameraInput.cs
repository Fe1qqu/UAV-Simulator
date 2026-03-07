using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class LevelEditorCameraInput : MonoBehaviour
{
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

    private void Awake()
    {
        input = InputModeController.Instance.Input;

        lookAction = input.LevelEditorCamera.Look;
        moveAction = input.LevelEditorCamera.Move;
        upDownAction = input.LevelEditorCamera.UpDown;
        enableMovementAction = input.LevelEditorCamera.EnableMovement;
    }

    private void OnEnable()
    {
        enableMovementAction.performed += OnMovementStarted;
        enableMovementAction.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        enableMovementAction.performed -= OnMovementStarted;
        enableMovementAction.canceled -= OnMovementCanceled;
    }

    private void OnMovementStarted(InputAction.CallbackContext _)
    {
        SetMovementEnabled(true);
    }

    private void OnMovementCanceled(InputAction.CallbackContext _)
    {
        SetMovementEnabled(false);
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
