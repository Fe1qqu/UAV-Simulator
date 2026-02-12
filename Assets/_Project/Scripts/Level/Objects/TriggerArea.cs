using UnityEngine;
using System;

public class TriggerArea : LevelObject, ITriggerReceiver
{
    public event Action<Collider> ObjectEntered;
    public event Action<Collider> ObjectExited;

    public void OnTriggerEntered(Collider collider)
    {
        ObjectEntered?.Invoke(collider);
    }

    public void OnTriggerExited(Collider collider)
    {
        ObjectExited?.Invoke(collider);
    }
}
