using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class UAVCameraSwitcher : MonoBehaviour
{
    [Header("Cinemachine cameras")]
    [SerializeField] private List<CinemachineCamera> cameras = new();

    [Header("Default active camera index")]
    [SerializeField] private int defaultCameraIndex = 0;

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 2;
    [SerializeField] private int inactivePriority = 1;

    private int currentCameraIndex;

    private void Awake()
    {
        if (cameras.Count == 0)
        {
            Debug.LogError("[UAVCameraSwitcher] No cameras assigned.");
            return;
        }

        currentCameraIndex = Mathf.Clamp(defaultCameraIndex, 0, cameras.Count - 1);
        if (currentCameraIndex != defaultCameraIndex)
        {
            Debug.LogWarning(
                $"[UAVCameraSwitcher] defaultCameraIndex ({defaultCameraIndex}) " +
                $"was out of range [0 .. {cameras.Count - 1}] and has been clamped to {currentCameraIndex}."
            );
        }

        // Resetting priorities for all cameras
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] == null)
            {
                Debug.LogError($"[UAVCameraSwitcher] Camera at index {i} is null on object '{gameObject.name}'.");
                continue;
            }

            cameras[i].Priority = inactivePriority;
        }

        ActivateCamera(currentCameraIndex);
    }

    /// <summary>
    /// Enables only the camera with the given index.
    /// </summary>
    private void ActivateCamera(int index)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].Priority = (i == index) ? activePriority : inactivePriority;
        }
    }

    /// <summary>
    /// Switch to the next camera in the list (looping).
    /// </summary>
    public void NextCamera()
    {
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
        ActivateCamera(currentCameraIndex);
    }

    /// <summary>
    /// Switch to a specific camera by index.
    /// </summary>
    public void SwitchTo(int index)
    {
        if (index < 0 || index >= cameras.Count)
        {
            Debug.LogWarning($"[UAVCameraSwitcher] Invalid camera index: {index}.");
            return;
        }

        currentCameraIndex = index;
        ActivateCamera(currentCameraIndex);
    }
}
