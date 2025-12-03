using UnityEngine;
using Unity.Cinemachine;

public class DroneThirdPersonCameraInputHandler : MonoBehaviour
{
    private CinemachineInputAxisController cinemachineInputAxisController;
    private DroneCameraInput cameraInput;

    private void Awake()
    {
        cinemachineInputAxisController = GetComponent<CinemachineInputAxisController>();
        if (cinemachineInputAxisController == null)
        {
            Debug.LogError("[ThirdPersonCameraInputHandler] Missing CinemachineInputAxisController.");
            enabled = false;
            return;
        }

        cameraInput = GetComponent<DroneCameraInput>();
        if (cameraInput == null)
        {
            Debug.LogError("[ThirdPersonCameraInputHandler] DroneCameraInput component not found on the same GameObject.");
            enabled = false;
            return;
        }

        cinemachineInputAxisController.enabled = false;
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
    }

    private void OnMovementEnabledChanged(bool enabled)
    {
        cinemachineInputAxisController.enabled = enabled;

        if (enabled)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
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
}
