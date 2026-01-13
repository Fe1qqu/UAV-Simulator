using UnityEngine;
using System;
using System.Collections.Concurrent;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> actions = new();

    private static UnityMainThreadDispatcher instance;

    public static bool IsInitialized => instance != null;

    public static void Enqueue(Action action)
    {
        if (action == null)
        {
            return;
        }

        if (instance == null)
        {
            throw new InvalidOperationException("UnityMainThreadDispatcher is not initialized. Ensure it exists in the scene.");
        }

        actions.Enqueue(action);
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        while (actions.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }
}
