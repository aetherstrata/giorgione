// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord;
using Discord.Audio;

using Giorgione.Audio.Generators;

namespace Giorgione.Extensions;

public static class NoiseExtensions
{
    public static async Task PlayWhiteNoise(this IAudioClient client, double amplitude = 0.5, CancellationToken ct = default)
    {
        await using var discordStream = client.CreatePCMStream(AudioApplication.Music);

        var noise = new WhiteNoiseGenerator(4);

        while (client.ConnectionState == ConnectionState.Connected && !ct.IsCancellationRequested)
        {
            await discordStream.WriteAsync(noise.Generate(amplitude), ct);
        }
    }

    public static async Task PlayBrownNoise(this IAudioClient client, double amplitude = 0.5, CancellationToken ct = default)
    {
        await using var discordStream = client.CreatePCMStream(AudioApplication.Music);

        var noise = new BrownNoiseGenerator(4);

        while (client.ConnectionState == ConnectionState.Connected && !ct.IsCancellationRequested)
        {
            await discordStream.WriteAsync(noise.Generate(amplitude), ct);
        }
    }
}
