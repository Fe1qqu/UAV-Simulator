using UnityEngine;
using System;

public class PlayInput : MonoBehaviour
{
    public event Action RestartRequested;

    private Input input;

    private void Awake()
    {
        input = new Input();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Play.RestartLevel.performed += _ => RestartRequested?.Invoke();
    }

    private void OnDisable()
    {
        input.Play.RestartLevel.performed -= _ => RestartRequested?.Invoke();
        input.Disable();
    }
}
