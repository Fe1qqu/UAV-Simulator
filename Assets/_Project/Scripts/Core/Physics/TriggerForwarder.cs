using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public sealed class TriggerForwarder : MonoBehaviour
{
    [SerializeField]
    private List<MonoBehaviour> receivers = new();

    private List<ITriggerReceiver> cachedReceivers;

    private void Awake()
    {
        if (receivers == null || receivers.Count == 0)
        {
            Debug.LogError($"[TriggerForwarder] No receivers assigned on {name}.");
            return;
        }

        cachedReceivers = new List<ITriggerReceiver>(receivers.Count);

        foreach (MonoBehaviour monoBehaviour in receivers)
        {
            if (monoBehaviour is ITriggerReceiver receiver)
            {
                cachedReceivers.Add(receiver);
            }
            else
            {
                Debug.LogError($"[TriggerForwarder] {monoBehaviour.name} does not implement ITriggerReceiver on {name}.");
            }
        }

        if (cachedReceivers.Count == 0)
        {
            Debug.LogError($"[TriggerForwarder] No valid ITriggerReceiver found on {name}.");
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        for (int i = 0; i < cachedReceivers.Count; i++)
        {
            cachedReceivers[i].OnTriggerEntered(collider);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        for (int i = 0; i < cachedReceivers.Count; i++)
        {
            cachedReceivers[i].OnTriggerExited(collider);
        }
    }
}
