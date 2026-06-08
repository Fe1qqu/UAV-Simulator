using System.Threading;
using System.Threading.Tasks;

public interface IUAVCommand
{
    Task ExecuteAsync(UAVCommandContext context, CancellationToken token);
}
