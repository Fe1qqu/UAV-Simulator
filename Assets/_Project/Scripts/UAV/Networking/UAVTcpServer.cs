using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UavSim.UAV.Protocol;

public class UAVTcpServer : MonoBehaviour
{
    [SerializeField] private UAVCommandController uavCommandController;

    [SerializeField] private int bufferSize = 4096;
    [SerializeField] private int port = 9000;

    private TcpListener tcpListener;

    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        if (uavCommandController == null)
        {
            Debug.LogError($"[UAVTcpServer] UAVCommandController is not assigned.");
        }
    }

    private void Start()
    {
        if (!UnityMainThreadDispatcher.IsInitialized)
        {
            Debug.LogError("[UAVTcpServer] UnityMainThreadDispatcher is missing. Server will not start.");
            enabled = false;
            return;
        }

        tcpListener = new TcpListener(IPAddress.Loopback, port);
        tcpListener.Start();

        cancellationTokenSource = new CancellationTokenSource();
        _ = AcceptClientsLoopAsync(cancellationTokenSource.Token);

        Debug.Log("[UAVTcpServer] UAV server started on 127.0.0.1:9000.");
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        tcpListener.Stop();
    }

    private async Task AcceptClientsLoopAsync(CancellationToken cancellationToken)
    {
        // Register the TcpListener stop once
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

                Debug.Log("[UAVTcpServer] Client connected.");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleClientAsync(tcpClient);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"[UAVTcpServer] Client session error: {exception}.");
                    }
                    finally
                    {
                        Debug.Log("[UAVTcpServer] Client disconnected.");

                        UnityMainThreadDispatcher.Enqueue(() => uavCommandController.ResetState());

                        tcpClient.Close();
                    }
                });
            }
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        using NetworkStream networkStream = tcpClient.GetStream();
        byte[] buffer = new byte[bufferSize];

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

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                uavCommandController.EnqueueCommand(commandName, arguments);
                completionSource.SetResult(true);
            }
            catch (Exception exception)
            {
                completionSource.SetException(exception);
            }
        });

        try
        {
            await completionSource.Task;
            await SendCommandResultAsync(stream, commandId, "ok");
        }
        catch (Exception exception)
        {
            await SendCommandErrorAsync(stream, commandId, exception.Message);
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
