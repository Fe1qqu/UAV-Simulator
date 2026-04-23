using UnityEngine;

public class InputModeController : MonoBehaviour
{
    public static InputModeController Instance { get; private set; }

    private Input input;
    public Input Input => input;

    private InputMode currentMode;
    public InputMode CurrentMode => currentMode;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[InputModeController] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        input = new Input();
    }

    public void SetMode(InputMode mode)
    {
        if (currentMode == mode)
        {
            return;
        }

        currentMode = mode;

        input.Disable();

        switch (mode)
        {
            case InputMode.Play:
                input.Play.Enable();
                input.DroneControl.Enable();
                input.DroneCamera.Enable();
                input.UI.Enable();
                break;

            case InputMode.LevelEditor:
                input.LevelEditor.Enable();
                input.LevelEditorCamera.Enable();
                input.UI.Enable();
                break;

            case InputMode.UI:
                input.UI.Enable();
                break;

            case InputMode.Loading:
            case InputMode.None:
                break;
        }
    }
}
