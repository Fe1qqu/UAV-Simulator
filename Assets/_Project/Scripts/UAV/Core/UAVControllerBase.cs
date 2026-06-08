using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UAVControllerBase : MonoBehaviour, IControllable, IUAVActor
{
    public event Action<Collision> CollisionHappened;
    public event Action Exploded;

    public UAVControllerBase Controller => this;

    public abstract float ThrottleInput { get; }
    public abstract float YawInput { get; }
    public abstract float PitchInput { get; }
    public abstract float RollInput { get; }

    private readonly Dictionary<Renderer, bool> rendererStates = new();
    private readonly Dictionary<Collider, bool> colliderStates = new();

    /// <summary>
    /// Fully resets the uav runtime state and teleports it to the specified transform.
    /// 
    /// Implementation MUST:
    /// - Reset all control inputs
    /// - Reset internal simulation state (RPM, target rotations, etc.)
    /// - Reset Rigidbody state (position, rotation, velocities)
    /// - Synchronize any cached target state with the provided rotation
    /// 
    /// This method is the only valid way to externally reposition the uav during runtime.
    /// </summary>
    public abstract void ResetState(Vector3 position, Quaternion rotation);

    public virtual void Apply(float throttle, float yaw, float pitch, float roll) { }

    /// <summary>
    /// May be null if the uav has no rotors.
    /// </summary>
    public virtual IReadOnlyDictionary<string, float> DebugRotorRPMs => null;

    protected virtual void Awake()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            rendererStates[renderer] = renderer.enabled;
        }

        foreach (Collider collider in GetComponentsInChildren<Collider>(true))
        {
            colliderStates[collider] = collider.enabled;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;

        //Debug.Log($"[UAVControllerBase] Impact: {impact}. ");

        CollisionHappened?.Invoke(collision);
    }

    public void Explode()
    {
        Debug.Log("[UAVControllerBase] Exploded.");

        Exploded?.Invoke();
    }

    public void Hide()
    {
        foreach (var keyValuePair in rendererStates)
        {
            if (keyValuePair.Key != null)
            {
                keyValuePair.Key.enabled = false;
            }
        }

        foreach (var keyValuePair in colliderStates)
        {
            if (keyValuePair.Key != null)
            {
                keyValuePair.Key.enabled = false;
            }
        }
    }

    public void Show()
    {
        foreach (var keyValuePair in rendererStates)
        {
            if (keyValuePair.Key != null)
            {
                keyValuePair.Key.enabled = keyValuePair.Value;
            }
        }

        foreach (var keyValuePair in colliderStates)
        {
            if (keyValuePair.Key != null)
            {
                keyValuePair.Key.enabled = keyValuePair.Value;
            }
        }
    }
}
