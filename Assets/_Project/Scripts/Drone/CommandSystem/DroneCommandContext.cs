public class DroneCommandContext
{
    public DroneControlAuthority ControlAuthority { get; }
    public DroneVideoStreamer VideoStreamer { get; }

    public DroneCommandContext(DroneControlAuthority controlAuthority, DroneVideoStreamer videoStreamer)
    {
        ControlAuthority = controlAuthority;
        VideoStreamer = videoStreamer;
    }
}
