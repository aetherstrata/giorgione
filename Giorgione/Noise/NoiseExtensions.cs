// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using Discord.Audio;

using ConnectionState = Discord.ConnectionState;

namespace Giorgione.Noise;

public static class NoiseExtensions
{
    public static async Task PlayWhiteNoise(this IAudioClient client, double amplitude = 0.5, CancellationToken ct = default)
    {
        await using var discordStream = client.CreatePCMStream(AudioApplication.Music);

        var noise = new WhiteNoiseGenerator(48000, 4);

        while (client.ConnectionState == ConnectionState.Connected)
        {
            noise.Generate(amplitude);
            await discordStream.WriteAsync(noise.GetBuffer(), ct);
        }
    }

    public static async Task PlayBrownNoise(this IAudioClient client, double amplitude = 0.5, CancellationToken ct = default)
    {
        await using var discordStream = client.CreatePCMStream(AudioApplication.Music);

        var noise = new BrownNoiseGenerator(48000, 4);

        while (client.ConnectionState == ConnectionState.Connected)
        {
            noise.Generate(amplitude);
            await discordStream.WriteAsync(noise.GetBuffer(), ct);
        }
    }
}
