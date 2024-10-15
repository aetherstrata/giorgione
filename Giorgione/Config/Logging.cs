// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Events;

namespace Giorgione.Config;

internal static class Logging
{
    internal static IServiceCollection AddSerilog(this IServiceCollection services)
    {
        const string outputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u4}] {Message:lj} <{ThreadId}><{ThreadName}>{NewLine}{Exception}";

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File(AppContext.BaseDirectory + "log-.txt", LogEventLevel.Debug, outputTemplate,
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        return services.AddLogging(log => log.AddSerilog(logger, true));
    }
}
