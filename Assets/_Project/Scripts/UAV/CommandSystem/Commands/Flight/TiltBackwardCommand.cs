using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Наклоняет дрон назад на angle_deg, держит duration_ms мс, возвращает к нулю.
///
/// Назад = отрицательный pitch (pitchInput=-1 → currentPitchAngle убывает).
/// После достижения угла — насильный snap через SetTargetAngles, фаза удержания,
/// затем возврат и ClearDirectAngleMode.
/// </summary>
public class TiltBackwardCommand : UAVCommandBase
{
    private readonly float targetAngle;
    private readonly float durationSec;
    private const float AngleTolerance = 0.5f;

    public TiltBackwardCommand(float angleDeg, float durationMs)
    {
        this.targetAngle = Mathf.Abs(angleDeg);
        this.durationSec = durationMs / 1000f;
    }

    public override async Task ExecuteAsync(UAVCommandContext context, CancellationToken token)
    {
        Debug.Log($"[TiltBackwardCommand] Angle={targetAngle}°, Duration={durationSec:F2}s");

        var q = context.Quadcopter;
        if (q == null) { Debug.LogError("[TiltBackwardCommand] QuadcopterController not found."); return; }

        // ── Фаза 1: наклоняемся назад через input ─────────────────────
        // pitchInput=-1 → currentPitchAngle -= rotationSpeed * dt
        while (q.CurrentPitchAngle > -targetAngle + AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, 0f, -1f, 0f);
            await Task.Yield();
        }

        // Snap к целевому углу
        q.SetTargetAngles(-targetAngle, q.CurrentRollAngle, q.CurrentYawAngle);

        // ── Фаза 2: держим наклон ─────────────────────────────────────
        float timer = 0f;
        while (timer < durationSec)
        {
            token.ThrowIfCancellationRequested();
            q.SetTargetAngles(-targetAngle, q.CurrentRollAngle, q.CurrentYawAngle);
            context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
            timer += Time.deltaTime;
            await Task.Yield();
        }

        // ── Фаза 3: возвращаемся к нулю ──────────────────────────────
        q.ClearDirectAngleMode();
        while (q.CurrentPitchAngle < -AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, 0f, 1f, 0f);
            await Task.Yield();
        }

        q.SetTargetAngles(0f, 0f, q.CurrentYawAngle);
        await Task.Yield();
        q.ClearDirectAngleMode();
        context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
