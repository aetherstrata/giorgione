using System.Diagnostics;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Giorgione.Config;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
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
    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _interactionService = interactionService;
    private readonly BotConfig _config = config;

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += onLog;
        _interactionService.Log += onLog;

        _client.MessageReceived += eval;

        return interactionHandler.InitializeAsync()
            .ContinueWith(task => _client.LoginAsync(TokenType.Bot, _config.Token), cancellationToken)
            .ContinueWith(task => _client.StartAsync(), cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return _client.StopAsync();
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

    private Task eval(SocketMessage arg)
    {
        const string command = "g.eval";

        if (arg.Author.Id != _config.SuperuserId || !arg.Content.StartsWith(command, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        Task.Run(async () =>
        {
            using var typing = arg.Channel.EnterTypingState();

            try
            {
                object? o = await CSharpScript.EvaluateAsync(arg.CleanContent[command.Length..].Trim(),
                        globals: new EvalContext(this),
                        globalsType: typeof(EvalContext),
                        options: ScriptOptions.Default
                            .WithReferences(typeof(Enumerable).Assembly, typeof(HashSet<int>).Assembly)
                            .WithImports("System.IO", "System.Linq", "System.Collections.Generic"));

                string? result = o.ToString();

                await arg.Channel.SendMessageAsync(result, messageReference: arg.Reference);
            }
            catch (CompilationErrorException e)
            {
                await arg.Channel.SendMessageAsync(string.Join(Environment.NewLine, e.Diagnostics));
            }
        });

        return Task.CompletedTask;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // Required for script evaluation
    public class EvalContext(GiorgioneBot context)
    {
        public DiscordSocketClient Client { get; } = context._client;
        public InteractionService InteractionService { get; } = context._interactionService;
        public BotConfig Config { get; } = context._config;
    }
}
