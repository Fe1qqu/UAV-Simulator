using UnityEngine;
using System;

public class EditorInput : MonoBehaviour
{
    public event Action Delete;

    private Input input;

    private void Awake()
    {
        input = new Input();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Editor.Delete.performed += _ => Delete?.Invoke();
    }

    private void OnDisable()
    {
        input.Editor.Delete.performed -= _ => Delete?.Invoke();
        input.Disable();
    }
}
