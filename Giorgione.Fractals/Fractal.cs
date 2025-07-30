// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Giorgione.Fractals;

public delegate int FractalFun(Complex c, int maxIter);

public static class Fractal
{
    public static FractalFun Mandelbrot() => (c, maxIter) =>
    {
        var z = Complex.Zero;
        int iter = 0;

        while (z.SquareNorm() <= 4.0 && iter < maxIter)
        {
            z = z * z + c;
            iter++;
        }

        return iter;
    };

    public static FractalFun MarekDragon(Complex? zc = null)
    {
        zc ??= new(
            0.967185908568486410443674886477546272891414928183791109030967706864635411,
            0.254069711430842235165028021915023308737438569083612090539458212495255093
        );

        var r = zc.Value;

        return (c, maxIter) =>
        {
            var z = c;
            int iter = 0;

            while (z.SquareNorm() <= 4.0 && iter < maxIter)
            {
                z = r * z + z * z;
                iter++;
            }

            return iter;
        };
    }
}
