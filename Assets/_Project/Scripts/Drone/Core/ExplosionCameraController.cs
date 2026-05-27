using UnityEngine;

public class ExplosionCameraController : MonoBehaviour
{
    [SerializeField] private float distance = 10f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float lerpSpeed = 3f;
    [SerializeField] private float followTime = 3f;

    private Vector3 explosionPosition;
    private Vector3 targetPosition;
    private float timer;

    public void Initialize(Vector3 startPosition, Quaternion startRotation, Vector3 explosionPosition, Vector3 droneForward)
    {
        this.explosionPosition = explosionPosition;

        Vector3 offset = -droneForward * distance + Vector3.up * height;
        targetPosition = explosionPosition + offset;

        transform.position = startPosition;
        transform.rotation = startRotation;

        timer = 0f;
    }

    private void LateUpdate()
    {
        timer += Time.deltaTime;

        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);

        transform.LookAt(explosionPosition);

        if (timer >= followTime)
        {
            enabled = false;
        }
    }
}
