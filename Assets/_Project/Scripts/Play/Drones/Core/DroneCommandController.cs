using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DroneCommandController : MonoBehaviour
{
    private readonly Queue<Func<Task>> commandQueue = new();
    private bool isExecutingQueue;

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

    public async Task TakeOffAsync()
    {
        Debug.Log("[DroneCommandController] TakeOff started");
        await Task.Delay(3000); // заглушка
        Debug.Log("[DroneCommandController] TakeOff completed");
    }

    public async Task MoveForwardAsync(float distanceCm)
    {
        Debug.Log($"[DroneCommandController] MoveForward {distanceCm}cm started");
        await Task.Delay(3000); // заглушка
        Debug.Log("[DroneCommandController] MoveForward completed");
    }
}
