using System.Drawing;
using System.Numerics;

namespace GameFramework.Utilities.Extensions;

public static class VectorExtensions
{
    public static Point ToPoint(this Vector2 vector)
    {
        return new Point((int)Math.Round(vector.X, MidpointRounding.ToEven), (int)Math.Round(vector.Y, MidpointRounding.ToEven));
    }

    public static PointF ToPointF(this Vector2 vector)
    {
        return new PointF(vector.X, vector.Y);
    }

    public static Vector2 Perpendicular(this Vector2 original)
    {
        var x = original.X;
        var y = original.Y;

        return new Vector2(-y, x);
    }
}