using UnityEngine;

public interface ITriggerReceiver
{
    void OnTriggerEntered(Collider collider);
    void OnTriggerExited(Collider collider);
}
