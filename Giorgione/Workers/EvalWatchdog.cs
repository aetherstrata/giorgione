// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Giorgione.Config;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;
using Quartz.Core;

namespace Giorgione.Workers;

public class EvalWatchdog(
    DiscordSocketClient client,
    InteractionService interactionService,
    BotConfig config,
    ISchedulerFactory  schedulerFactory,
    ILogger<EvalWatchdog> logger)
    : IHostedService
{
    private const string command_name = "g.eval";

    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _interactionService = interactionService;
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;

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
        if (arg.Author.Id != config.SuperuserId || !arg.Content.StartsWith(command_name, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        return Task.Run(async () =>
        {
            var typing = arg.Channel.EnterTypingState();

            try
            {
                object? o = await CSharpScript.EvaluateAsync(cleanMessage(arg.Content),
                    globals: await EvalContext.Create(this),
                    globalsType: typeof(EvalContext),
                    options: ScriptOptions.Default
                        .WithReferences(typeof(Enumerable).Assembly, typeof(HashSet<int>).Assembly, typeof(Span<char>).Assembly, typeof(QuartzScheduler).Assembly)
                        .WithImports("System", "System.IO", "System.Linq", "System.Text", "System.Collections.Generic", "Quartz"));

                string result = o?.ToString() ?? "void";

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
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(result));

                    await arg.Channel.SendFileAsync(stream, "out.txt");
                }
            }
            catch (CompilationErrorException e)
            {
                string diag = string.Join('\n', e.Diagnostics);

                logger.LogError(e, "Compilation error in eval command");

                var embed = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithTitle("Compilation Error")
                    .WithDescription($"```\n{diag}\n```")
                    .Build();

                await arg.Channel.SendMessageAsync(embed: embed);
            }
            finally
            {
                typing.Dispose();
            }
        });
    }

    private static string cleanMessage(ReadOnlySpan<char> message)
    {
        message = message[command_name.Length..].Trim();

        // Remove code block quotes
        if (message.StartsWith("```") && message.EndsWith("```"))
        {
            message = message[message.IndexOf('\n')..^3];
        }

        return message.ToString();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // Required for script evaluation
    public class EvalContext
    {
        public DiscordSocketClient Client { get; }
        public InteractionService InteractionService { get; }
        public IScheduler Scheduler { get; }

        private EvalContext(DiscordSocketClient client, InteractionService interactionService, IScheduler scheduler)
        {
            Client = client;
            InteractionService = interactionService;
            Scheduler = scheduler;
        }

        internal static async Task<EvalContext> Create(EvalWatchdog context)
        {
            var scheduler = await context._schedulerFactory.GetScheduler();
            return new EvalContext(context._client, context._interactionService, scheduler);
        }
    }
}
