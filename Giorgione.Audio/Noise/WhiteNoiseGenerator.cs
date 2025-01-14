// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Audio.Noise;

public class WhiteNoiseGenerator(int bufferSeconds) : NoiseGenerator(bufferSeconds)
{
    /// <inheritdoc />
    public override ReadOnlyMemory<byte> Generate(double amplitude)
    {
        for (int i = 0; i < Buffer.Length / Depth; i++)
        {
            short pcmValue = (short)(Tick() * amplitude * short.MaxValue);

            // Convert the sample to little-endian
            Buffer.Span[i * Depth] = (byte)(pcmValue & 0xFF);
            Buffer.Span[i * Depth + 1] = (byte)((pcmValue >> 8) & 0xFF);
        }

        return Buffer;
    }
}
