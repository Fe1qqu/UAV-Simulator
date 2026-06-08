using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UAVCommandController : MonoBehaviour
{
    [SerializeField] private UAVControlAuthority uavControlAuthority;
    [SerializeField] private UAVVideoStreamer uavVideoStreamer;

    private readonly Queue<IUAVCommand> commandQueue = new();
    private readonly Dictionary<string, Func<JObject, IUAVCommand>> factory = new();

    private CancellationTokenSource cancellationTokenSource;
    private bool isExecutingQueue;

    private UAVCommandContext uavCommandContext;

    private void Awake()
    {
        if (uavControlAuthority == null)
        {
            Debug.LogError($"[uavCommandController] uavControlAuthority is not assigned.");
        }

        if (uavVideoStreamer == null)
        {
            Debug.LogError($"[uavCommandController] uavVideoStreamer is not assigned.");
        }

        UAVCommandContext uavCommandContext = new(uavControlAuthority, uavVideoStreamer);

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
            Debug.LogWarning($"[uavCommandController] Unknown command: {name}.");
            return;
        }

        IUAVCommand command = creator(arguments);
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

        uavControlAuthority.SetMode(UAVControlMode.RemoteCommand);

        while (commandQueue.Count > 0)
        {
            IUAVCommand uavCommand = commandQueue.Dequeue();

            try
            {
                await uavCommand.ExecuteAsync(uavCommandContext, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[uavCommandController] Command queue cancelled.");
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[uavCommandController] Command error: {ex}.");
            }
        }

        uavControlAuthority.SetMode(UAVControlMode.Manual);

        isExecutingQueue = false;
        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
    }

    public void ResetState()
    {
        cancellationTokenSource?.Cancel();
        commandQueue.Clear();

        uavControlAuthority.SetMode(UAVControlMode.Manual);
        uavVideoStreamer.StreamOff();
    }
}
