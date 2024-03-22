// See https://aka.ms/new-console-template for more information

using System.Text.Json;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Events;

namespace Giorgione;

public static class Program
{
    public const bool IsDebug =
#if DEBUG
        true;
#else
        false;
#endif

    private const string output_template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u4}] {Message:lj} <{ThreadId}><{ThreadName}>{NewLine}{Exception}";

    public static async Task Main(string[] args)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.Console(LogEventLevel.Debug, output_template)
            .WriteTo.File(AppContext.BaseDirectory + "log-.txt", LogEventLevel.Debug, output_template, rollingInterval: RollingInterval.Day);

        Log.Logger = loggerConfig.CreateLogger();

        var services = new ServiceCollection()
            .AddHttpClient()
            .AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            })
            .AddSingleton<BotConfig>(_ =>
            {
                byte[] json = File.ReadAllBytes("config.json");
                var config = JsonSerializer.Deserialize(json, JsonContext.Default.BotConfig);
                return config ?? throw new InvalidDataException("The config file has an invalid format");
            })
            .AddSingleton(_ => new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers |
                                 GatewayIntents.GuildPresences | GatewayIntents.MessageContent,
                DefaultRetryMode = RetryMode.AlwaysFail,
                AuditLogCacheSize = 15,
                MessageCacheSize = 50,
            })
            .AddSingleton(_ => new InteractionServiceConfig
            {
                UseCompiledLambda = true
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<InteractionService>()
            .AddSingleton<InteractionHandler>()
            .AddSingleton<App>()
            .BuildServiceProvider();

        await services.GetRequiredService<App>().StartAsync();
    }
}
