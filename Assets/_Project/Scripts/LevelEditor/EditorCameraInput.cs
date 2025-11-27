using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EditorCameraController))]
public class EditorCameraInput : MonoBehaviour
{
    public static EditorCameraInput Instance { get; private set; }

    [Header("Input Actions")]
    [Tooltip("Main input action asset created via the Input System.")]
    private Input input;

    [Tooltip("Action that controls mouse look input.")]
    private InputAction lookAction;

    [Tooltip("Action that controls horizontal movement input.")]
    private InputAction moveAction;

    [Tooltip("Action that controls vertical movement input.")]
    private InputAction upDownAction;

    // Current look delta value
    public Vector2 Look => lookAction.ReadValue<Vector2>();

    // Current movement vector value
    public Vector2 Move => moveAction.ReadValue<Vector2>();

    // Current vertical movement value
    public float UpDown => upDownAction.ReadValue<float>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[EditorCameraInput] Duplicate instance detected. Only one instance is allowed in the scene");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        input = new Input();

        lookAction = input.EditorCamera.Look;
        moveAction = input.EditorCamera.Move;
        upDownAction = input.EditorCamera.UpDown;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }
}
