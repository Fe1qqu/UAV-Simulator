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

    // QuadcopterController нужен командам, управляющим углами напрямую.
    // Заполняется автоматически из GetComponent — если на объекте есть QuadcopterController.
    private QuadcopterController quadcopterController;

    private readonly Queue<IUAVCommand> commandQueue = new();
    private readonly Dictionary<string, Func<JObject, IUAVCommand>> factory = new();
    private CancellationTokenSource cancellationTokenSource;
    private bool isExecutingQueue;
    private UAVCommandContext uavCommandContext;

    private void Awake()
    {
        if (uavControlAuthority == null)
            Debug.LogError("[UAVCommandController] uavControlAuthority is not assigned.");

        if (uavVideoStreamer == null)
            Debug.LogError("[UAVCommandController] uavVideoStreamer is not assigned.");

        // Пытаемся получить QuadcopterController с того же GameObject
        quadcopterController = GetComponent<QuadcopterController>();

        uavCommandContext = new UAVCommandContext(
            uavControlAuthority,
            uavVideoStreamer,
            quadcopterController
        );

        // ── Базовые команды ───────────────────────────────────────────────
        factory["takeoff"]      = _ => new TakeOffCommand();
        factory["land"]         = _ => new LandCommand();
        factory["move_forward"] = args =>
        {
            float distance = args["distance_cm"]!.Value<float>();
            return new MoveForwardCommand(distance);
        };
        factory["streamon"]  = _ => new StreamOnCommand();
        factory["streamoff"] = _ => new StreamOffCommand();

        // ── Новые команды ─────────────────────────────────────────────────
        factory["throttle"] = args =>
        {
            float power      = args["power"]!.Value<float>();
            float durationMs = args["duration_ms"]!.Value<float>();
            return new ThrottleCommand(power, durationMs);
        };

        factory["tilt_forward"] = args =>
        {
            float angleDeg   = args["angle_deg"]!.Value<float>();
            float durationMs = args["duration_ms"]!.Value<float>();
            return new TiltForwardCommand(angleDeg, durationMs);
        };

        factory["tilt_backward"] = args =>
        {
            float angleDeg   = args["angle_deg"]!.Value<float>();
            float durationMs = args["duration_ms"]!.Value<float>();
            return new TiltBackwardCommand(angleDeg, durationMs);
        };

        factory["rotate_left"] = args =>
        {
            float angleDeg = args["angle_deg"]!.Value<float>();
            return new RotateLeftCommand(angleDeg);
        };

        factory["rotate_right"] = args =>
        {
            float angleDeg = args["angle_deg"]!.Value<float>();
            return new RotateRightCommand(angleDeg);
        };

        factory["tilt_left"] = args =>
        {
            float angleDeg   = args["angle_deg"]!.Value<float>();
            float durationMs = args["duration_ms"]!.Value<float>();
            return new TiltLeftCommand(angleDeg, durationMs);
        };

        factory["tilt_right"] = args =>
        {
            float angleDeg   = args["angle_deg"]!.Value<float>();
            float durationMs = args["duration_ms"]!.Value<float>();
            return new TiltRightCommand(angleDeg, durationMs);
        };
    }

    public void EnqueueCommand(string name, JObject arguments)
    {
        if (!factory.TryGetValue(name, out var creator))
        {
            Debug.LogWarning($"[UAVCommandController] Unknown command: {name}.");
            return;
        }

        IUAVCommand command = creator(arguments);
        commandQueue.Enqueue(command);

        if (!isExecutingQueue)
            _ = ExecuteQueueAsync();
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
                Debug.Log("[UAVCommandController] Command queue cancelled.");
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UAVCommandController] Command error: {ex}.");
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
    }
}
