using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour, IControllable
{
    private Rigidbody rigidBody;

    [System.Serializable]
    public class Blade
    {
        public Transform bone;
        public bool clockwise;
        [HideInInspector] public float CurrentRPM;
        [HideInInspector] public Vector3 relativePos;
    }

    [Header("Blades")]
    public Blade[] blades = new Blade[4];

    [Header("Physics Settings")]
    public float liftCoefficient = 1e-6f;
    public float dragCoefficient = 1e-6f;
    public float maxBladeRPM = 8000f;
    public float BladeRPMChangeRate = 0.5f;

    private float throttleInput;
    private float yawInput;
    private float pitchInput;
    private float rollInput;

    public float ThrottleInput => throttleInput;
    public float YawInput => yawInput;
    public float PitchInput => pitchInput;
    public float RollInput => rollInput;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

        if (rigidBody == null)
        {
            Debug.LogError($"[DroneController] Rigidbody is missing on '{gameObject.name}'.");
            enabled = false;
            return;
        }

        if (blades == null || blades.Length == 0)
        {
            Debug.LogError($"[DroneController] No blades assigned on '{gameObject.name}'.");
            enabled = false;
            return;
        }

        foreach (var blade in blades)
        {
            if (blade == null || blade.bone == null)
            {
                Debug.LogError($"[DroneController] One of the blades on '{gameObject.name}' is not configured correctly.");
                enabled = false;
                return;
            }

            blade.relativePos = transform.InverseTransformPoint(blade.bone.position);
        }
    }

    void Start()
    {
    }

    void Update()
    {
        //Debug.Log($"{throttleInput}, {yawInput}, {pitchInput}, {rollInput}");
        //Debug.Log(blades[0].rpm);
        foreach (var blade in blades)
        {
            float direction = blade.clockwise ? 1f : -1f;
            blade.bone.Rotate(360f * blade.CurrentRPM * direction * Vector3.up / 60f * Time.deltaTime, Space.Self);
        }
    }

    void FixedUpdate()
    {
        UpdateMotorSpeeds();
        ApplyBladeForces();
    }

    public void ApplyThrottle(float value)
    {
        throttleInput = value;
    }

    public void ApplyYaw(float value)
    {
        yawInput = value;
    }

    public void ApplyPitch(float value)
    {
        pitchInput = value;
    }

    public void ApplyRoll(float value)
    {
        rollInput = value;
    }

    private void UpdateMotorSpeeds()
    {
        foreach (var blade in blades)
        {
            blade.CurrentRPM = Mathf.Lerp(blade.CurrentRPM, throttleInput * maxBladeRPM, BladeRPMChangeRate * Time.fixedDeltaTime);
        }

        // Добавляем yaw за счёт изменения пар винтов
        // CW ускоряются, CCW замедляются
        //foreach (var blade in blades)
        //{
        //    float yawEffect = yawInput * 0.3f * maxBladeRPM;
        //    if (blade.clockwise) blade.rpm += yawEffect;
        //    else blade.rpm -= yawEffect;
        //}

        // Roll и Pitch: балансируем тягу по направлениям
        // Принятое распределение (для удобства):
        // rotors[0] — front-left
        // rotors[1] — front-right
        // rotors[2] — back-right
        // rotors[3] — back-left

        //if (blades.Length == 4)
        //{
        //    blades[0].rpm -= rollInput * 0.5f * maxBladeRPM + pitchInput * 0.5f * maxBladeRPM; // front-left
        //    blades[1].rpm += rollInput * 0.5f * maxBladeRPM - pitchInput * 0.5f * maxBladeRPM; // front-right
        //    blades[2].rpm += rollInput * 0.5f * maxBladeRPM + pitchInput * 0.5f * maxBladeRPM; // back-right
        //    blades[3].rpm -= rollInput * 0.5f * maxBladeRPM - pitchInput * 0.5f * maxBladeRPM; // back-left
        //}

        // RPM limit
        foreach (var blade in blades)
        {
            blade.CurrentRPM = Mathf.Clamp(blade.CurrentRPM, 0, maxBladeRPM);
        }
    }

    private void ApplyBladeForces()
    {
        //float lift = liftCoefficient * blades[3].rpm /** blades[0].rpm*/* 500;
        //Vector3 liftForce = transform.up * lift;
        //rigidbody.AddForceAtPosition(liftForce, blades[3].bone.position, ForceMode.Force);

        foreach (Blade blade in blades)
        {
            float lift = liftCoefficient * blade.CurrentRPM * blade.CurrentRPM;
            Vector3 liftForce = transform.up * lift;

            rigidBody.AddForceAtPosition(liftForce, blade.bone.position, ForceMode.Force);

            // Реактивный момент (yaw)
            //float torque = dragCoefficient * blade.rpm * blade.rpm * (blade.clockwise ? 1f : -1f);
            //rigidbody.AddTorque(transform.up * torque, ForceMode.Force);
        }
    }
}
