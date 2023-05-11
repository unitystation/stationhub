using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using UnitystationLauncher.Infrastructure;
using Xunit;

namespace UnitystationLauncher.Tests.Infrastructure;

public static class TaskExtensionsTests
{
    #region Task.AwaitWithTimeout
    [Fact]
    public static void AwaitWithTimeout_TimeoutsShouldThrow()
    {
        Func<Task> act =
            async () => await Task.Run(InfiniteWait).AwaitWithTimeout(TimeSpan.FromMilliseconds(100), _ => { });

        act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public static void AwaitWithTimeout_ActionShouldRunOnTaskCompletion()
    {
        Func<Task> act = async () =>
            await Task.Run(() => Wait(100)).AwaitWithTimeout(TimeSpan.FromMilliseconds(200),
                success =>
                {
                    success.Should().BeTrue();
                });

        act.Should().NotThrowAsync();
    }
    #endregion

    private static bool Wait(int milliseconds)
    {
        Thread.Sleep(milliseconds);
        return true;
    }

    private static bool InfiniteWait()
    {
        while (true) ;
#pragma warning disable CS0162
        return true;
#pragma warning restore CS0162
    }
}