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

/// <summary>
/// Режимы полёта квадрокоптера. Переключается кнопкой M через CycleFlightMode().
/// </summary>
public enum FlightMode
{
    /// <summary>Наклоны ограничены ±maxTiltAngle. Газ ручной.</summary>
    AngleLimited,

    /// <summary>Наклоны без ограничений (акробатика). Газ ручной.</summary>
    AngleUnlimited,

    /// <summary>
    /// Altitude Hold — газ автоматически удерживается на уровне hoverThrottle.
    /// Наклоны ограничены, как в AngleLimited.
    /// </summary>
    AltHold
}

[System.Serializable]
public class Rotor
{
    public Transform bone;
    public bool clockwise;
    [HideInInspector] public float CurrentRPM;
}

public class QuadcopterController : UAVControllerBase
{
    private Rigidbody rigidBody;

    [Header("Rotors")]
    [SerializeField] private Rotor frontLeft;
    [SerializeField] private Rotor frontRight;
    [SerializeField] private Rotor rearLeft;
    [SerializeField] private Rotor rearRight;

    [Header("Physics Settings")]
    [SerializeField] private float liftCoefficient = 1e-6f;
    [SerializeField] private float maxRotorRPM = 3000f;
    [SerializeField] private float rotorRPMChangeRate = 0.5f;

    [Header("Flight Controls")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float yawSpeed = 30f;
    [SerializeField] private float maxTiltAngle = 30f;

    [Header("Flight Modes")]
    [SerializeField] private FlightMode flightMode = FlightMode.AngleLimited;

    /// <summary>
    /// Целевой RPM всех роторов в режиме AltHold.
    /// Задаётся напрямую в оборотах/мин — удобно для расчётов из кода.
    /// Изменить из инспектора: поле hoverRPM (0..maxRotorRPM).
    /// Изменить из кода: присвой <see cref="HoverRPM"/> или вызови <see cref="SetHoverRPM"/>.
    /// </summary>
    [SerializeField] private float hoverRPM = 1560f;

    /// <summary>
    /// Текущий целевой RPM удержания высоты. Можно читать и писать напрямую.
    /// Значение зажимается в [0, maxRotorRPM] при применении.
    /// </summary>
    public float HoverRPM
    {
        get => hoverRPM;
        set => hoverRPM = value;
    }

    /// <summary>
    /// Задаёт целевой RPM удержания высоты. Эквивалентно <c>HoverRPM = rpm</c>.
    /// Удобно для вызова из команд или внешних скриптов.
    /// </summary>
    /// <param name="rpm">Обороты в минуту (0..maxRotorRPM, обычно 0..3000).</param>
    public void SetHoverRPM(float rpm) => hoverRPM = rpm;

    /// <summary>Текущий режим полёта. Только для чтения (например, из UI).</summary>
    public FlightMode CurrentFlightMode => flightMode;

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

    // ── Текущие накопленные углы ──────────────────────────────────────────
    private float currentPitchAngle = 0f;
    private float currentRollAngle  = 0f;
    private float currentYawAngle   = 0f;

    // ── Режим прямого управления углами (используется командами) ─────────
    private bool  _directAngleMode  = false;
    private float _targetPitchAngle = 0f;
    private float _targetRollAngle  = 0f;
    private float _targetYawAngle   = 0f;

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

        rotors[RotorPosition.FrontLeft]  = frontLeft;
        rotors[RotorPosition.FrontRight] = frontRight;
        rotors[RotorPosition.RearLeft]   = rearLeft;
        rotors[RotorPosition.RearRight]  = rearRight;

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

        currentYawAngle   = transform.eulerAngles.y;
        currentPitchAngle = 0f;
        currentRollAngle  = 0f;
    }

    void Update()
    {
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
        UpdateAnglesFromInput();
        ApplyRotationFromAngles();
    }

    private void LateUpdate()
    {
        foreach (var keyValuePair in rotors)
        {
            rpmDebugDict[keyValuePair.Key.ToString()] = keyValuePair.Value.CurrentRPM;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Переключение мода — вызывается из UAVInput по кнопке M
    // ─────────────────────────────────────────────────────────────────────
    public void CycleFlightMode()
    {
        int count = Enum.GetValues(typeof(FlightMode)).Length;
        flightMode = (FlightMode)(((int)flightMode + 1) % count);
        Debug.Log($"[QuadcopterController] Flight mode: {flightMode}");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Накопление углов из input ИЛИ применение прямых углов от команд.
    // ─────────────────────────────────────────────────────────────────────
    private void UpdateAnglesFromInput()
    {
        if (_directAngleMode)
        {
            currentYawAngle   = _targetYawAngle;
            currentPitchAngle = Mathf.MoveTowards(currentPitchAngle, _targetPitchAngle, rotationSpeed * Time.fixedDeltaTime);
            currentRollAngle  = Mathf.MoveTowards(currentRollAngle,  _targetRollAngle,  rotationSpeed * Time.fixedDeltaTime);
            return;
        }

        float dt = Time.fixedDeltaTime;

        currentYawAngle += yawInput * yawSpeed * dt;

        currentPitchAngle += pitchInput * rotationSpeed * dt;
        currentRollAngle  += -rollInput * rotationSpeed * dt;

        // Ограничение наклонов: AngleLimited и AltHold — да, AngleUnlimited — нет
        bool limited = (flightMode == FlightMode.AngleLimited || flightMode == FlightMode.AltHold);
        if (limited)
        {
            currentPitchAngle = Mathf.Clamp(currentPitchAngle, -maxTiltAngle, maxTiltAngle);
            currentRollAngle  = Mathf.Clamp(currentRollAngle,  -maxTiltAngle, maxTiltAngle);
        }
    }

    private void ApplyRotationFromAngles()
    {
        Quaternion yawQ   = Quaternion.AngleAxis(currentYawAngle,   Vector3.up);
        Quaternion pitchQ = Quaternion.AngleAxis(currentPitchAngle, Vector3.right);
        Quaternion rollQ  = Quaternion.AngleAxis(currentRollAngle,  Vector3.forward);

        Quaternion targetRotation = yawQ * pitchQ * rollQ;
        rigidBody.MoveRotation(targetRotation);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Скорости роторов.
    // AltHold: targetRPM берётся напрямую из hoverRPM — задай его из инспектора
    //          или кодом: SetHoverRPM(rpm) / HoverRPM = rpm.
    // Остальные моды: gas = throttleInput [0..1] → rpm = throttleInput * maxRotorRPM.
    // ─────────────────────────────────────────────────────────────────────
    private void UpdateMotorSpeeds()
    {
        float targetRPM = (flightMode == FlightMode.AltHold)
            ? Mathf.Clamp(hoverRPM, 0f, maxRotorRPM)   // ← напрямую в RPM
            : throttleInput * maxRotorRPM;

        frontLeft.CurrentRPM  = Mathf.Lerp(frontLeft.CurrentRPM,  targetRPM, rotorRPMChangeRate * Time.fixedDeltaTime);
        frontRight.CurrentRPM = Mathf.Lerp(frontRight.CurrentRPM, targetRPM, rotorRPMChangeRate * Time.fixedDeltaTime);
        rearLeft.CurrentRPM   = Mathf.Lerp(rearLeft.CurrentRPM,   targetRPM, rotorRPMChangeRate * Time.fixedDeltaTime);
        rearRight.CurrentRPM  = Mathf.Lerp(rearRight.CurrentRPM,  targetRPM, rotorRPMChangeRate * Time.fixedDeltaTime);

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

    public override void Apply(float throttle, float yaw, float pitch, float roll)
    {
        throttleInput = Mathf.Clamp01(throttle);
        yawInput      = Mathf.Clamp(yaw,   -1f, 1f);
        pitchInput    = Mathf.Clamp(pitch, -1f, 1f);
        rollInput     = Mathf.Clamp(roll,  -1f, 1f);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Публичный API для команд с точным углом
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Устанавливает целевые углы напрямую (используется командами для snap и удержания).
    /// pitch и roll зажимаются только до maxTiltAngle если они задаются явно.
    /// yawDeg — накопленный yaw (не нормализованный), float.NaN = не менять.
    /// </summary>
    public void SetTargetAngles(float pitchDeg, float rollDeg, float yawDeg = float.NaN)
    {
        _directAngleMode  = true;
        _targetPitchAngle = Mathf.Clamp(pitchDeg, -maxTiltAngle, maxTiltAngle);
        _targetRollAngle  = Mathf.Clamp(rollDeg,  -maxTiltAngle, maxTiltAngle);
        _targetYawAngle   = float.IsNaN(yawDeg) ? currentYawAngle : yawDeg;
    }

    /// <summary>
    /// Выход из режима прямого управления углами.
    /// Синхронизирует currentYawAngle с _targetYawAngle чтобы после ClearDirectAngleMode
    /// накопленный yaw совпадал с тем, куда повернули дрон (важно для цепочки rotate-команд).
    /// </summary>
    public void ClearDirectAngleMode()
    {
        // Синхронизируем накопленный yaw: если мы делали snap yaw через SetTargetAngles,
        // нужно чтобы currentYawAngle отражало реальное положение после выхода из directMode
        currentYawAngle   = _targetYawAngle;
        currentPitchAngle = _targetPitchAngle;
        currentRollAngle  = _targetRollAngle;

        _directAngleMode = false;
        yawInput   = 0f;
        pitchInput = 0f;
        rollInput  = 0f;
    }

    public float CurrentYawAngle   => currentYawAngle;
    public float CurrentPitchAngle => currentPitchAngle;
    public float CurrentRollAngle  => currentRollAngle;

    public override void ResetState(Vector3 position, Quaternion rotation)
    {
        throttleInput = 0f;
        yawInput      = 0f;
        pitchInput    = 0f;
        rollInput     = 0f;

        _directAngleMode  = false;
        _targetPitchAngle = 0f;
        _targetRollAngle  = 0f;

        Vector3 euler     = rotation.eulerAngles;
        currentYawAngle   = euler.y;
        currentPitchAngle = 0f;
        currentRollAngle  = 0f;
        _targetYawAngle   = euler.y;

        foreach (Rotor rotor in rotors.Values)
        {
            rotor.CurrentRPM = 0f;
        }

        if (rigidBody != null)
        {
            rigidBody.position        = position;
            rigidBody.rotation        = rotation;
            rigidBody.linearVelocity  = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        rpmDebugDict.Clear();
    }
}
