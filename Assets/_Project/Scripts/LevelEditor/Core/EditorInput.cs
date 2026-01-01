using UnityEngine;
using System;

public class EditorInput : MonoBehaviour
{
    public event Action DeleteSelected;

    private Input input;

    private void Awake()
    {
        input = new Input();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Editor.DeleteSelected.performed += _ => DeleteSelected?.Invoke();
    }

    private void OnDisable()
    {
        input.Editor.DeleteSelected.performed -= _ => DeleteSelected?.Invoke();
        input.Disable();
    }
}
