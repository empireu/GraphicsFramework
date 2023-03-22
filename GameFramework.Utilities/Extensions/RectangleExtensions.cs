using System.Drawing;

namespace GameFramework.Utilities.Extensions;

public static class RectangleExtensions
{
    public static float CenterX(ref this RectangleF rectangle)
    {
        return rectangle.X + (rectangle.Width / 2);
    }

    public static float CenterY(ref this RectangleF rectangle)
    {
        return rectangle.Y + (rectangle.Height / 2);
    }

}