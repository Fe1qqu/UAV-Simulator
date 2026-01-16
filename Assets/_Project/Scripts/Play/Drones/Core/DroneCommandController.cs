using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DroneCommandController : MonoBehaviour
{
    private DroneVideoStreamer droneVideoStreamer;

    private readonly Queue<Func<Task>> commandQueue = new();
    private bool isExecutingQueue;

    private IControllable controllable;

    // TEST
    [Header("Takeoff test")]
    [SerializeField] private float maxThrottle = 0.5f;
    [SerializeField] private float takeoffRampSpeed = 0.1f;
    private float currentThrottle;
    private bool isTakingOff;
    // TEST

    private void Awake()
    {
        droneVideoStreamer = GetComponent<DroneVideoStreamer>();
        if (droneVideoStreamer == null)
        {
            Debug.LogError($"[DroneCommandController] There is no DroneVideoStreamer component on the object {gameObject.name}.");
        }

        controllable = GetComponent<IControllable>();
        if (controllable == null)
        {
            Debug.LogError($"[DroneCommandController] There is no IControllable component on the object {gameObject.name}.");
        }
    }

    public void EnqueueCommand(Func<Task> command)
    {
        commandQueue.Enqueue(command);

        if (!isExecutingQueue)
        {
            _ = ExecuteQueueAsync();
        }
    }

    private async Task ExecuteQueueAsync()
    {
        isExecutingQueue = true;

        while (commandQueue.Count > 0)
        {
            Func<Task> command = commandQueue.Dequeue();
            await command();
        }

        isExecutingQueue = false;
    }

    // -------- Commands --------

    // TEST
    private void FixedUpdate()
    {
        if (!isTakingOff)
            return;

        currentThrottle = Mathf.MoveTowards(
            currentThrottle,
            maxThrottle,
            takeoffRampSpeed * Time.fixedDeltaTime
        );

        controllable.ApplyThrottle(currentThrottle);
    }
    // TEST

    public async Task TakeOffAsync()
    {
        Debug.Log("[DroneCommandController] TakeOff started");

        // TEST
        isTakingOff = true;

        while (currentThrottle < maxThrottle - 0.01f)
        {
            await Task.Delay(20); // polling состояния
        }

        isTakingOff = false;

        currentThrottle = 0;
        controllable.ApplyThrottle(currentThrottle);
        // TEST

        Debug.Log("[DroneCommandController] TakeOff completed");
    }

    public async Task MoveForwardAsync(float distanceCm)
    {
        Debug.Log($"[DroneCommandController] MoveForward {distanceCm}cm started");

        await Task.Delay(3000); // заглушка

        Debug.Log("[DroneCommandController] MoveForward completed");
    }

    public Task StreamOnAsync()
    {
        return UnityMainThread.RunAsync(() => droneVideoStreamer.StreamOn());
    }

    public Task StreamOffAsync()
    {
        return UnityMainThread.RunAsync(() => droneVideoStreamer.StreamOff());
    }
}
