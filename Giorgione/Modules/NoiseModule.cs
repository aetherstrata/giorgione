// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.Audio;
using Discord.Interactions;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("noise", "For all your noisy needs")]
public class NoiseModule(ILogger<NoiseModule> logger) : BotModule(logger)
{
    private const int channels = 2;
    private const int sample_rate = 48000;

    private static readonly Random random = new();

    [SlashCommand("join", "Play white noise in your voice channel.", runMode: RunMode.Async)]
    public async Task Join(IVoiceChannel? channel = null)
    {
        channel ??= (Context.User as IGuildUser)?.VoiceChannel;

        if (channel == null)
        {
            await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
            return;
        }

        await RespondAsync($"Making noise in channel: {channel.Name}");

        var audioClient = await channel.ConnectAsync();

        await using (var discord = audioClient.CreatePCMStream(AudioApplication.Music))
        {
            Memory<byte> buffer = new byte[sample_rate * channels * 2].AsMemory(); // s16le = 2 bytes

            while (audioClient.ConnectionState == ConnectionState.Connected)
            {
                for (int i = 0; i < sample_rate * channels; i++)
                {
                    double sample = random.NextDouble() * 2.0 - 1.0;
                    short pcmValue = (short)(sample * 0.1 * short.MaxValue);

                    // Convert the sample to little-endian
                    buffer.Span[i * 2] = (byte)(pcmValue & 0xFF);
                    buffer.Span[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
                }

                await discord.WriteAsync(buffer);
            }
        }

        await channel.DisconnectAsync();
    }
}
