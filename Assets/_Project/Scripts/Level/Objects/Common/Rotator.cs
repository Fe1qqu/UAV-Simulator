using UnityEngine;

public class Rotator : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Rotation")]
    [Tooltip("Axis used for rotation.")]
    [SerializeField] private RotationAxis axis = RotationAxis.Z;

    [Tooltip("Rotation speed in degrees per second.")]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("Use local space rotation instead of world space.")]
    [SerializeField] private bool useLocalSpace = true;

    private void Update()
    {
        Vector3 rotationAxis = GetAxisVector();
        float delta = rotationSpeed * Time.deltaTime;

        if (useLocalSpace)
        {
            transform.Rotate(rotationAxis, delta, Space.Self);
        }
        else
        {
            transform.Rotate(rotationAxis, delta, Space.World);
        }
    }

    private Vector3 GetAxisVector()
    {
        return axis switch
        {
            RotationAxis.X => Vector3.right,
            RotationAxis.Y => Vector3.up,
            RotationAxis.Z => Vector3.forward,
            _ => Vector3.up
        };
    }
}
