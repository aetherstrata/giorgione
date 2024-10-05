// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;

using Discord;
using Discord.Net;

using Microsoft.Extensions.Logging;

namespace Giorgione;

public static class DiscordExtensions
{
    /// <summary>
    /// Respond to the interaction with the error of the request
    /// </summary>
    public static Task RespondRequestError<T>(this T module, HttpException ex) where T : BotModule
    {
        var embed = new EmbedBuilder()
            .WithTitle("Request Error")
            .WithDescription(ex.Reason)
            .WithColor(Color.Red)
            .Build();

        return module.Context.Interaction.RespondAsync(embed: embed);
    }

    /// <summary>
    /// Respond to the interaction with a generic error message
    /// </summary>
    public static Task RespondUnhandledError<T>(this T module, Exception ex) where T : BotModule
    {
        var embed = new EmbedBuilder()
            .WithTitle("Internal Error")
            .WithDescription("An internal error occurred. Try again later.")
            .WithColor(Color.Red)
            .Build();

        return module.Context.Interaction.RespondAsync(embed: embed);
    }

    public static async Task TryRequestAsync<T>(
        this T module,
        Func<T, Task> taskFactory,
        [CallerMemberName] string? callerName = null) where T : BotModule
    {
        try
        {
            await taskFactory(module).ConfigureAwait(false);
        }
        catch (HttpException e)
        {
            module.Logger.LogError(e, "A request error occurred while processing {Member}", callerName);

            await module.RespondRequestError(e);
        }
        catch (Exception e)
        {
            module.Logger.LogError(e, "An unhandled error occurred while processing {Member}", callerName);

            await module.RespondUnhandledError(e);
        }
    }

    public static async Task<TResult?> TryRequestAsync<T, TResult>(
        this T module,
        Func<T, Task<TResult>> taskFactory,
        TResult? fallback = default,
        [CallerMemberName] string? callerName = null) where T : BotModule
    {
        try
        {
            return await taskFactory(module).ConfigureAwait(false);
        }
        catch (HttpException e)
        {
            module.Logger.LogError(e, "A request error occurred while processing {Member}", callerName);

            await module.RespondRequestError(e);
        }
        catch (Exception e)
        {
            module.Logger.LogError(e, "An unhandled error occurred while processing {Member}", callerName);

            await module.RespondUnhandledError(e);
        }

        return fallback;
    }
}
