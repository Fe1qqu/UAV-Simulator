using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class DroneCommandController : MonoBehaviour
{
    [SerializeField] private DroneControlAuthority droneControlAuthority;
    [SerializeField] private DroneVideoStreamer droneVideoStreamer;

    private readonly Queue<IDroneCommand> commandQueue = new();
    private readonly Dictionary<string, Func<JObject, IDroneCommand>> factory = new();

    private CancellationTokenSource cancellationTokenSource;
    private bool isExecutingQueue;

    private DroneCommandContext droneCommandContext;

    private void Awake()
    {
        if (droneControlAuthority == null)
        {
            Debug.LogError($"[DroneCommandController] DroneControlAuthority is not assigned.");
        }

        if (droneVideoStreamer == null)
        {
            Debug.LogError($"[DroneCommandController] DroneVideoStreamer is not assigned.");
        }

        droneCommandContext = new DroneCommandContext(droneControlAuthority, droneVideoStreamer);

        factory["takeoff"] = _ => new TakeOffCommand();
        factory["land"] = _ => new LandCommand();
        factory["move_forward"] = arguments =>
        {
            float distance = arguments["distance_cm"]!.Value<float>();
            return new MoveForwardCommand(distance);
        };
        factory["streamon"] = _ => new StreamOnCommand();
        factory["streamoff"] = _ => new StreamOffCommand();
    }

    public void EnqueueCommand(string name, JObject arguments)
    {
        if (!factory.TryGetValue(name, out var creator))
        {
            Debug.LogWarning($"[DroneCommandController] Unknown command: {name}.");
            return;
        }

        IDroneCommand command = creator(arguments);
        commandQueue.Enqueue(command);

        if (!isExecutingQueue)
        {
            _ = ExecuteQueueAsync();
        }
    }

    private async Task ExecuteQueueAsync()
    {
        isExecutingQueue = true;
        cancellationTokenSource = new CancellationTokenSource();

        droneControlAuthority.SetMode(DroneControlMode.RemoteCommand);

        while (commandQueue.Count > 0)
        {
            IDroneCommand droneCommand = commandQueue.Dequeue();

            try
            {
                await droneCommand.ExecuteAsync(droneCommandContext, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[DroneCommandController] Command queue cancelled.");
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DroneCommandController] Command error: {ex}.");
            }
        }

        droneControlAuthority.SetMode(DroneControlMode.Manual);

        isExecutingQueue = false;
        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
    }

    public void ResetState()
    {
        cancellationTokenSource?.Cancel();
        commandQueue.Clear();

        droneControlAuthority.SetMode(DroneControlMode.Manual);
        droneVideoStreamer.StreamOff();
    }
}
