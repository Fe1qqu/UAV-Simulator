using UnityEngine;
using System.Collections.Generic;

public enum RotorPosition
{
    FrontLeft,
    FrontRight,
    RearLeft,
    RearRight
}

[System.Serializable]
public class Rotor
{
    public Transform bone;
    public bool clockwise;
    [HideInInspector] public float CurrentRPM;
}

public class QuadcopterController : DroneControllerBase, IControllable
{
    private Rigidbody rigidBody;

    [Header("Rotors")]
    [SerializeField] private Rotor frontLeft;
    [SerializeField] private Rotor frontRight;
    [SerializeField] private Rotor rearLeft;
    [SerializeField] private Rotor rearRight;

    [Header("Physics Settings")]
    [SerializeField] private float liftCoefficient = 1e-6f;
    [SerializeField] private float dragCoefficient = 1e-6f;
    [SerializeField] private float maxRotorRPM = 8000f;
    [SerializeField] private float rotorRPMChangeRate = 0.5f;

    private Dictionary<RotorPosition, Rotor> rotors = new();

    private Dictionary<string, float> rpmDebugDict = new();
    public override IReadOnlyDictionary<string, float> DebugRotorRPMs => rpmDebugDict;

    private float throttleInput;
    private float yawInput;
    private float pitchInput;
    private float rollInput;

    public override float ThrottleInput => throttleInput;
    public override float YawInput => yawInput;
    public override float PitchInput => pitchInput;
    public override float RollInput => rollInput;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            Debug.LogError($"[QuadcopterController] Rigidbody is missing on '{gameObject.name}'.");
            enabled = false;
            return;
        }

        rotors[RotorPosition.FrontLeft] = frontLeft;
        rotors[RotorPosition.FrontRight] = frontRight;
        rotors[RotorPosition.RearLeft] = rearLeft;
        rotors[RotorPosition.RearRight] = rearRight;

        foreach (var kvp in rotors)
        {
            Rotor rotor = kvp.Value;
            if (rotor == null || rotor.bone == null)
            {
                Debug.LogError($"[QuadcopterController] Rotor {kvp.Key} not configured correctly.");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        foreach (var kvp in rotors)
        {
            Rotor rotor = kvp.Value;
            float direction = rotor.clockwise ? 1f : -1f;
            rotor.bone.Rotate(360f * rotor.CurrentRPM * direction * Vector3.up / 60f * Time.deltaTime, Space.Self);
        }
    }

    void FixedUpdate()
    {
        UpdateMotorSpeeds();
        ApplyRotorForces();
    }

    private void LateUpdate()
    {
        foreach (var kvp in rotors)
        {
            rpmDebugDict[kvp.Key.ToString()] = kvp.Value.CurrentRPM;
        }
    }

    private void UpdateMotorSpeeds()
    {
        foreach (Rotor rotor in rotors.Values)
        {
            rotor.CurrentRPM = Mathf.Lerp(rotor.CurrentRPM, throttleInput * maxRotorRPM, rotorRPMChangeRate * Time.fixedDeltaTime);
        }

        // RPM limit
        foreach (Rotor rotor in rotors.Values)
        {
            rotor.CurrentRPM = Mathf.Clamp(rotor.CurrentRPM, 0, maxRotorRPM);
        }
    }

    private void ApplyRotorForces()
    {
        foreach (Rotor rotor in rotors.Values)
        {
            float lift = liftCoefficient * rotor.CurrentRPM * rotor.CurrentRPM;
            Vector3 liftForce = transform.up * lift;

            rigidBody.AddForceAtPosition(liftForce, rotor.bone.position, ForceMode.Force);
        }
    }

    public void ApplyThrottle(float value) => throttleInput = value;
    public void ApplyYaw(float value) => yawInput = value;
    public void ApplyPitch(float value) => pitchInput = value;
    public void ApplyRoll(float value) => rollInput = value;

    public override void ResetState()
    {
        // Resetting control inputs
        throttleInput = 0f;
        yawInput = 0f;
        pitchInput = 0f;
        rollInput = 0f;

        // Resetting RPM of all rotors
        foreach (Rotor rotor in rotors.Values)
        {
            rotor.CurrentRPM = 0f;
        }

        // Physics reset
        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.Sleep();
        }

        // Clearing debug data
        rpmDebugDict.Clear();
    }
}
