// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.Extensions.DependencyInjection;

using Quartz;

namespace Giorgione.Config;

internal static class Scheduling
{
    internal static IServiceCollection AddScheduling(this IServiceCollection services, string? connectionString)
    {
        services.AddQuartz(config =>
        {
            config.SchedulerName = "JobScheduler";
            config.UseDefaultThreadPool();
            config.UsePersistentStore(store =>
            {
                store.UseProperties = true;
                store.UsePostgres(connectionString ?? throw new InvalidOperationException("Could not read Quartz's connection string"));
                store.UseNewtonsoftJsonSerializer();
            });

            var bcJobKey = JobKey.Create(nameof(BirthdateChecker));

            config.AddJob<BirthdateChecker>(bcJobKey);
            config.AddTrigger(trigger => trigger
                .ForJob(bcJobKey)
                .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0,0)));
        });

        services.AddQuartzHostedService();

        return services;
    }
}
