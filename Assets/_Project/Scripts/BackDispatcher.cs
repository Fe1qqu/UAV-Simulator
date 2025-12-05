using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BackDispatcher : MonoBehaviour
{
    public static BackDispatcher Instance { get; private set; }

    private Input input;
    private readonly List<IBackHandler> handlers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[BackDispatcher] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        input = new Input();
        input.UI.Cancel.performed += OnCancelPressed;
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void OnCancelPressed(InputAction.CallbackContext callbackContext)
    {
        for (int i = handlers.Count - 1; i >= 0; i--)
        {
            if (handlers[i]?.OnBack() == true)
            {
                return;
            }
        }

        Debug.Log("[BackDispatcher] Cancel pressed, but no handler consumed it.");
    }

    public void Register(IBackHandler handler)
    {
        if (!handlers.Contains(handler))
        {
            handlers.Add(handler);
        }
    }

    public void Unregister(IBackHandler handler)
    {
        handlers.Remove(handler);
    }
}
