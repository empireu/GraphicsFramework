using System.Drawing;
using System.Numerics;
using Veldrid.Sdl2;

namespace GameFramework.Extensions;

public static class WindowExtensions
{
    public static Size Size(this Sdl2Window window)
    {
        return new Size(window.Width, window.Height);
    }

    public static Vector2 SizeVector2(this Sdl2Window window)
    {
        return new Vector2(window.Width, window.Height);
    }
}