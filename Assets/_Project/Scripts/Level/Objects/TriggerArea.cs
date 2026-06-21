using UnityEngine;
using System;
using System.Collections.Generic;

public class TriggerArea : LevelObject, ITriggerReceiver
{
    protected readonly HashSet<Transform> ObjectsInTrigger = new();

    public event Action<Collider> ObjectEntered;
    public event Action<Collider> ObjectExited;

    public virtual void OnTriggerEntered(Collider collider)
    {
        ObjectEntered?.Invoke(collider);
    }

    public virtual void OnTriggerExited(Collider collider)
    {
        ObjectExited?.Invoke(collider);
    }
}
