// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Giorgione;

internal class InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, BotConfig config)
{
    internal async Task InitializeAsync()
    {
        // Process when the client is ready, so we can register our commands.
        client.Ready += OnReady;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        // Process the InteractionCreated payloads to execute Interactions commands
        client.InteractionCreated += OnInteractionCreated;
    }

    private async Task OnReady()
    {
        // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
        // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
        if (Program.IsDebug)
            await handler.RegisterCommandsToGuildAsync(config.GuildId);
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

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await interaction.RespondAsync("Condizione non soddisfatta");
                        // implement
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
}
