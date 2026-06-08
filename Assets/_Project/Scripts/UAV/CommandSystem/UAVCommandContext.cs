public class UAVCommandContext
{
    public UAVControlAuthority ControlAuthority { get; }
    public UAVVideoStreamer VideoStreamer { get; }

    public UAVCommandContext(UAVControlAuthority controlAuthority, UAVVideoStreamer videoStreamer)
    {
        ControlAuthority = controlAuthority;
        VideoStreamer = videoStreamer;
    }
}
