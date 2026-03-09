using UnityEngine;
using UnityEngine.InputSystem;

public class BackInput : MonoBehaviour
{
    public static BackInput Instance { get; private set; }

    private Input input;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[BackInput] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        input = InputModeController.Instance.Input;
    }

    private void OnEnable()
    {
        input.UI.Cancel.performed += OnBack;
    }

    private void OnDisable()
    {
        input.UI.Cancel.performed -= OnBack;
    }

    private void OnBack(InputAction.CallbackContext _)
    {
        //print("[BackInput] OnBack.");
        BackDispatcher.DispatchBack();
    }
}
