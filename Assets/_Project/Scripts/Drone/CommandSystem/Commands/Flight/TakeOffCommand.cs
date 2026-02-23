using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class TakeOffCommand : DroneCommandBase
{
    private const float TargetThrottle = 1f;
    private const float Duration = 2f;

    public override async Task ExecuteAsync(DroneCommandContext droneCommandContext, CancellationToken token)
    {
        Debug.Log($"[TakeOffCommand] Takeoff.");

        float timer = 0f;

        while (timer < Duration)
        {
            token.ThrowIfCancellationRequested();

            droneCommandContext.ControlAuthority.ApplyRemote(TargetThrottle, 0f, 0f, 0f);

            timer += Time.deltaTime;
            await Task.Yield();
        }

        droneCommandContext.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
