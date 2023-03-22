using System.Drawing;
using Veldrid;

namespace GameFramework.Extensions;

public static class FramebufferExtensions
{
    public static Size Size(this Framebuffer framebuffer)
    {
        return new Size((int)framebuffer.Width, (int)framebuffer.Height);
    }
}