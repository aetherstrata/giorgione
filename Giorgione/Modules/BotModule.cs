// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;

using Discord.Interactions;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

public class BotModule(ILogger<BotModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    protected ILogger<BotModule> Logger { get; } = logger;

    /// <inheritdoc />
    public override void BeforeExecute(ICommandInfo command)
    {
        base.BeforeExecute(command);

        Logger.LogDebug("Executing command {ModuleName}::{CommandName}", command.Module.Name, command.MethodName);
    }

    /// <inheritdoc />
    public override void AfterExecute(ICommandInfo command)
    {
        base.AfterExecute(command);

        Logger.LogDebug("Executed command {ModuleName}::{CommandName}", command.Module.Name, command.MethodName);
    }

    /// <summary>
    /// Respond to the interaction with a generic error message
    /// </summary>
    protected Task RespondError(string title, string message, [CallerMemberName] string methodName = "")
    {
        Logger.LogDebug("Error on command {Module}::{Method}: {Message}", GetType(), methodName, message);

        return RespondAsync(embed: Embeds.GenericError(title, message));
    }
}


