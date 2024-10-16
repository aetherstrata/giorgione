// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Giorgione.Noise;

public interface INoiseGenerator
{
    /// <summary>
    /// Get the PCM samples buffer in memory.
    /// </summary>
    /// <returns>the buffer to read from.</returns>
    ReadOnlyMemory<byte> GetBuffer();

    /// <summary>
    /// Generate noise samples and place them in the memory buffer.
    /// </summary>
    /// <param name="amplitude">The noise amplitude between <c>0.0</c> and <c>1.0</c></param>
    /// <returns>the amount of bytes written.</returns>
    int Generate(double amplitude);
}
