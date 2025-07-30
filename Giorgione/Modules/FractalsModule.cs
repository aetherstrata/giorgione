// Copyright (c) Davide Pierotti <d.pierotti@live.it>. Licensed under the GPLv3 Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

using Discord.Interactions;

using Giorgione.Fractals;

using Microsoft.Extensions.Logging;

namespace Giorgione.Modules;

[Group("fractal", "Infinite beauty")]
public class FractalsModule(ILogger<BotModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("mandelbrot", "Generate a mandelbrot fractal")]
    public async Task Mandelbrot(double centerX, double centerY, double zoom = 1, int width = 800, int height = 800)
    {
        using var generator = new FractalGenerator(width, height);
        using var imageStream = generator.GenerateFractal((0, 0), 1.5, Fractal.Mandelbrot());
        logger.LogDebug("Fractal generated");

        await RespondWithFileAsync(imageStream, "fractal.png");
    }

    [SlashCommand("marek", "Generate a Marek's dragon fractal")]
    public async Task Marek(double centerX = -0.5, double centerY = -0.1, double zoom = 1, int width = 800, int height = 800, int? rx = null, int? ry = null)
    {
        Complex? r = (rx.HasValue, ry.HasValue) switch
        {
            (false, false) => null,
            (true, false) => new Complex(rx.Value, 0),
            (false, true) => new Complex(0, ry.Value),
            _ => new Complex(rx.Value, ry.Value),
        };
        using var generator = new FractalGenerator(width, height);
        using var imageStream = generator.GenerateFractal((centerX, centerY), 1.5, Fractal.MarekDragon(r));
        logger.LogDebug("Fractal generated");

        await RespondWithFileAsync(imageStream, "fractal.png");
    }
}
