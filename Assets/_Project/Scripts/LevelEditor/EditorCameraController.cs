using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EditorCameraController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Base movement speed of the camera.")]
    public float moveSpeed = 50f;

    [Tooltip("Multiplier applied to movement speed when fast mode is active.")]
    public float fastMoveMultiplier = 3f;

    [Tooltip("Smooth time for camera movement interpolation.")]
    public float moveSmoothTime = 0.1f;

    [Header("Rotation")]
    [Tooltip("Sensitivity of mouse look movement.")]
    public float lookSensitivity = 10f;

    [Tooltip("Smooth time for camera rotation interpolation.")]
    public float rotationSmoothTime = 0.05f;

    [Tooltip("Minimum pitch angle the camera can rotate to.")]
    public float pitchMin = -80f;

    [Tooltip("Maximum pitch angle the camera can rotate to.")]
    public float pitchMax = 80f;

    private Camera cam;

    // Current pitch rotation value
    private float pitch;

    // Current yaw rotation value
    private float yaw;

    // Target camera movement destination
    private Vector3 targetPosition;

    // Velocity reference for SmoothDamp movement
    private Vector3 moveVelocity;

    // Target yaw rotation before smoothing
    private float targetYaw;

    // Target pitch rotation before smoothing
    private float targetPitch;

    // Velocity reference for yaw smoothing
    private float yawVelocity;

    // Velocity reference for pitch smoothing
    private float pitchVelocity;

    private EditorCameraInput cameraInput;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        cameraInput = GetComponent<EditorCameraInput>();
        if (cameraInput == null)
        {
            Debug.LogWarning("[EditorCameraController] EditorCameraInput component not found on the same GameObject.");
        }
    }

    private void Start()
    {
        Vector3 euler = transform.eulerAngles;
        yaw = targetYaw = euler.y;
        pitch = targetPitch = euler.x;
        targetPosition = transform.position;

        UnlockCursor();
    }

    private void OnEnable()
    {
        if (cameraInput != null)
        {
            cameraInput.MovementEnabledChanged += OnMovementEnabledChanged;
        }
    }

    private void OnDisable()
    {
        if (cameraInput != null)
        {
            cameraInput.MovementEnabledChanged -= OnMovementEnabledChanged;
        }

        UnlockCursor();
    }

    private void OnMovementEnabledChanged(bool enabled)
    {
        if (enabled)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
    }

    private void Update()
    {
        if (!cameraInput.IsMovementEnabled)
        {
            return;
        }

        // Prevent movement while dragging objects.
        if (DragPlacementHandler.Instance != null && DragPlacementHandler.Instance.IsDragging)
        {
            //Debug.Log("[EditorCameraController] Camera movement blocked — drag in progress.");
            return;
        }

        Vector2 lookInput = cameraInput.Look;
        Vector3 moveInput = new Vector3(cameraInput.Move.x, cameraInput.UpDown, cameraInput.Move.y);

        HandleRotation(lookInput);
        HandleMovement(moveInput);
    }

    /// <summary>
    /// Locks the cursor to the center of the screen for free-look.
    /// </summary>
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Releases the cursor and shows it again.
    /// </summary>
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Applies smooth mouse-look rotation to the camera.
    /// </summary>
    private void HandleRotation(Vector2 lookInput)
    {
        targetYaw += lookInput.x * lookSensitivity * Time.deltaTime;
        targetPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, pitchMin, pitchMax);

        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotationSmoothTime, Mathf.Infinity, Time.deltaTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime, Mathf.Infinity, Time.deltaTime);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    /// <summary>
    /// Applies movement to the camera with smooth damping.
    /// </summary>
    private void HandleMovement(Vector3 moveInput)
    {
        float speed = moveSpeed;

        Vector3 move = (transform.right * moveInput.x +
                        Vector3.up * moveInput.y +
                        transform.forward * moveInput.z) * (speed * Time.deltaTime);

        targetPosition += move;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, moveSmoothTime, Mathf.Infinity, Time.deltaTime);
    }
}
