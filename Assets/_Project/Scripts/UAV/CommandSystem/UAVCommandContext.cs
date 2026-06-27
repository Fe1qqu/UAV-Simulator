public class UAVCommandContext
{
    public UAVControlAuthority ControlAuthority { get; }
    public UAVVideoStreamer VideoStreamer { get; }

    /// <summary>
    /// Прямой доступ к контроллеру квадрокоптера для команд,
    /// которым нужно управлять углами точно (tilt, rotate).
    /// Может быть null, если контроллер не является QuadcopterController.
    /// </summary>
    public QuadcopterController Quadcopter { get; }

    public UAVCommandContext(
        UAVControlAuthority controlAuthority,
        UAVVideoStreamer videoStreamer,
        QuadcopterController quadcopter = null)
    {
        ControlAuthority = controlAuthority;
        VideoStreamer     = videoStreamer;
        Quadcopter        = quadcopter;
    }
}
