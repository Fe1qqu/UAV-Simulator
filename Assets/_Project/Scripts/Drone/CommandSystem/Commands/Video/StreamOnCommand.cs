using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public sealed class StreamOnCommand : IDroneCommand
{
    public async Task ExecuteAsync(DroneCommandContext droneCommandContext, CancellationToken token)
    {
        Debug.Log("[StreamOnCommand] Starting video stream.");

        await UnityMainThread.RunAsync(() => droneCommandContext.VideoStreamer.StreamOn());

        Debug.Log("[StreamOnCommand] Video stream started.");
    }
}
