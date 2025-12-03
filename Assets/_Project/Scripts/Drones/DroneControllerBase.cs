using UnityEngine;
using System.Collections.Generic;

public abstract class DroneControllerBase : MonoBehaviour
{
    public abstract float ThrottleInput { get; }
    public abstract float YawInput { get; }
    public abstract float PitchInput { get; }
    public abstract float RollInput { get; }

    /// <summary>
    /// May be null if the drone has no rotors.
    /// </summary>
    public virtual IReadOnlyDictionary<string, float> DebugRotorRPMs => null;
}
