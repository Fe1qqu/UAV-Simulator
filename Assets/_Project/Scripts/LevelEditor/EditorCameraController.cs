using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class EditorCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 50f;
    public float fastMoveMultiplier = 3f;
    public float moveSmoothTime = 0.1f;

    [Header("Rotation")]
    public float lookSensitivity = 10f;
    public float rotationSmoothTime = 0.05f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    private Camera cam;
    private float pitch;
    private float yaw;

    private Vector3 targetPosition;
    private Vector3 moveVelocity;

    private float targetYaw;
    private float targetPitch;
    private float yawVelocity;
    private float pitchVelocity;

    private bool isControlling = false;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        Vector3 euler = transform.eulerAngles;
        yaw = targetYaw = euler.y;
        pitch = targetPitch = euler.x;
        targetPosition = transform.position;

        UnlockCursor();
    }

    private void Update()
    {
        HandleMouseControl();

        if (!isControlling)
        {
            return;
        }

        var input = EditorCameraInput.Instance;
        if (input == null)
        {
            return;
        }

        if (DragPlacementHandler.Instance != null && DragPlacementHandler.Instance.IsDragging)
        {
            return;
        }    

        Vector3 moveInput = new Vector3(input.Move.x, input.UpDown, input.Move.y);
        Vector2 lookInput = input.Look;

        HandleRotation(lookInput);
        HandleMovement(moveInput);
    }

    private void HandleMouseControl()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            LockCursor();
            isControlling = true;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            UnlockCursor();
            isControlling = false;
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleRotation(Vector2 lookInput)
    {
        targetYaw += lookInput.x * lookSensitivity * Time.deltaTime;
        targetPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, pitchMin, pitchMax);

        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotationSmoothTime, Mathf.Infinity, Time.deltaTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime, Mathf.Infinity, Time.deltaTime);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement(Vector3 moveInput)
    {
        float speed = moveSpeed;
        //if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
        //    speed *= fastMoveMultiplier;

        Vector3 move = (transform.right * moveInput.x +
                        Vector3.up * moveInput.y +
                        transform.forward * moveInput.z) * (speed * Time.deltaTime);

        targetPosition += move;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, moveSmoothTime, Mathf.Infinity, Time.deltaTime);
    }
}
