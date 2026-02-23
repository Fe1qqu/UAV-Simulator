using UnityEngine;

public class DroneControlAuthority : MonoBehaviour
{
    [SerializeField] private MonoBehaviour controllableBehaviour;

    private IControllable controllable;

    public DroneControlMode CurrentMode { get; private set; } = DroneControlMode.Manual;

    private void Awake()
    {
        if (controllableBehaviour == null)
        {
            Debug.LogError("[DroneControlAuthority] ControllableBehaviour is not assigned.");
            enabled = false;
            return;
        }

        controllable = controllableBehaviour as IControllable;
        if (controllable == null)
        {
            Debug.LogError($"[DroneControlAuthority] {controllableBehaviour.name} does not implement IControllable.");
        }
    }

    public void SetMode(DroneControlMode mode)
    {
        CurrentMode = mode;
    }

    public void ApplyManual(float throttle, float yaw, float pitch, float roll)
    {
        if (CurrentMode != DroneControlMode.Manual)
        {
            return;
        }

        controllable?.Apply(throttle, yaw, pitch, roll);
    }

    public void ApplyRemote(float throttle, float yaw, float pitch, float roll)
    {
        if (CurrentMode != DroneControlMode.RemoteCommand)
        {
            return;
        }

        controllable?.Apply(throttle, yaw, pitch, roll);
    }
}
