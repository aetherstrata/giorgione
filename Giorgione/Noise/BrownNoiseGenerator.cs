// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Noise;

public class BrownNoiseGenerator(int sampleRate, int seconds) : NoiseGenerator(sampleRate, seconds)
{
    private const double beta = 0.025;

    /// <inheritdoc />
    public override int Generate(double amplitude)
    {
        for (int time = 0; time < Duration; time++)
        {
            double sample = Tick();

            for (int i = 0; i < Buffer.Length / 2; i++)
            {
                sample -= beta * (sample - Tick());

                short pcmValue = (short)(sample * amplitude * short.MaxValue);

                // Convert the sample to little-endian
                Buffer.Span[time * SampleRate + i * 2] = (byte)(pcmValue & 0xFF);
                Buffer.Span[time * SampleRate + i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
            }
        }

        return Buffer.Length;
    }
}
