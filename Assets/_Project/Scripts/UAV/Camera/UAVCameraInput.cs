using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class UAVCameraInput : MonoBehaviour
{
    private Input input;

    private InputAction lookAction;
    private InputAction enableMovementAction;

    public Vector2 Look => lookAction.ReadValue<Vector2>();

    public bool IsMovementEnabled { get; private set; }
    public event Action<bool> MovementEnabledChanged;

    private void Awake()
    {
        input = InputModeController.Instance.Input;

        lookAction = input.UAVCamera.Look;
        enableMovementAction = input.UAVCamera.EnableMovement;
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
