using System;
using System.Threading.Tasks;

public static class UnityMainThread
{
    public static async Task RunAsync(Action action, int timeoutMs = 2000)
    {
        TaskCompletionSource<bool> taskCompletionSource = new();

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                action();
                taskCompletionSource.SetResult(true);
            }
            catch (Exception exception)
            {
                taskCompletionSource.SetException(exception);
            }
        });

        if (await Task.WhenAny(taskCompletionSource.Task, Task.Delay(timeoutMs)) != taskCompletionSource.Task)
        {
            throw new TimeoutException("Main thread execution timeout.");
        }

        await taskCompletionSource.Task;
    }
}