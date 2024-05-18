// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;
using System.Text.RegularExpressions;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Giorgione.Config;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Giorgione.Workers;

public partial class EvalWatchdog : IHostedService
{
    private const string command_name = "g.eval";

    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly BotConfig _config;
    private readonly ILogger<EvalWatchdog> _logger;

    public EvalWatchdog(
        DiscordSocketClient client,
        InteractionService interactionService,
        BotConfig config,
        ILogger<EvalWatchdog> logger)
    {
        _config = config;
        _logger = logger;
        _client = client;
        _interactionService = interactionService;
    }

    [GeneratedRegex(@"^```\S*\s", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private partial Regex beginRegex();

    [GeneratedRegex(@"```$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private partial Regex endRegex();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived += eval;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived -= eval;

        return Task.CompletedTask;
    }

    private Task eval(SocketMessage arg)
    {
        if (arg.Author.Id != _config.SuperuserId || !arg.Content.StartsWith(command_name, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        Task.Run(async () =>
        {
            using var typing = arg.Channel.EnterTypingState();

            string code = arg.Content[command_name.Length..].Trim();

            code = beginRegex().Replace(code, string.Empty);
            code = endRegex().Replace(code, string.Empty);

            try
            {
                object? o = await CSharpScript.EvaluateAsync(code,
                    globals: new EvalContext(this),
                    globalsType: typeof(EvalContext),
                    options: ScriptOptions.Default
                        .WithReferences(typeof(Enumerable).Assembly, typeof(HashSet<int>).Assembly, typeof(Span<char>).Assembly)
                        .WithImports("System", "System.IO", "System.Linq", "System.Collections.Generic"));

                string result = o.ToString() ?? "null";

                if (result.Length < 2000)
                {
                    var embed = new EmbedBuilder()
                        .WithColor(Color.DarkGreen)
                        .WithTitle("Script Result")
                        .WithDescription($"```cs\n{result}\n```")
                        .Build();

                    await arg.Channel.SendMessageAsync(embed: embed, messageReference: arg.Reference);
                }
                else
                {
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(result));

                    await arg.Channel.SendFileAsync(stream, "out.txt");
                }
            }
            catch (CompilationErrorException e)
            {
                string diag = string.Join(Environment.NewLine, e.Diagnostics);

                _logger.LogError(e, "Compilation error in eval command");

                var embed = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithTitle("Compilation Error")
                    .WithDescription($"```\n{diag}\n```")
                    .Build();

                await arg.Channel.SendMessageAsync(embed: embed);
            }
        });

        return Task.CompletedTask;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // Required for script evaluation
    public class EvalContext(EvalWatchdog context)
    {
        public DiscordSocketClient Client { get; } = context._client;
        public InteractionService InteractionService { get; } = context._interactionService;
    }
}
