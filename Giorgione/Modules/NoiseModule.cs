// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.Interactions;

using Giorgione.Noise;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("noise", "For all your noisy needs")]
public class NoiseModule(ILogger<NoiseModule> logger) : BotModule(logger)
{
    [SlashCommand("white", "Play white noise in your voice channel.", runMode: RunMode.Async)]
    public async Task PlayWhiteAsync(IVoiceChannel? channel = null)
    {
        channel ??= (Context.User as IGuildUser)?.VoiceChannel;

        if (channel == null)
        {
            await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
            return;
        }

        await RespondAsync($"Making noise in channel: {channel.Name}");

        var audioClient = await channel.ConnectAsync();

        await audioClient.PlayWhiteNoise(0.1);
    }

    [SlashCommand("brown", "Play brown noise in your voice channel.", runMode: RunMode.Async)]
    public async Task PlayBrownAsync(IVoiceChannel? channel = null)
    {
        channel ??= (Context.User as IGuildUser)?.VoiceChannel;

        if (channel == null)
        {
            await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
            return;
        }

        await RespondAsync($"Making noise in channel: {channel.Name}");

        var audioClient = await channel.ConnectAsync();

        await audioClient.PlayBrownNoise(0.3);
    }
}
