// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord.Interactions;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

public class BotModule(ILogger<BotModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    public ILogger<BotModule> Logger { get; } = logger;

    public override void BeforeExecute(ICommandInfo command)
    {
        base.BeforeExecute(command);

        Logger.LogDebug("Executing command {ModuleName}::{CommandName}", command.Module.Name, command.MethodName);
    }

    public override void AfterExecute(ICommandInfo command)
    {
        base.AfterExecute(command);

        Logger.LogDebug("Executed command {ModuleName}::{CommandName}", command.Module.Name, command.MethodName);
    }
}


