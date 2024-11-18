// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Giorgione.Config;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Giorgione;

public class InteractionHandler(
    DiscordSocketClient client,
    InteractionService handler,
    BotConfig config,
    ILogger<InteractionHandler> logger,
    IServiceProvider services,
    IHostEnvironment environment)
{
    internal async Task InitializeAsync()
    {
        // Process when the client is ready, so we can register our commands.
        client.Ready += OnReady;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        using (var scope = services.CreateScope())
        {
            await handler.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);
        }

        // Process the InteractionCreated payloads to execute Interactions commands
        client.InteractionCreated += OnInteractionCreated;

        // Handle errors in async execution contexts
        handler.SlashCommandExecuted += OnSlashCommandExecuted;
    }

    private async Task OnReady()
    {
        // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
        // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
        if (environment.IsDevelopment())
            await handler.RegisterCommandsToGuildAsync(config.TestGuildId);
        else
            await handler.RegisterCommandsGloballyAsync(true);
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(client, interaction);

            // Execute the incoming command.
            var result = await handler.ExecuteCommandAsync(context, services);

            // Handle RunMode.Sync command results
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await interaction.RespondAsync("Condizione non soddisfatta");
                        // implement
                        break;

                    case InteractionCommandError.Exception:
                        await interaction.RespondAsync("Something went wrong");
                        break;
                }
            }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction
                    .GetOriginalResponseAsync()
                    .ContinueWith(msg => msg.Result.DeleteAsync());
            }
        }
    }

    private async Task OnSlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess) return;

        switch (result.Error)
        {
            case InteractionCommandError.UnknownCommand:
                logger.LogError("Command {ModuleName}::{CommandName} failed: unknown command", commandInfo.Module.Name, commandInfo.Name);
                await context.Interaction.RespondAsync(embed: Embeds.GenericError("Error", "Unknown command provided"));
                break;
            case InteractionCommandError.Exception:
                await context.Interaction.RespondAsync(embed: Embeds.GenericError("Error", "An unhandled exception occurred"));
                break;
            case InteractionCommandError.UnmetPrecondition:
                logger.LogError("Command {ModuleName}::{CommandName} failed: unmet preconditions", commandInfo.Module.Name, commandInfo.Name);
                var preconditionResult = (PreconditionResult) result;
                await context.Interaction.RespondAsync(embed: Embeds.GenericError("Unmet precondition", preconditionResult.ErrorReason));
                break;
        }
    }
}
