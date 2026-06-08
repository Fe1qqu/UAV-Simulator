using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class MoveForwardCommand : UAVCommandBase
{
    private readonly float distanceCm;

    public MoveForwardCommand(float distanceCm)
    {
        this.distanceCm = distanceCm;
    }

    public override async Task ExecuteAsync(UAVCommandContext context, CancellationToken token)
    {
        Debug.Log($"[MoveForwardCommand] Move {distanceCm} cm.");

        float duration = distanceCm / 50f; // example: 50 cm/s
        float timer = 0f;

        while (timer < duration)
        {
            token.ThrowIfCancellationRequested();

            context.ControlAuthority.ApplyRemote(0.5f, 0f, 0.4f, 0f);

            timer += Time.deltaTime;
            await Task.Yield();
        }

        context.ControlAuthority.ApplyRemote(0.5f, 0f, 0f, 0f);
    }
}
