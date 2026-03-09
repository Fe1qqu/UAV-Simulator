// NOTE:
// In state-machine driven UI, typically only one BackHandler is active at a time.
// Stack depth > 1 usually indicates overlay or modal UI (dialogs, popups).

using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class BackDispatcher : MonoBehaviour
{
    public static BackDispatcher Instance { get; private set; }

    [SerializeField] private bool enableDebugLogging = false;

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
        DontDestroyOnLoad(gameObject);
    }

    public static void DispatchBack()
    {
        if (Instance == null)
        {
            Debug.LogWarning("[BackDispatcher] No instance.");
            return;
        }

        Instance.HandleBack();
    }

    private void HandleBack()
    {
        if (handlers.Count == 0)
        {
            Debug.Log("[BackDispatcher] Back pressed, but no handler consumed it.");
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

    private void Register(IBackHandler handler)
    {
        if (!handlers.Contains(handler))
        {
            handlers.Add(handler);
            DumpStack("Register");
        }
    }

    private void Unregister(IBackHandler handler)
    {
        if (handlers.Remove(handler))
        {
            DumpStack("Unregister");
        }
    }

    public static void RegisterHandler(IBackHandler handler)
    {
        if (Instance == null)
        {
            Debug.LogError($"[BackDispatcher] BackDispatcher is null. Cannot register handler: {handler}.");
            return;
        }

        Instance.Register(handler);
    }

    public static void UnregisterHandler(IBackHandler handler)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.Unregister(handler);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void DumpStack(string reason)
    {
        if (!enableDebugLogging)
        {
            return;
        }

        StringBuilder stringBuilder = new(256);

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
