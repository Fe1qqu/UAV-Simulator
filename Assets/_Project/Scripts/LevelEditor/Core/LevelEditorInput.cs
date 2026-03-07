using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class LevelEditorInput : MonoBehaviour
{
    public event Action DeleteRequested;

    private Input input;
    private InputAction deleteAction;

    private void Awake()
    {
        input = InputModeController.Instance.Input;
        deleteAction = input.LevelEditor.Delete;
    }

    private void OnEnable()
    {
        deleteAction.performed += OnDelete;
    }

    private void OnDisable()
    {
        deleteAction.performed -= OnDelete;
    }

    private void OnDelete(InputAction.CallbackContext _)
    {
        DeleteRequested?.Invoke();
    }
}
