// NOTE:
// In state-machine driven UI, typically only one BackHandler is active at a time.
// Stack depth > 1 usually indicates overlay or modal UI (dialogs, popups).

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Text;

public class BackDispatcher : MonoBehaviour
{
    public static BackDispatcher Instance { get; private set; }

    [SerializeField] private bool enableDebugLogging = false;

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

    private void OnCancelPressed(InputAction.CallbackContext _)
    {
        if (handlers.Count == 0)
        {
            Debug.Log("[BackDispatcher] Cancel pressed, but no handler consumed it.");
            return;
        }

        DumpStack("Before Back");

        for (int i = handlers.Count - 1; i >= 0; i--)
        {
            if (handlers[i]?.OnBack() == true)
            {
                DumpStack($"Consumed");
                return;
            }
        }
    }

    public void Register(IBackHandler handler)
    {
        if (!handlers.Contains(handler))
        {
            handlers.Add(handler);
            DumpStack("Register");
        }
    }

    public void Unregister(IBackHandler handler)
    {
        if (handlers.Remove(handler))
        {
            DumpStack("Unregister");
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void DumpStack(string reason)
    {
        if (!enableDebugLogging)
        {
            return;
        }

        StringBuilder stringBuilder = new StringBuilder(256);

        stringBuilder.Append("[BackDispatcher] Stack dump (").Append(reason).Append("): ");

        if (handlers.Count == 0)
        {
            stringBuilder.Append("<empty>");
        }
        else
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(" -> ");
                }

                MonoBehaviour handler = handlers[i] as MonoBehaviour;
                stringBuilder.Append('[').Append(i).Append("] ");

                stringBuilder.Append(handler != null ? handler.name : handlers[i]?.ToString() ?? "<null>");
            }
        }

        Debug.Log(stringBuilder.ToString());
    }
}
