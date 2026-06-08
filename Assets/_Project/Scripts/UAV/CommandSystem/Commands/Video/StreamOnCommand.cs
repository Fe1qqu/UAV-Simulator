using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public sealed class StreamOnCommand : IUAVCommand
{
    public async Task ExecuteAsync(UAVCommandContext uavCommandContext, CancellationToken token)
    {
        Debug.Log("[StreamOnCommand] Starting video stream.");

        await UnityMainThread.RunAsync(() => uavCommandContext.VideoStreamer.StreamOn());

        Debug.Log("[StreamOnCommand] Video stream started.");
    }
}
