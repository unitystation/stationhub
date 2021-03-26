using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace UnitystationLauncher.Infrastructure
{
    public static class ObservableExtensions
    {
        public static IObservable<TSource> ThrottleSubsequent<TSource>(
            this IObservable<TSource> source,
            TimeSpan dueTime)
        {
            return ThrottleSubsequent(source, dueTime, Scheduler.Default);
        }


        /// <summary>
        /// Based on: https://stackoverflow.com/a/42589839/10021384
        /// </summary>
        public static IObservable<TSource> ThrottleSubsequent<TSource>(
            this IObservable<TSource> source, 
            TimeSpan dueTime, 
            IScheduler scheduler)
        {
            return source.Publish(s => s
                .Window(() => s
                    .Select(x => Observable.Interval(dueTime, scheduler))
                    .Switch()
                ))
                .Publish(cooldownWindow =>
                    Observable.Merge(
                        cooldownWindow
                            .SelectMany(o => o.Take(1)),
                        cooldownWindow
                            .SelectMany(o => o.Skip(1))
                            .Throttle(dueTime, scheduler)
                    )
                );
        }
    }
}
