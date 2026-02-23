using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

public class DroneVideoStreamer : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera droneCamera;

    [Header("Stream settings")]
    [SerializeField] private int fps = 60;
    [SerializeField] private int jpegQuality = 70;

    [Header("Network")]
    [SerializeField] private int udpPort = 11111;
    [SerializeField] private int maxPacketSize = 1200;

    private int width;
    private int height;

    private RenderTexture renderTexture;
    private Texture2D readTexture;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    private Coroutine streamCoroutine;
    private bool isStreaming;

    void Awake()
    {
        if (droneCamera == null)
        {
            Debug.LogError("[DroneVideoStreamer] Drone—amera is not assigned.");
            enabled = false;
            return;
        }

        width = Screen.width;
        height = Screen.height;

        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderTexture.Create();

        readTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        droneCamera.targetTexture = renderTexture;

        remoteEndPoint = new IPEndPoint(IPAddress.Loopback, udpPort);
    }

    private void OnDestroy()
    {
        StreamOff();
        droneCamera.targetTexture = null;
        renderTexture.Release();
    }

    // -------- Internal --------

    private IEnumerator StreamLoop()
    {
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        float frameDelay = 1f / fps;
        int frameId = 0;

        while (isStreaming)
        {
            yield return waitForEndOfFrame;

            RenderTexture.active = renderTexture;
            readTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            readTexture.Apply(false);
            RenderTexture.active = null;

            byte[] jpeg = readTexture.EncodeToJPG(jpegQuality);

            // Breaking into fragments
            int totalPackets = (jpeg.Length + maxPacketSize - 1) / maxPacketSize;

            for (int i = 0; i < totalPackets; i++)
            {
                int offset = i * maxPacketSize;
                int size = Math.Min(maxPacketSize, jpeg.Length - offset);
                byte[] packet = new byte[size + 6]; // 2 bytes frame id + 2 bytes packet number + 2 bytes totalPackets
                packet[0] = (byte)(frameId >> 8);
                packet[1] = (byte)(frameId & 0xFF);
                packet[2] = (byte)(i >> 8);
                packet[3] = (byte)(i & 0xFF);
                packet[4] = (byte)(totalPackets >> 8);
                packet[5] = (byte)(totalPackets & 0xFF);
                Array.Copy(jpeg, offset, packet, 6, size);

                try
                {
                    udpClient.Send(packet, packet.Length, remoteEndPoint);
                }
                catch (SocketException socketException)
                {
                    Debug.LogError($"[DroneVideoStreamer] UDP send error: {socketException.Message}.");
                }
            }

            frameId = (frameId + 1) % 65536; // Limit frameId to 16 bits
            yield return new WaitForSeconds(frameDelay);
        }
    }

    // -------- Public API --------

    public void StreamOn()
    {
        if (isStreaming)
        {
            return;
        }

        Debug.Log("[DroneVideoStreamer] Stream ON.");

        udpClient = new UdpClient();
        isStreaming = true;
        streamCoroutine = StartCoroutine(StreamLoop());
    }

    public void StreamOff()
    {
        if (!isStreaming)
        {
            return;
        }

        Debug.Log("[DroneVideoStreamer] Stream OFF.");

        isStreaming = false;

        if (streamCoroutine != null)
        {
            StopCoroutine(streamCoroutine);
            streamCoroutine = null;
        }

        udpClient?.Close();
        udpClient = null;
    }
}
