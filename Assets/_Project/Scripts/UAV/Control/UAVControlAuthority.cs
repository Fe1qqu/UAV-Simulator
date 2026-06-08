using UnityEngine;

public class UAVControlAuthority : MonoBehaviour
{
    [SerializeField] private MonoBehaviour controllableBehaviour;

    private IControllable controllable;

    public UAVControlMode CurrentMode { get; private set; } = UAVControlMode.Manual;

    private void Awake()
    {
        if (controllableBehaviour == null)
        {
            Debug.LogError("[UAVControlAuthority] ControllableBehaviour is not assigned.");
            enabled = false;
            return;
        }

        controllable = controllableBehaviour as IControllable;
        if (controllable == null)
        {
            Debug.LogError($"[UAVControlAuthority] {controllableBehaviour.name} does not implement IControllable.");
        }
    }

    public void SetMode(UAVControlMode mode)
    {
        CurrentMode = mode;
    }

    public void ApplyManual(float throttle, float yaw, float pitch, float roll)
    {
        if (CurrentMode != UAVControlMode.Manual)
        {
            return;
        }

        controllable?.Apply(throttle, yaw, pitch, roll);
    }

    public void ApplyRemote(float throttle, float yaw, float pitch, float roll)
    {
        if (CurrentMode != UAVControlMode.RemoteCommand)
        {
            return;
        }

        controllable?.Apply(throttle, yaw, pitch, roll);
    }
}
