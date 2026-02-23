using System.Threading;
using System.Threading.Tasks;

public abstract class DroneCommandBase : IDroneCommand
{
    public abstract Task ExecuteAsync(DroneCommandContext context, CancellationToken token);
}
