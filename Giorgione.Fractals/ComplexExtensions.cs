// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Giorgione.Fractals;

internal static class ComplexExtensions
{
    internal static double SquareNorm(this Complex c)
    {
        return c.Imaginary * c.Imaginary +  c.Real * c.Real;
    }
}
