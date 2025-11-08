using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EditorCameraController))]
public class EditorCameraInput : MonoBehaviour
{
    public static EditorCameraInput Instance { get; private set; }

    private Input input;

    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction upDownAction;

    public Vector2 Look => lookAction.ReadValue<Vector2>();
    public Vector2 Move => moveAction.ReadValue<Vector2>();
    public float UpDown => upDownAction.ReadValue<float>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[EditorCameraInput] Duplicate instance detected. There should only be one in the scene.");
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
        //input.DroneControl.DebugUI.performed += OnDebugUIToggle;
    }

    private void OnDisable()
    {
        //input.DroneControl.DebugUI.performed -= OnDebugUIToggle;
        input.Disable();
    }
}
