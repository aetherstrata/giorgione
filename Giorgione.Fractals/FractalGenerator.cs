// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Giorgione.Fractals;

public sealed class FractalGenerator : IDisposable
{
    public int Width { get; }
    public int Height { get; }
    public double Aspect { get; }

    private readonly Image<Rgb24> _image;

    public FractalGenerator(int width, int height)
    {
        Width = width;
        Height = height;

        Aspect = (double) width / height;

        _image = new Image<Rgb24>(Width, Height);
    }

    public MemoryStream GenerateFractal((double X, double Y) center, double zoom, FractalFun  fractal)
    {
        const int maxIter = 100;

        double scale = 1.0 / (0.5 * zoom * Height);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var point = new Complex(
                    (x - Width / 2.0) * scale * Aspect + center.X,
                    (y - Height / 2.0) * scale * Aspect + center.Y);

                int iteration = fractal(point, maxIter);

                byte color = (byte)((double)iteration / maxIter * 255);

                _image[x, y] = new Rgb24(color, color, color);
            }
        }

        var stream = new MemoryStream();
        _image.SaveAsPng(stream);
        return stream;
    }

    public void Dispose()
    {
        _image.Dispose();
    }
}
