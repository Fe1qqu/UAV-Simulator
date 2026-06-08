using UnityEngine;
using System;

[RequireComponent(typeof(UAVControllerBase))]
public class UAVDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject explosionCameraPrefab;
    [SerializeField] private Camera uavCamera;
    [SerializeField] private AudioListener uavAudioListener;

    public event Action<ExplosionEffect> ExplosionSpawned;

    public ExplosionCameraController ActiveExplosionCamera { get; private set; }

    private UAVControllerBase uav;

    private void Awake()
    {
        uav = GetComponent<UAVControllerBase>();
    }

    private void OnEnable()
    {
        uav.Exploded += OnExploded;
    }

    private void OnDisable()
    {
        uav.Exploded -= OnExploded;
    }

    private void OnExploded()
    {
        HandleCamera();
        SpawnExplosion();
        HideUAV();
    }

    private void HandleCamera()
    {
        Vector3 startPosition = uavCamera.transform.position;
        Quaternion startRotation = uavCamera.transform.rotation;

        uavCamera.gameObject.SetActive(false);
        uavAudioListener.enabled = false;

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

    private void HideUAV()
    {
        uav.Hide();
    }

    public void Restore()
    {
        if (ActiveExplosionCamera != null)
        {
            Destroy(ActiveExplosionCamera.gameObject);
            ActiveExplosionCamera = null;
        }

        uavCamera.gameObject.SetActive(true);
        uavAudioListener.enabled = true;
        uav.Show();
    }
}
