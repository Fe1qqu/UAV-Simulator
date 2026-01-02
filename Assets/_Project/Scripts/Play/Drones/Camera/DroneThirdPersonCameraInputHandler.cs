using UnityEngine;
using Unity.Cinemachine;

public class DroneThirdPersonCameraInputHandler : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;
    [SerializeField] private DroneCameraInput droneCameraInput;

    private void Awake()
    {
        if (cinemachineInputAxisController == null)
        {
            Debug.LogError("[ThirdPersonCameraInputHandler] CinemachineInputAxisController is not assigned.");
            return;
        }

        if (droneCameraInput == null)
        {
            Debug.LogError("[ThirdPersonCameraInputHandler] DroneCameraInput is not assigned.");
            return;
        }

        cinemachineInputAxisController.enabled = false;
    }

    private void OnEnable()
    {
        droneCameraInput.MovementEnabledChanged += OnMovementEnabledChanged;
    }

    private void OnDisable()
    {
        droneCameraInput.MovementEnabledChanged -= OnMovementEnabledChanged;
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
