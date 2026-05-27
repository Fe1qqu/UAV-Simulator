using UnityEngine;
using System;

[RequireComponent(typeof(DroneControllerBase))]
public class DroneDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject explosionCameraPrefab;
    [SerializeField] private Camera droneCamera;
    [SerializeField] private AudioListener droneAudioListener;

    public event Action<ExplosionEffect> ExplosionSpawned;

    public ExplosionCameraController ActiveExplosionCamera { get; private set; }

    private DroneControllerBase drone;

    private void Awake()
    {
        drone = GetComponent<DroneControllerBase>();
    }

    private void OnEnable()
    {
        drone.Exploded += OnExploded;
    }

    private void OnDisable()
    {
        drone.Exploded -= OnExploded;
    }

    private void OnExploded()
    {
        

        HandleCamera();
        SpawnExplosion();
        HideDrone();
    }

    private void HandleCamera()
    {
        Vector3 startPosition = droneCamera.transform.position;
        Quaternion startRotation = droneCamera.transform.rotation;

        droneCamera.gameObject.SetActive(false);
        droneAudioListener.enabled = false;

        GameObject cameraGameObject = Instantiate(explosionCameraPrefab);
        ActiveExplosionCamera = cameraGameObject.GetComponent<ExplosionCameraController>();
        ActiveExplosionCamera.Initialize(startPosition, startRotation, transform.position, transform.forward);
    }

    private void SpawnExplosion()
    {
        GameObject explosionGameObject = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        ExplosionEffect explosionEffect = explosionGameObject.GetComponent<ExplosionEffect>();
        ExplosionSpawned?.Invoke(explosionEffect);
    }

    private void HideDrone()
    {
        drone.Hide();
    }

    public void Restore()
    {
        if (ActiveExplosionCamera != null)
        {
            Destroy(ActiveExplosionCamera.gameObject);
            ActiveExplosionCamera = null;
        }

        droneCamera.gameObject.SetActive(true);
        droneAudioListener.enabled = true;
        drone.Show();
    }
}
