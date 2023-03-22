using System.Diagnostics;
using GameFramework.Layers;
using GameFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Size = System.Drawing.Size;

namespace GameFramework;

public abstract class GameApplication
{
    private readonly Sdl2Window _window;
    private readonly GraphicsDevice _device;

    public GraphicsDevice Device => _device;

    public Sdl2Window Window => _window;

    private readonly CommandList _commandList;

    public GameInput Input { get; }

    public FrameStatus FrameStatus { get; private set; }

    public GameApplicationResources Resources { get; }

    private LayerCollection? _layers;

    public LayerCollection Layers
    {
        get
        {
            if (_layers == null)
            {
                throw new InvalidOperationException("Cannot access layer collection before initialization!");
            }

            return _layers;
        }
    }

    public RgbaFloat ClearColor { get; set; } = RgbaFloat.Grey;

    private IServiceProvider? _serviceProvider;

    public IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Cannot access service provider before initialization!");
            }

            return _serviceProvider;
        }
    }

    public GameApplication(GameApplicationDescription description)
    {
        VeldridStartup.CreateWindowAndGraphicsDevice(
            description.WindowCreateInfo, 
            description.GraphicsDeviceOptions, 
            description.Backend, 
            out _window, 
            out _device);

        Resources = new GameApplicationResources(this);
        Input = new GameInput(this);

        _commandList = _device.ResourceFactory.CreateCommandList();

        SetupWindowEvents();
    }

    private void RegisterInternalServices(ServiceCollection collection)
    {
        collection.AddSingleton(this);
        collection.AddSingleton(Resources);
        collection.AddSingleton(Resources.Window);
        collection.AddSingleton(Resources.Device);
        collection.AddSingleton(Resources.Device.ResourceFactory);
        collection.AddSingleton(Resources.AssetManager);
        collection.AddSingleton(Input);
    }

    private void SetupWindowEvents()
    {
        _window.Resized += WindowOnResized;
    }

    private void WindowOnResized()
    {
        Resize(new Size(_window.Width, _window.Height));
    }

    public GameApplication() : this(GameApplicationDescription.Default)
    {

    }

    /// <summary>
    ///     Called once before the service provider is built and before any updates occur.
    /// </summary>
    protected virtual void RegisterServices(ServiceCollection services) { }

    /// <summary>
    ///     Called after the layer collection was created, before any updates occur.
    /// </summary>
    protected virtual void Initialize() { }

    protected virtual void BeforeStart()
    {
        foreach (var layer in Layers.FrontToBack)
        {
            layer.BeforeStart();
        }
    }

    /// <summary>
    ///     Called at the start of the update loop, before ProcessInput, Update and Render.
    /// </summary>
    protected virtual void BeforeUpdate(FrameInfo frameInfo)
    {
        foreach (var layer in Layers.FrontToBack)
        {
            layer.BeforeUpdate(frameInfo);
        }
    }

    protected virtual void Update(FrameInfo frameInfo)
    {
        foreach (var layer in Layers.FrontToBack)
        {
            layer.Update(frameInfo);
        }
    }

    /// <summary>
    ///     Called before Render.
    /// </summary>
    protected virtual void ProcessInput(FrameInfo frameInfo)
    {
        foreach (var keyEvent in Input.InputSnapshot.KeyEvents)
        {
            Layers.SendEvent(in keyEvent);
        }

        foreach (var mouseEvent in Input.InputSnapshot.MouseEvents)
        {
            Layers.SendEvent(in mouseEvent);
        }

        if (Input.InputSnapshot.WheelDelta != 0)
        {
            var @event = new ScrollEvent(Input.InputSnapshot.WheelDelta);
            Layers.SendEvent(in @event);
        }
    }

    protected virtual void Render(FrameInfo frameInfo)
    {
        foreach (var layer in Layers.BackToFront)
        {
            layer.Render(frameInfo);
        }
    }

    /// <summary>
    ///     Called at the end of the event loop, after Render.
    /// </summary>
    protected virtual void AfterRender(FrameInfo frameInfo)
    {
        foreach (var layer in Layers.FrontToBack)
        {
            layer.AfterRender(frameInfo);
        }
    }

    protected virtual void Resize(Size size)
    {
        _device.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);

        foreach (var layer in Layers.FrontToBack)
        {
            layer.Resize(size);
        }
    }

    /// <summary>
    ///     Called when the windows closes and the resources of the game need to be destroyed.
    /// </summary>
    protected virtual void Destroy()
    {
        foreach (var layer in Layers.FrontToBack)
        {
            layer.Destroy();
        }
    }

    private void SetupFrame()
    {
        _commandList.Begin();
        _commandList.SetFramebuffer(_device.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, ClearColor);
        _commandList.ClearDepthStencil(0);
        _commandList.End();

        _device.SubmitCommands(_commandList);
        _device.WaitForIdle();

        Input.InputSnapshot = _window.PumpEvents();
    }

    public void Run()
    {
        var services = new ServiceCollection();

        RegisterInternalServices(services);
        RegisterServices(services);

        _serviceProvider = services.BuildServiceProvider();

        _layers = new LayerCollection(_serviceProvider);

        Initialize();

        var started = false;

        var ts = Stopwatch.GetTimestamp();

        while (_window.Exists)
        {
            SetupFrame();

            if (!started)
            {
                BeforeStart();
                started = true;
            }

            var elapsed = (float)TimeSpan.FromTicks(Stopwatch.GetTimestamp() - ts).TotalSeconds;
            ts = Stopwatch.GetTimestamp();

            var frameInfo = new FrameInfo(elapsed);

            var beforeUpdateTime = Measurements.MeasureTimeSpan(() => BeforeUpdate(frameInfo)).TotalSeconds;

            var inputTime = Measurements.MeasureTimeSpan(() => ProcessInput(frameInfo)).TotalSeconds;
            
            var updateTime = Measurements.MeasureTimeSpan(() => Update(frameInfo)).TotalSeconds;

            var renderTime = Measurements.MeasureTimeSpan(() => Render(frameInfo)).TotalSeconds;
            
            var afterRenderTime = Measurements.MeasureTimeSpan(() => AfterRender(frameInfo)).TotalSeconds;
            
            var swapBuffersTime= Measurements.MeasureTimeSpan(_device.SwapBuffers).TotalSeconds;

            FrameStatus = new FrameStatus(
                (float)beforeUpdateTime, 
                (float)updateTime, 
                (float)renderTime,
                (float)afterRenderTime, 
                (float)swapBuffersTime,
                (float)inputTime);
        }

        Destroy();
    }
}