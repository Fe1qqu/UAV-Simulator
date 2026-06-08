using System.Threading;
using System.Threading.Tasks;

public abstract class UAVCommandBase : IUAVCommand
{
    public abstract Task ExecuteAsync(UAVCommandContext context, CancellationToken token);
}
