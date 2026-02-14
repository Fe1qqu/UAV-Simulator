using UnityEngine;
using System.Collections.Generic;

public abstract class DroneControllerBase : MonoBehaviour, IDroneActor
{
    public DroneControllerBase Controller => this;

    public abstract float ThrottleInput { get; }
    public abstract float YawInput { get; }
    public abstract float PitchInput { get; }
    public abstract float RollInput { get; }

    /// <summary>
    /// Called when level is restarted. Must fully reset drone runtime state.
    /// </summary>
    public abstract void ResetState();

    /// <summary>
    /// May be null if the drone has no rotors.
    /// </summary>
    public virtual IReadOnlyDictionary<string, float> DebugRotorRPMs => null;
}
