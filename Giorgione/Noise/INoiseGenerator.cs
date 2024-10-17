// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Noise;

public interface INoiseGenerator
{
    /// <summary>
    /// Generate noise samples at the desired amplitude.
    /// </summary>
    /// <param name="amplitude">The noise amplitude between <c>0.0</c> and <c>1.0</c></param>
    /// <returns>The <see cref="Memory{T}"/> buffer of the samples.</returns>
    /// <remarks>The format is 16bit 48kHz Stereo PCM.</remarks>
    ReadOnlyMemory<byte> Generate(double amplitude);
}
