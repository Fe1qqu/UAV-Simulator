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
    [SerializeField] private float maxRotorRPM = 4000f;
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

    void Update() //визуальное изменение
    {
        foreach (var kvp in rotors)
        {
            Rotor rotor = kvp.Value;
            float direction = rotor.clockwise ? 1f : -1f;
            rotor.bone.Rotate(360f * rotor.CurrentRPM * direction * Vector3.up / 60f * Time.deltaTime, Space.Self);
        }
    }

    void FixedUpdate() //фактическое изменение
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
        // Вычисляем целевые RPM для каждого ротора по формулам
        float frontLeftTarget = throttleInput - 0.4f * yawInput - 0.2f * pitchInput + 0.3f * rollInput;
        float frontRightTarget = throttleInput + 0.4f * yawInput - 0.2f * pitchInput - 0.3f * rollInput;
        float rearLeftTarget = throttleInput + 0.4f * yawInput + 0.2f * pitchInput + 0.3f * rollInput;
        float rearRightTarget = throttleInput - 0.4f * yawInput + 0.2f * pitchInput - 0.3f * rollInput;

        // Применяем целевые RPM к каждому ротору
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

        // Ограничиваем RPM
        foreach (Rotor rotor in rotors.Values)
        {
            rotor.CurrentRPM = Mathf.Clamp(rotor.CurrentRPM, 0, maxRotorRPM);
        }
    }
    private void ApplyRotorForces()
    {
        // Переменная для суммарного крутящего момента
        Vector3 totalTorque = Vector3.zero;

        foreach (Rotor rotor in rotors.Values)
        {
            // 1. Подъемная сила (оставляем как было)
            float lift = liftCoefficient * rotor.CurrentRPM * rotor.CurrentRPM;
            Vector3 liftForce = transform.up * lift;
            rigidBody.AddForceAtPosition(liftForce, rotor.bone.position, ForceMode.Force);

            // 2. Крутящий момент от ротор
            float torqueCoefficient = liftCoefficient * 0.1f; // Например, 10% от подъемной силы
            float torqueMagnitude = torqueCoefficient * rotor.CurrentRPM * rotor.CurrentRPM;

            // Направление момента зависит от направления вращения
            Vector3 torque = Vector3.zero;
            if (rotor.clockwise)
            {
                torque = transform.up * torqueMagnitude; // По часовой = отрицательный момент
            }
            else
            {
                torque = -transform.up * torqueMagnitude; // Против часовой = положительный момент
            }

            // Добавляем к суммарному моменту
            totalTorque += torque;
        }

        // 3. Применяем суммарный крутящий момент к дрону
        rigidBody.AddTorque(totalTorque, ForceMode.Force);
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

