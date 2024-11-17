// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Audio.Generators;

public abstract class NoiseGenerator : INoiseGenerator
{
    private readonly Random _random = new();

    /// Discord Audio requires Stereo PCM
    public const int Channels = 2;

    /// Samples are quantised with 16 bits | s16le
    public const int Depth = 2;

    public const int SamplingRate = 48000;

    protected readonly Memory<byte> Buffer;

    protected NoiseGenerator(int bufferSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSeconds);

        Buffer = new byte[bufferSeconds * SamplingRate * Channels * Depth].AsMemory();
    }

    /// <summary>
    /// Generate a new audio sample
    /// </summary>
    /// <returns>the sample value between <c>-1.0</c> and <c>1.0</c>.</returns>
    protected double Tick() => _random.NextDouble() * 2.0 - 1.0;

    /// <inheritdoc />
    public abstract ReadOnlyMemory<byte> Generate(double amplitude);
}
