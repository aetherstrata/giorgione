// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Noise;

public class WhiteNoiseGenerator(int sampleRate, int seconds) : NoiseGenerator(sampleRate, seconds)
{
    /// <inheritdoc />
    public override int Generate(double amplitude)
    {
        for (int i = 0; i < Buffer.Length / 2; i++)
        {
            short pcmValue = (short)(Tick() * amplitude * short.MaxValue);

            // Convert the sample to little-endian
            Buffer.Span[i * 2] = (byte)(pcmValue & 0xFF);
            Buffer.Span[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
        }

        return Buffer.Length;
    }
}
