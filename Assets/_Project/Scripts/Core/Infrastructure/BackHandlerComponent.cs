using UnityEngine;

public class BackHandlerComponent : MonoBehaviour
{
    private IBackHandler handler;

    private void Awake()
    {
        handler = GetComponent<IBackHandler>();
        if (handler == null)
        {
            Debug.LogError($"[BackHandlerComponent] {name} missing IBackHandler reference.");
        }
    }

    private void Start()
    {
        //Debug.Log("[BackHandlerComponent] Start for: " + name);

        if (handler != null)
        {
            BackDispatcher.Instance.Register(handler);
        }
    }

    private void OnDestroy()
    {
        if (handler != null)
        {
            BackDispatcher.Instance.Unregister(handler);
        }
    }
}
