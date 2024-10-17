// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Noise;

public class BrownNoiseGenerator(int bufferSeconds) : NoiseGenerator(bufferSeconds)
{
    /// Leaky integrator constant
    private const double c = 0.99;

    /// <inheritdoc />
    public override ReadOnlyMemory<byte> Generate(double amplitude)
    {
        double sample = 0.0;

        for (int i = 0; i < Buffer.Length / Depth; i++)
        {
            sample = c * sample + (1 - c) * Tick();

            short pcmValue = (short)(sample * amplitude * short.MaxValue);

            // Convert the sample to little-endian
            Buffer.Span[i * Depth] = (byte)(pcmValue & 0xFF);
            Buffer.Span[i * Depth + 1] = (byte)((pcmValue >> 8) & 0xFF);
        }

        return Buffer;
    }
}
