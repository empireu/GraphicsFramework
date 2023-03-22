using System.Drawing;
using System.Runtime.CompilerServices;

namespace GameFramework.Utilities;

public static class DrawingPrimitives
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    public static IEnumerable<Point> PixelLine(Point start, Point end)
    {
        var x0 = start.X;
        var y0 = start.Y;
        var x1 = end.X;
        var y1 = end.Y;

        var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

        if (steep)
        {
            var t = x0;
            x0 = y0;
            y0 = t;

            t = x1;
            x1 = y1;
            y1 = t;
        }
        if (x0 > x1)
        {
            var t = x0;
            x0 = x1;
            x1 = t;
            t = y0;
            y0 = y1;
            y1 = t;
        }

        var dx = x1 - x0;
        var dy = Math.Abs(y1 - y0);
        var error = dx / 2;
        var yStep = (y0 < y1) ? 1 : -1;
        var y = y0;

        for (var x = x0; x <= x1; x++)
        {
            yield return new Point(steep ? y : x, steep ? x : y);

            error -= dy;
            
            if (error >= 0)
            {
                continue;
            }

            y += yStep;
            error += dx;
        }
    }

    public static IEnumerable<Point> PixelCircle(Point center, int radius)
    {
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    yield return new Point(center.X + x, center.Y + y);
                }
            }
        }
    }
}