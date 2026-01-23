// NOTE:
// BackHandlerComponent is intended for state-driven UI where the Back context
// is tied to the GameObject's enable/disable lifecycle.
//
// Typical usage assumes that only ONE BackHandler is active at a time.
// Stack depth > 1 is expected only for overlay / modal UI (dialogs, popups).
//
// Do NOT use this component for logic-driven states (Pause, Gameplay, Modes).
// In such cases, BackDispatcher.Register / Unregister should be called manually.

using UnityEngine;

public class BackHandlerComponent : MonoBehaviour
{
    private IBackHandler handler;

    private void Awake()
    {
        handler = GetComponent<IBackHandler>();
        if (handler == null)
        {
            Debug.LogError($"[BackHandlerComponent] GameObject '{name}' has BackHandlerComponent but does not implement IBackHandler.");
        }
    }

    private void OnEnable()
    {
        BackDispatcher.RegisterHandler(handler);
    }

    private void OnDisable()
    {
        BackDispatcher.UnregisterHandler(handler);
    }
}
