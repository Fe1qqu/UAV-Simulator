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
    private QuadcopterController quadcopterController;

    // Теперь в очереди хранится пара: команда + TCS для сигнала о завершении
    private readonly Queue<(IUAVCommand command, TaskCompletionSource<bool> tcs)> commandQueue = new();
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

    /// <summary>
    /// Ставит команду в очередь и возвращает Task, который завершится
    /// когда именно эта команда выполнена. TCP-сервер ждёт его перед
    /// отправкой ответа Python-клиенту — это гарантирует, что drone.throttle()
    /// на стороне Python заблокирован до реального окончания газа в Unity.
    /// </summary>
    public Task EnqueueCommand(string name, JObject arguments)
    {
        if (!factory.TryGetValue(name, out var creator))
        {
            Debug.LogWarning($"[UAVCommandController] Unknown command: {name}.");
            return Task.CompletedTask;
        }

        IUAVCommand command = creator(arguments);
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        commandQueue.Enqueue((command, tcs));

        if (!isExecutingQueue)
            _ = ExecuteQueueAsync();

        return tcs.Task;
    }

    private async Task ExecuteQueueAsync()
    {
        isExecutingQueue = true;
        cancellationTokenSource = new CancellationTokenSource();
        uavControlAuthority.SetMode(UAVControlMode.RemoteCommand);

        while (commandQueue.Count > 0)
        {
            var (uavCommand, tcs) = commandQueue.Dequeue();
            try
            {
                await uavCommand.ExecuteAsync(uavCommandContext, cancellationTokenSource.Token);
                tcs.TrySetResult(true);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[UAVCommandController] Command queue cancelled.");
                tcs.TrySetCanceled();
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UAVCommandController] Command error: {ex}.");
                tcs.TrySetException(ex);
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
