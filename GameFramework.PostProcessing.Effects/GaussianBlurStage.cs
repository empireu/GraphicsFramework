using System.Numerics;
using GameFramework.Assets;
using GameFramework.Extensions;
using Veldrid;

namespace GameFramework.PostProcessing.Effects;

public sealed class GaussianBlurStage : PostProcessingStage
{
    private readonly GameApplication _app;
    private readonly ColorStage _colorStage;
    private readonly DeviceBuffer _settingsBuffer;
    private GaussianBlurOptions _options;

    public unsafe GaussianBlurStage(GameApplication app)
    {
        _app = app;
        var layout = app.Resources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Settings", ResourceKind.UniformBuffer, ShaderStages.Fragment)));
     
        _settingsBuffer =
            app.Resources.Factory.CreateBuffer(new BufferDescription(GaussianBlurOptions.Size,
                BufferUsage.UniformBuffer));
     
        var set = app.Resources.Factory.CreateResourceSet(new ResourceSetDescription(layout, _settingsBuffer));

        _colorStage = new ColorStage(
            app,
            new EmbeddedResourceKey(typeof(GaussianBlurStage).Assembly, "GameFramework.PostProcessing.Effects.Shaders.GaussianBlur.spirv"),
            new [] { layout },
            new [] { set });

        Options = GaussianBlurOptions.Default(app.Window.SizeVector2());
    }

    public GaussianBlurOptions Options
    {
        get => _options;
        set => _options = value;
    }

    private void UpdateSettingsBuffer()
    {
        _app.Device.UpdateBuffer(_settingsBuffer, 0, _options);
    }

    public override void UpdateOutput(OutputDescription outputDescription)
    {
        _colorStage.UpdateOutput(outputDescription);
    }

    public override void Process(PostProcessingFramebuffer inputFramebuffer, PostProcessingFramebuffer outputBuffer)
    {
        UpdateSettingsBuffer();
        
        _colorStage.Process(inputFramebuffer, outputBuffer);
    }

    public override void Dispose()
    {
        _colorStage.Dispose();
        _settingsBuffer.Dispose();
    }
}