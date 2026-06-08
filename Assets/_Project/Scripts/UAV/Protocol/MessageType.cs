namespace UavSim.UAV.Protocol
{
    public enum MessageType
    {
        Command,
        CommandResult,
        Error
        //Log
    }

    public static class MessageTypeProtocol
    {
        public static string ToWire(this MessageType type)
        {
            return type switch
            {
                MessageType.Command => "command",
                MessageType.CommandResult => "command_result",
                MessageType.Error => "error",
                //MessageType.Log => "log",
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }
    }
}
