// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Giorgione.Workers;

using Microsoft.Extensions.DependencyInjection;

using Quartz;

namespace Giorgione.Config;

internal static class Scheduling
{
    internal static IServiceCollection AddScheduling(this IServiceCollection services, string? connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddQuartz(config =>
        {
            config.SchedulerName = "JobScheduler";
            config.UseDefaultThreadPool();
            config.UsePersistentStore(store =>
            {
                store.UseProperties = true;
                store.UsePostgres(connectionString);
                store.UseNewtonsoftJsonSerializer();
            });

            config.AddJob<AnimeFeedChecker>(job => job
                .WithIdentity(nameof(AnimeFeedChecker))
                .WithDescription("Check AnimeWorld feed for new published works"));

            config.AddJob<BirthdateChecker>(job => job
                .WithIdentity(nameof(BirthdateChecker))
                .WithDescription("Check for birthdays occurring on this day of the year"));

            config.AddTrigger(trigger => trigger
                .WithIdentity("every-midnight")
                .WithDescription("Trigger a background task everyday at midnight")
                .ForJob(nameof(BirthdateChecker))
                .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0,0)));

            config.AddTrigger(trigger => trigger
                .WithIdentity("every-5min")
                .WithDescription("Trigger a background task every 5 minutes")
                .ForJob(nameof(AnimeFeedChecker))
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(5)));
        });

        services.AddQuartzHostedService(config =>
        {
            config.WaitForJobsToComplete = true;
        });

        return services;
    }
}
