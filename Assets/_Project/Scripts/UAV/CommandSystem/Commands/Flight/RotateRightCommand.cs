using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Поворачивает дрон вправо (по часовой стрелке) на angle_deg.
///
/// yawInput=+1 → currentYawAngle увеличивается (вправо).
/// После достижения угла — snap в точный targetYaw через SetTargetAngles.
/// </summary>
public class RotateRightCommand : UAVCommandBase
{
    private readonly float angleDeg;
    private const float AngleTolerance = 0.5f;

    public RotateRightCommand(float angleDeg)
    {
        this.angleDeg = Mathf.Abs(angleDeg);
    }

    public override async Task ExecuteAsync(UAVCommandContext context, CancellationToken token)
    {
        Debug.Log($"[RotateRightCommand] Angle={angleDeg}°");

        var q = context.Quadcopter;
        if (q == null) { Debug.LogError("[RotateRightCommand] QuadcopterController not found."); return; }

        float startYaw  = q.CurrentYawAngle;
        float targetYaw = startYaw + angleDeg; // вправо = yaw растёт

        while (q.CurrentYawAngle < targetYaw - AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, 1f, 0f, 0f);
            await Task.Yield();
        }

        q.SetTargetAngles(q.CurrentPitchAngle, q.CurrentRollAngle, targetYaw);
        await Task.Yield();
        q.ClearDirectAngleMode();
        context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
