using System;
using System.Threading.Tasks;

namespace UnitystationLauncher.Infrastructure;

public static class TaskExtensions
{
    public static async Task AwaitWithTimeout<TSource>(this Task<TSource> task, TimeSpan timeout, Action<TSource> success)
    {
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
        {
            success(task.Result);
        }
        else
        {
            throw new OperationCanceledException();
        }
    }
}