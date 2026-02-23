using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public sealed class StreamOffCommand : IDroneCommand
{
    public async Task ExecuteAsync(DroneCommandContext droneCommandContext, CancellationToken token)
    {
        Debug.Log("[StreamOffCommand] Stopping video stream.");

        await UnityMainThread.RunAsync(() => droneCommandContext.VideoStreamer.StreamOff());

        Debug.Log("[StreamOffCommand] Video stream stopped.");
    }
}
