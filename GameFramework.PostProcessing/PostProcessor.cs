using System.Diagnostics;
using System.Drawing;
using GameFramework.Assets;
using GameFramework.Extensions;
using Veldrid;

namespace GameFramework.PostProcessing;

public sealed class PostProcessor : IDisposable
{
    private readonly List<PostProcessingStage> _stages = new();

    private readonly CommandList _commandList;

    private readonly GameApplication _application;
    private PostProcessingFramebuffer? _outputFramebuffer;
    private readonly PixelFormat _pixelFormat;

    public PostProcessingFramebuffer? Input { get; private set; }

    public Framebuffer InputFramebuffer
    {
        get
        {
            if (Input == null)
            {
                throw new InvalidOperationException("Input framebuffer not initialized.");
            }

            return Input.Framebuffer;
        }
    }

    private PostProcessingFramebuffer? _intermediaryFramebuffer;

    private readonly ColorStage _copyStage;

    public RgbaFloat BackgroundColor { get; set; } = RgbaFloat.Clear;

    public void AddStage(PostProcessingStage stage, bool updateOutput = true)
    {
        Debug.Assert(Input != null);

        _stages.Add(stage);

        if (updateOutput)
        {
            stage.UpdateOutput(Input.Framebuffer.OutputDescription);
        }
    }

    public void SetOutput(Framebuffer framebuffer)
    {
        _outputFramebuffer = new PostProcessingFramebuffer(_application.Device, false, framebuffer);
    
        _copyStage.UpdateOutput(_outputFramebuffer.Framebuffer.OutputDescription);
    }

    public PostProcessor(GameApplication application, PixelFormat pixelFormat = PixelFormat.B8_G8_R8_A8_UNorm)
    {
        _commandList = application.Resources.Factory.CreateCommandList();

        _application = application;
        _pixelFormat = pixelFormat;

        _copyStage = new ColorStage(application,
            new EmbeddedResourceKey(typeof(PostProcessor).Assembly,
                "GameFramework.PostProcessing.Shaders.PassTrough.spirv"), Array.Empty<ResourceLayout>(), Array.Empty<ResourceSet>());
        
        ResizeInputs(application.Window.Size());
    }

    public void ResizeInputs(Size size)
    {
        Input?.Dispose();
        _intermediaryFramebuffer?.Dispose();

        var width = (uint)size.Width;
        var height = (uint)size.Height;

        Input = PostProcessingFramebuffer.Create(_application.Device, width, height, _pixelFormat);
        
        _intermediaryFramebuffer = PostProcessingFramebuffer.Create(_application.Device, width, height, _pixelFormat);

        SetIntermediaryFormat(Input.Framebuffer.OutputDescription);
    }

    public void SetIntermediaryFormat(OutputDescription description)
    {
        foreach (var stage in _stages)
        {
            stage.UpdateOutput(description);
        }
    }

    public void ClearColor()
    {
        Debug.Assert(Input != null);
        Debug.Assert(_intermediaryFramebuffer != null);

        Clear(Input, BackgroundColor);
        Clear(_intermediaryFramebuffer, BackgroundColor);
    }

    private void Clear(PostProcessingFramebuffer framebuffer, RgbaFloat color)
    {
        _commandList.Begin();
        _commandList.SetFramebuffer(framebuffer.Framebuffer);
        _commandList.ClearColorTarget(0, color);
        _commandList.End();

        _application.Device.SubmitCommands(_commandList);
    }

    public void Render(bool clear = true)
    {
        if (_outputFramebuffer == null)
        {
            throw new InvalidOperationException("Cannot render without an output framebuffer.");
        }

        Debug.Assert(Input != null);
        Debug.Assert(_intermediaryFramebuffer != null);

        var stageInput = Input;
        var stageOutput = _intermediaryFramebuffer;

        for (var index = 0; index < _stages.Count; index++)
        {
            var stage = _stages[index];
            
            if (!stage.Enabled)
            {
                continue;
            }

            stage.Process(stageInput, stageOutput);

            Clear(stageInput, BackgroundColor);

            if (index < _stages.Count - 1)
            {
                (stageInput, stageOutput) = (stageOutput, stageInput);
            }
        }

        if (_stages.Count == 0)
        {
            stageOutput = stageInput;
        }

        _copyStage.Process(stageOutput, _outputFramebuffer);
    }

    public void Dispose()
    {
        _copyStage.Dispose();

        foreach (var postProcessingStage in _stages)
        {
            postProcessingStage.Dispose();
        }

        _commandList.Dispose();
        _outputFramebuffer?.Dispose();
        _intermediaryFramebuffer?.Dispose();
        Input?.Dispose();
    }
}