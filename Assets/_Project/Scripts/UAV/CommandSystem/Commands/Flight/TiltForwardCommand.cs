using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Наклоняет дрон вперёд на angle_deg, держит duration_ms мс, затем возвращает к нулю.
///
/// Реализация (input-driven):
///   Фаза 1 — подаём pitchInput=+1 пока CurrentPitchAngle не достигнет targetAngle,
///             после чего насильно фиксируем угол через SetTargetAngles.
///   Фаза 2 — держим угол через SetTargetAngles на протяжении durationSec.
///   Фаза 3 — подаём pitchInput=-1 пока не вернёмся к 0, затем ClearDirectAngleMode.
///
/// ВАЖНО: maxTiltAngle в QuadcopterController ограничивает наклон (по умолчанию 30°).
///        Если angle_deg > maxTiltAngle — команда зажмёт цель до maxTiltAngle.
/// </summary>
public class TiltForwardCommand : UAVCommandBase
{
    private readonly float targetAngle;  // положительный pitch = вперёд
    private readonly float durationSec;
    private const float AngleTolerance = 0.5f;

    public TiltForwardCommand(float angleDeg, float durationMs)
    {
        this.targetAngle = Mathf.Abs(angleDeg);
        this.durationSec = durationMs / 1000f;
    }

    public override async Task ExecuteAsync(UAVCommandContext context, CancellationToken token)
    {
        Debug.Log($"[TiltForwardCommand] Angle={targetAngle}°, Duration={durationSec:F2}s");

        var q = context.Quadcopter;
        if (q == null) { Debug.LogError("[TiltForwardCommand] QuadcopterController not found."); return; }

        // ── Фаза 1: наклоняемся вперёд через input ────────────────────
        // pitchInput=+1 → currentPitchAngle += rotationSpeed * dt (в FixedUpdate)
        // Ждём пока угол не достигнет цели
        while (q.CurrentPitchAngle < targetAngle - AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, 0f, 1f, 0f);
            await Task.Yield();
        }

        // Фиксируем угол насильно (snap), чтобы не промахнуться
        q.SetTargetAngles(targetAngle, q.CurrentRollAngle, q.CurrentYawAngle);

        // ── Фаза 2: держим наклон через directAngleMode ───────────────
        float timer = 0f;
        while (timer < durationSec)
        {
            token.ThrowIfCancellationRequested();
            // SetTargetAngles удерживает угол через MoveTowards в FixedUpdate
            q.SetTargetAngles(targetAngle, q.CurrentRollAngle, q.CurrentYawAngle);
            context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
            timer += Time.deltaTime;
            await Task.Yield();
        }

        // ── Фаза 3: возвращаемся к нулю через input ──────────────────
        q.ClearDirectAngleMode();
        while (q.CurrentPitchAngle > AngleTolerance)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(0.5f, 0f, -1f, 0f);
            await Task.Yield();
        }

        // Финальный snap к нулю и выход из directAngleMode
        q.SetTargetAngles(0f, 0f, q.CurrentYawAngle);
        await Task.Yield();
        q.ClearDirectAngleMode();
        context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
