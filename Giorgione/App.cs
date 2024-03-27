using System.Diagnostics;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Giorgione.Config;

using Serilog;
using Serilog.Events;

namespace Giorgione;

internal class App(DiscordSocketClient client, InteractionService interactionService, InteractionHandler interactionHandler, BotConfig config)
{
    internal async Task StartAsync()
    {
        client.Log += onLog;
        interactionService.Log += onLog;

        await interactionHandler.InitializeAsync();
        await client.LoginAsync(TokenType.Bot, config.Token);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private Task onLog(LogMessage message)
    {
        const string messageTemplate = "{Source} :: {Message}";

        var logLevel = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => throw new UnreachableException()
        };

        Log.Write(logLevel, message.Exception, messageTemplate, message.Source, message.Message);

        return Task.CompletedTask;
    }
}
