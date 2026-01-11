using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UavSim.Core.Protocol.Drone;

[RequireComponent(typeof(DroneCommandController))]
public class DroneTcpServer : MonoBehaviour
{
    private TcpListener tcpListener;
    private DroneCommandController droneCommandController;

    private const int BufferSize = 4096;
    private const int Port = 9000;

    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        droneCommandController = GetComponent<DroneCommandController>();

        tcpListener = new TcpListener(IPAddress.Loopback, Port);
        tcpListener.Start();

        cancellationTokenSource = new CancellationTokenSource();
        _ = AcceptClientsLoopAsync(cancellationTokenSource.Token);

        Debug.Log("[DroneTcpServer] Drone server started on 127.0.0.1:9000.");
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        tcpListener.Stop();
    }

    private async Task AcceptClientsLoopAsync(CancellationToken cancellationToken)
    {
        // Зарегистрируем остановку TcpListener один раз
        using (cancellationToken.Register(() => tcpListener.Stop()))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient;
                try
                {
                    tcpClient = await tcpListener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    // Listener has been stopped, exiting
                    break;
                }
                catch (InvalidOperationException)
                {
                    // The token may have been revoked before Accept
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }

                Debug.Log("[DroneTcpServer] Client connected");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleClientAsync(tcpClient);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[DroneTcpServer] Client session error: {ex}");
                    }
                    finally
                    {
                        Debug.Log("[DroneTcpServer] Client disconnected");
                        tcpClient.Close();
                    }
                });
            }
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        using NetworkStream networkStream = tcpClient.GetStream();
        byte[] buffer = new byte[BufferSize];

        while (tcpClient.Connected)
        {
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead <= 0)
            {
                break;
            }

            string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            JObject message = JObject.Parse(json);

            await HandleMessageAsync(message, networkStream);
        }
    }

    private async Task HandleMessageAsync(JObject message, NetworkStream stream)
    {
        string messageType = message["message_type"]!.ToString();

        if (messageType != MessageType.Command.ToWire())
        {
            await SendErrorAsync(stream, "Unsupported message type");
            return;
        }

        string commandName = message["command_name"]!.ToString();
        string commandId = message["command_id"]!.ToString();
        JObject arguments = (JObject)(message["arguments"] ?? new JObject());

        TaskCompletionSource<bool> completionSource = new();

        droneCommandController.EnqueueCommand(async () =>
        {
            try
            {
                await ExecuteCommandAsync(commandName, arguments);
                completionSource.SetResult(true);
            }
            catch (System.Exception ex)
            {
                completionSource.SetException(ex);
            }
        });

        try
        {
            await completionSource.Task;
            await SendCommandResultAsync(stream, commandId, "ok");
        }
        catch (System.Exception ex)
        {
            await SendCommandErrorAsync(stream, commandId, ex.Message);
        }
    }

    private async Task ExecuteCommandAsync(string commandName, JObject arguments)
    {
        switch (commandName)
        {
            case "takeoff":
                await droneCommandController.TakeOffAsync();
                break;

            //case "land":
            //    await droneCommandController.LandAsync();
            //    break;

            case "move_forward":
                float distance = arguments["distance_cm"]!.Value<float>();
                await droneCommandController.MoveForwardAsync(distance);
                break;

            //case "rotate_clockwise":
            //    float degrees = arguments["degrees"]!.Value<float>();
            //    await droneCommandController.RotateClockwiseAsync(degrees);
            //    break;

            default:
                throw new System.InvalidOperationException($"Unknown command: {commandName}");
        }
    }

    private async Task SendCommandResultAsync(NetworkStream stream, string commandId, string status)
    {
        JObject response = new()
        {
            ["message_type"] = MessageType.CommandResult.ToWire(),
            ["command_id"] = commandId,
            ["status"] = status
        };

        await SendJsonAsync(stream, response);
    }

    private async Task SendCommandErrorAsync(NetworkStream stream, string commandId, string errorMessage)
    {
        JObject response = new()
        {
            ["message_type"] = MessageType.Error.ToWire(),
            ["command_id"] = commandId,
            ["error_message"] = errorMessage
        };

        await SendJsonAsync(stream, response);
    }

    private async Task SendErrorAsync(NetworkStream stream, string errorMessage)
    {
        JObject response = new()
        {
            ["message_type"] = MessageType.Error.ToWire(),
            ["error_message"] = errorMessage
        };

        await SendJsonAsync(stream, response);
    }

    private async Task SendJsonAsync(NetworkStream stream, JObject payload)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(payload.ToString());
        await stream.WriteAsync(bytes, 0, bytes.Length);
    }
}
