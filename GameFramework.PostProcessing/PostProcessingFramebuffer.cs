using Veldrid;

namespace GameFramework.PostProcessing;

public sealed class PostProcessingFramebuffer : IDisposable
{
    public Framebuffer Framebuffer { get; }

    public Texture Color => Framebuffer.ColorTargets[0].Target;

    public TextureView? View { get; }

    public static PostProcessingFramebuffer Create(
        GraphicsDevice device,
        uint width,
        uint height,
        PixelFormat format)
    {
        var colorDescription = TextureDescription.Texture2D(
            width,
            height,
            1,
            1,
            format,
            TextureUsage.RenderTarget |TextureUsage.Sampled);

        var factory = device.ResourceFactory;
        var colorTarget = factory.CreateTexture(colorDescription);

        var framebuffer = factory.CreateFramebuffer(new FramebufferDescription(depthTarget: null, colorTargets: colorTarget));

        return new PostProcessingFramebuffer(device, true, framebuffer);
    }

    public PostProcessingFramebuffer(GraphicsDevice device, bool createView, Framebuffer framebuffer)
    {
        Framebuffer = framebuffer;

        if (createView)
        {
            View = device.ResourceFactory.CreateTextureView(Color);
        }
    }

    public void Dispose()
    {
        View?.Dispose();
        Color.Dispose();
        Framebuffer.Dispose();
    }
}