using System.Drawing;
using GameFramework.Layers;
using GameFramework.Utilities;
using ImGuiNET;
using Veldrid;
using ImGuiNet = ImGuiNET.ImGui;

namespace GameFramework.ImGui;

public sealed class ImGuiLayer : Layer
{
    private readonly GameApplication _application;
    private readonly ImGuiRenderer _renderer;
    private readonly CommandList _commandList;
    private readonly ImGuiIOPtr _io;

    public bool EnableStats { get; set; }

    public ImGuiLayer(GameApplication application)
    {
       
        _application = application;
        _renderer = new ImGuiRenderer(application.Device, application.Device.SwapchainFramebuffer.OutputDescription,
            application.Window.Width, application.Window.Height);

        _io = ImGuiNet.GetIO();

        _commandList = application.Resources.Factory.CreateCommandList();
    }

    protected override void OnAdded()
    {
        RegisterHandler<MouseEvent>(OnMouseEvent);
        RegisterHandler<KeyEvent>(OnKeyEvent);
        RegisterHandler<ScrollEvent>(OnScrollEvent);
    }

    public bool Captured => _io.WantCaptureMouse || ImGuiNet.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);

    private bool OnMouseEvent(MouseEvent arg)
    {
        return Captured;
    }

    private bool OnKeyEvent(KeyEvent arg)
    {
        return Captured;
    }

    private bool OnScrollEvent(ScrollEvent arg)
    {
        return Captured;
    }

    protected override void BeforeUpdate(FrameInfo frameInfo)
    {
        _renderer.Update(frameInfo.DeltaTime, _application.Input.InputSnapshot);
        
        Submit?.Invoke(_renderer);

        if (EnableStats)
        {
            SubmitStats();
        }
    }

    private void SubmitStats()
    {
        if (ImGuiNet.Begin("Stats"))
        {
            ImGuiNet.TreePush("Pools");
            {
                ImGuiNet.Text($"Buffers in use: {_application.Resources.BufferPool.InUse}");
                ImGuiNet.Text($"Explicit memory: {UnitConverter.ConvertBytes(_application.Resources.BufferPool.InUseSize)}");
            }

            ImGuiNet.TreePop();

            ImGuiNet.TreePush("Caches");
            {
                ImGuiNet.Text($"Cached layouts: {_application.Resources.ResourceCache.CachedResourceLayouts}");
            }

            ImGuiNet.TreePop();

            ImGuiNet.Text($"User time: {_application.FrameStatus.UserTime * 1000:F}ms");
            ImGuiNet.Text($"Backend: {_application.Device.BackendType}");
        }

        ImGuiNet.End();
    }

    protected override void Resize(Size size)
    {
        _renderer.WindowResized(size.Width, size.Height);
    }

    protected override void Render(FrameInfo frameInfo)
    {
        _commandList.Begin();
        _commandList.SetFramebuffer(_application.Device.SwapchainFramebuffer);

        _renderer.Render(_application.Device, _commandList);

        _commandList.End();
        _application.Device.SubmitCommands(_commandList);
    }

    public event Action<ImGuiRenderer>? Submit;
}