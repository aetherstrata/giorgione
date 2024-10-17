// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Noise;

public abstract class NoiseGenerator : INoiseGenerator
{
    // Discord Audio requires stereo PCM
    private const int channels = 2;

    private readonly Random _random = new();

    protected readonly Memory<byte> Buffer;
    protected readonly int SampleRate;
    protected readonly int Duration;

    protected NoiseGenerator(int sampleRate, int seconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seconds);

        Buffer = new byte[sampleRate * seconds * channels * 2].AsMemory(); // s16le = 2 bytes

        SampleRate = sampleRate;
        Duration = seconds;
    }

    /// <summary>
    /// Generate a new audio sample
    /// </summary>
    /// <returns>the sample value between <c>-1.0</c> and <c>1.0</c>.</returns>
    protected double Tick() => _random.NextDouble() * 2.0 - 1.0;

    /// <inheritdoc />
    public ReadOnlyMemory<byte> GetBuffer() => Buffer;

    /// <inheritdoc />
    public abstract int Generate(double amplitude);
}
