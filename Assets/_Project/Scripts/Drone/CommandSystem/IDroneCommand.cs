using System.Threading;
using System.Threading.Tasks;

public interface IDroneCommand
{
    Task ExecuteAsync(DroneCommandContext context, CancellationToken token);
}
