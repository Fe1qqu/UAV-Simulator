using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayInput : MonoBehaviour
{
    public event Action RestartRequested;

    private Input input;

    private void Awake()
    {
        input = InputModeController.Instance.Input;
    }

    private void OnEnable()
    {
        input.Play.RestartLevel.performed += OnRestartLevel;
    }

    private void OnDisable()
    {
        input.Play.RestartLevel.performed -= OnRestartLevel;
    }

    private void OnRestartLevel(InputAction.CallbackContext _)
    {
        RestartRequested?.Invoke();
    }
}
