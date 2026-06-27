using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Включает газ на power*100% на duration_ms миллисекунд,
/// после чего возвращается к hover (throttle=0.5).
/// Работает только в Manual-совместимых режимах (AngleLimited / ACRO).
/// В AltHold throttleInput всё равно переопределяется hoverRPM — используй AltHold отдельно.
/// </summary>
public class ThrottleCommand : UAVCommandBase
{
    private readonly float power;
    private readonly float durationSec;

    public ThrottleCommand(float power, float durationMs)
    {
        this.power       = Mathf.Clamp01(power);
        this.durationSec = durationMs / 1000f;
    }

    public override async Task ExecuteAsync(UAVCommandContext context, CancellationToken token)
    {
        Debug.Log($"[ThrottleCommand] Power={power:P0}, Duration={durationSec:F2}s");

        float timer = 0f;
        while (timer < durationSec)
        {
            token.ThrowIfCancellationRequested();
            context.ControlAuthority.ApplyRemote(power, 0f, 0f, 0f);
            timer += Time.deltaTime;
            await Task.Yield();
        }

        // Возврат к hover
        context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
