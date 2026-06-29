using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Кренит дрон вправо на angle_deg, держит duration_ms мс, возвращает к нулю.
///
/// В QuadcopterController: currentRollAngle += -rollInput * rotationSpeed * dt
///   rollInput = +1  →  currentRollAngle убывает (отрицательный = крен вправо)
///   rollInput = -1  →  currentRollAngle растёт (возврат к нулю)
///
/// После достижения угла — snap через SetTargetAngles, фаза удержания, возврат.
/// </summary>
public class TiltRightCommand : UAVCommandBase
{
    private readonly float targetAngle;
    private readonly float durationSec;
    private const float AngleTolerance = 0.5f;

    public TiltRightCommand(float angleDeg, float durationMs)
    {
        this.targetAngle = Mathf.Abs(angleDeg);
        this.durationSec = durationMs / 1000f;
    }

    public override async Task ExecuteAsync(UAVCommandContext context, CancellationToken token)
    {
        Debug.Log($"[TiltRightCommand] Angle={targetAngle}°, Duration={durationSec:F2}s");

        var q = context.Quadcopter;
        if (q == null) { Debug.LogError("[TiltRightCommand] QuadcopterController not found."); return; }

        // ── Фаза 1: крен вправо ───────────────────────────────────────
        // rollInput=+1 → currentRollAngle убывает (становится отрицательным = вправо)
        while (q.CurrentRollAngle > -targetAngle + AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 1f);
            await Task.Yield();
        }

        // Snap к целевому крену (отрицательный = вправо)
        q.SetTargetAngles(q.CurrentPitchAngle, -targetAngle, q.CurrentYawAngle);

        // ── Фаза 2: держим крен ───────────────────────────────────────
        float timer = 0f;
        while (timer < durationSec)
        {
            token.ThrowIfCancellationRequested();
            q.SetTargetAngles(q.CurrentPitchAngle, -targetAngle, q.CurrentYawAngle);
            context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
            timer += Time.deltaTime;
            await Task.Yield();
        }

        // ── Фаза 3: возврат к нулю ────────────────────────────────────
        q.ClearDirectAngleMode();
        while (q.CurrentRollAngle < -AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, -1f);
            await Task.Yield();
        }

        q.SetTargetAngles(q.CurrentPitchAngle, 0f, q.CurrentYawAngle);
        await Task.Yield();
        q.ClearDirectAngleMode();
        context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
