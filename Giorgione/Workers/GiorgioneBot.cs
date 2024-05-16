using System.Diagnostics;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Giorgione.Config;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Giorgione.Workers;

public class GiorgioneBot(
    DiscordSocketClient client,
    InteractionService interactionService,
    InteractionHandler interactionHandler,
    BotConfig config,
    ILogger<GiorgioneBot> logger)
    : IHostedService
{
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        client.Log += onLog;
        interactionService.Log += onLog;

        return interactionHandler.InitializeAsync()
            .ContinueWith(task => client.LoginAsync(TokenType.Bot, config.Token), cancellationToken)
            .ContinueWith(task => client.StartAsync(), cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return client.StopAsync();
    }

    private Task onLog(LogMessage message)
    {
        const string messageTemplate = "{Source} :: {Message}";

        var logLevel = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Verbose => LogLevel.Trace,
            _ => throw new UnreachableException()
        };

        logger.Log(logLevel, message.Exception, messageTemplate, message.Source, message.Message);

        return Task.CompletedTask;
    }
}
