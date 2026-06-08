using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class LandCommand : UAVCommandBase
{
    private const float Duration = 2f;

    public override async Task ExecuteAsync(UAVCommandContext uavCommandContext, CancellationToken token)
    {
        Debug.Log($"[LandCommand] Land.");

        float timer = 0f;

        while (timer < Duration)
        {
            token.ThrowIfCancellationRequested();

            uavCommandContext.ControlAuthority.ApplyRemote(0.2f, 0f, 0f, 0f);

            timer += Time.deltaTime;
            await Task.Yield();
        }

        uavCommandContext.ControlAuthority.ApplyRemote(0f, 0f, 0f, 0f);
    }
}
