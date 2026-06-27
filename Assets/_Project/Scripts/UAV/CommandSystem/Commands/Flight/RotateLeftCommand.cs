using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Поворачивает дрон влево (против часовой стрелки) на angle_deg.
///
/// Реализация:
///   Подаём yawInput=-1 пока накопленный yaw не изменится на нужную величину,
///   после чего snap в точный целевой yaw через SetTargetAngles.
///   Используем разницу startYaw - currentYaw (yaw убывает при yawInput=-1).
/// </summary>
public class RotateLeftCommand : UAVCommandBase
{
    private readonly float angleDeg;
    private const float AngleTolerance = 0.5f;

    public RotateLeftCommand(float angleDeg)
    {
        this.angleDeg = Mathf.Abs(angleDeg);
    }

    public override async Task ExecuteAsync(UAVCommandContext context, CancellationToken token)
    {
        Debug.Log($"[RotateLeftCommand] Angle={angleDeg}°");

        var q = context.Quadcopter;
        if (q == null) { Debug.LogError("[RotateLeftCommand] QuadcopterController not found."); return; }

        float startYaw  = q.CurrentYawAngle;
        float targetYaw = startYaw - angleDeg; // влево = yaw убывает

        // Крутим yawInput=-1 пока не накрутили нужный угол
        // CurrentYawAngle — накопленный float (не зажат в 0-360), поэтому просто сравниваем
        while (q.CurrentYawAngle > targetYaw + AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, -1f, 0f, 0f);
            await Task.Yield();
        }

        // Snap к точному целевому yaw, pitch и roll оставляем как есть
        q.SetTargetAngles(q.CurrentPitchAngle, q.CurrentRollAngle, targetYaw);
        await Task.Yield();
        q.ClearDirectAngleMode();
        context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
