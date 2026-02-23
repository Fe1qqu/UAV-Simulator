using UnityEngine;
using System.Collections.Generic;

public abstract class DroneControllerBase : MonoBehaviour, IControllable, IDroneActor
{
    public DroneControllerBase Controller => this;

    public abstract float ThrottleInput { get; }
    public abstract float YawInput { get; }
    public abstract float PitchInput { get; }
    public abstract float RollInput { get; }

    /// <summary>
    /// Fully resets the drone runtime state and teleports it to the specified transform.
    /// 
    /// Implementation MUST:
    /// - Reset all control inputs
    /// - Reset internal simulation state (RPM, target rotations, etc.)
    /// - Reset Rigidbody state (position, rotation, velocities)
    /// - Synchronize any cached target state with the provided rotation
    /// 
    /// This method is the only valid way to externally reposition the drone during runtime.
    /// </summary>
    public abstract void ResetState(Vector3 position, Quaternion rotation);

    public virtual void Apply(float throttle, float yaw, float pitch, float roll) { }

    /// <summary>
    /// May be null if the drone has no rotors.
    /// </summary>
    public virtual IReadOnlyDictionary<string, float> DebugRotorRPMs => null;
}
