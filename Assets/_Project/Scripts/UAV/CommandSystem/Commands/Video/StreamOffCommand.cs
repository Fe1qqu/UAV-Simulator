using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public sealed class StreamOffCommand : IUAVCommand
{
    public async Task ExecuteAsync(UAVCommandContext uavCommandContext, CancellationToken token)
    {
        Debug.Log("[StreamOffCommand] Stopping video stream.");

        await UnityMainThread.RunAsync(() => uavCommandContext.VideoStreamer.StreamOff());

        Debug.Log("[StreamOffCommand] Video stream stopped.");
    }
}
