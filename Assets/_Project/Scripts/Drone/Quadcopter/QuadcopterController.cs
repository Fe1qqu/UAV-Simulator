using UnityEngine;
using System;
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

public class QuadcopterController : DroneControllerBase
{
    private Rigidbody rigidBody;
    private Quaternion targetRotation;

    [Header("Rotors")]
    [SerializeField] private Rotor frontLeft;
    [SerializeField] private Rotor frontRight;
    [SerializeField] private Rotor rearLeft;
    [SerializeField] private Rotor rearRight;

    [Header("Physics Settings")]
    [SerializeField] private float liftCoefficient = 1e-6f;
    //[SerializeField] private float dragCoefficient = 1e-6f;
    [SerializeField] private float maxRotorRPM = 4000f;
    [SerializeField] private float rotorRPMChangeRate = 0.5f;

    [Header("Flight Controls")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float rotationLerpSpeed = 10f;

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

    protected override void Awake()
    {
        base.Awake();

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

        foreach (var keyValuePair in rotors)
        {
            Rotor rotor = keyValuePair.Value;
            if (rotor == null || rotor.bone == null)
            {
                Debug.LogError($"[QuadcopterController] Rotor {keyValuePair.Key} not configured correctly.");
                enabled = false;
                return;
            }
        }

        // Initialize target rotation
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // Rotor animation
        foreach (var keyValuePair in rotors)
        {
            Rotor rotor = keyValuePair.Value;
            float direction = rotor.clockwise ? 1f : -1f;
            rotor.bone.Rotate(360f * rotor.CurrentRPM * direction * Vector3.up / 60f * Time.deltaTime, Space.Self);
        }
    }

    void FixedUpdate()
    {
        UpdateMotorSpeeds();
        ApplyRotorForces();

        // Apply rotation control only when there's input
        if (HasRotationInput())
        {
            UpdateTargetRotation();
        }

        // Smoothly rotate towards target rotation
        ApplyRotationControl();
    }

    private void LateUpdate()
    {
        foreach (var keyValuePair in rotors)
        {
            rpmDebugDict[keyValuePair.Key.ToString()] = keyValuePair.Value.CurrentRPM;
        }
    }

    private bool HasRotationInput()
    {
        return Mathf.Abs(yawInput) > 0.01f ||
               Mathf.Abs(pitchInput) > 0.01f ||
               Mathf.Abs(rollInput) > 0.01f;
    }

    private void UpdateTargetRotation()
    {
        // Calculate rotation deltas based on input
        float yawRotation = yawInput * rotationSpeed * Time.fixedDeltaTime;
        float pitchRotation = pitchInput * rotationSpeed * Time.fixedDeltaTime;
        float rollRotation = rollInput * rotationSpeed * Time.fixedDeltaTime;

        // Apply rotations to target
        targetRotation *= Quaternion.Euler(pitchRotation, 0f, -rollRotation);
        targetRotation *= Quaternion.Euler(0f, yawRotation, 0f);
    }

    private void ApplyRotationControl()
    {
        // Smoothly interpolate to target rotation
        Quaternion newRotation = Quaternion.Slerp(
            rigidBody.rotation,
            targetRotation,
            rotationLerpSpeed * Time.fixedDeltaTime
        );

        rigidBody.MoveRotation(newRotation);
    }

    private void UpdateMotorSpeeds()
    {
        // Calculate the target RPM for each rotor
        float frontLeftTarget = throttleInput - 0.4f * yawInput - 0.2f * pitchInput + 0.3f * rollInput;
        float frontRightTarget = throttleInput + 0.4f * yawInput - 0.2f * pitchInput - 0.3f * rollInput;
        float rearLeftTarget = throttleInput + 0.4f * yawInput + 0.2f * pitchInput + 0.3f * rollInput;
        float rearRightTarget = throttleInput - 0.4f * yawInput + 0.2f * pitchInput - 0.3f * rollInput;

        // Apply target RPM to each rotor
        frontLeft.CurrentRPM = Mathf.Lerp(
            frontLeft.CurrentRPM,
            frontLeftTarget * maxRotorRPM,
            rotorRPMChangeRate * Time.fixedDeltaTime
        );

        frontRight.CurrentRPM = Mathf.Lerp(
            frontRight.CurrentRPM,
            frontRightTarget * maxRotorRPM,
            rotorRPMChangeRate * Time.fixedDeltaTime
        );

        rearLeft.CurrentRPM = Mathf.Lerp(
            rearLeft.CurrentRPM,
            rearLeftTarget * maxRotorRPM,
            rotorRPMChangeRate * Time.fixedDeltaTime
        );

        rearRight.CurrentRPM = Mathf.Lerp(
            rearRight.CurrentRPM,
            rearRightTarget * maxRotorRPM,
            rotorRPMChangeRate * Time.fixedDeltaTime
        );

        // Limiting RPM
        foreach (Rotor rotor in rotors.Values)
        {
            rotor.CurrentRPM = Mathf.Clamp(rotor.CurrentRPM, 0, maxRotorRPM);
        }
    }

    private void ApplyRotorForces()
    {
        // Apply lift only to the throttle
        foreach (Rotor rotor in rotors.Values)
        {
            float lift = liftCoefficient * rotor.CurrentRPM * rotor.CurrentRPM;
            Vector3 liftForce = transform.up * lift;
            rigidBody.AddForceAtPosition(liftForce, rotor.bone.position, ForceMode.Force);
        }
    }

    public override void Apply(float throttle, float yaw, float pitch, float roll)
    {
        throttleInput = Mathf.Clamp01(throttle);
        yawInput = Mathf.Clamp(yaw, -1f, 1f);
        pitchInput = Mathf.Clamp(pitch, -1f, 1f);
        rollInput = Mathf.Clamp(roll, -1f, 1f);
    }

    public override void ResetState(Vector3 position, Quaternion rotation)
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

        // RigidBody reset
        if (rigidBody != null)
        {
            rigidBody.position = position;
            rigidBody.rotation = rotation;

            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        // Reset target rotation
        targetRotation = rotation;

        // Clearing debug data
        rpmDebugDict.Clear();
    }
}
