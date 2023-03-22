using Veldrid;
using Veldrid.StartupUtilities;

namespace GameFramework;

public readonly struct GameApplicationDescription
{
    public WindowCreateInfo WindowCreateInfo { get; }

    public GraphicsDeviceOptions GraphicsDeviceOptions { get; }

    public GraphicsBackend Backend { get; }

    public GameApplicationDescription(WindowCreateInfo windowCreateInfo, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend backend)
    {
        WindowCreateInfo = windowCreateInfo;
        GraphicsDeviceOptions = graphicsDeviceOptions;
        Backend = backend;
    }

    public static GameApplicationDescription Default =>
        new(new WindowCreateInfo(
                100, 
                100, 
                1280, 
                720, 
                WindowState.Normal, 
                "Game"),

            new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt
            },

            GraphicsBackend.OpenGL);
}