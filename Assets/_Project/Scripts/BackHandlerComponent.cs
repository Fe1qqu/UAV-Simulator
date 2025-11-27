using UnityEngine;

[RequireComponent(typeof(IBackHandler))]
public class BackHandlerComponent : MonoBehaviour
{
    private IBackHandler handler;

    private void Awake()
    {
        handler = GetComponent<IBackHandler>();
    }

    private void OnEnable()
    {
        if (handler != null)
        {
            BackDispatcher.Instance.Register(handler);
        }
    }

    private void OnDisable()
    {
        if (handler != null)
        {
            BackDispatcher.Instance.Unregister(handler);
        }
    }
}
