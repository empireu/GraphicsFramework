using GameFramework.Assets;
using Veldrid;

namespace GameFramework.PostProcessing.Effects;

public sealed class ThresholdStage : PostProcessingStage
{
    private readonly GameApplication _app;
    private readonly ColorStage _colorStage;
    private readonly DeviceBuffer _settingsBuffer;
    private ThresholdOptions _options;

    public unsafe ThresholdStage(GameApplication app)
    {
        _app = app;
        var layout = app.Resources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Settings", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

        _settingsBuffer = app.Resources.Factory.CreateBuffer(new BufferDescription(ThresholdOptions.Size, BufferUsage.UniformBuffer));

        var set = app.Resources.Factory.CreateResourceSet(new ResourceSetDescription(layout, _settingsBuffer));

        _colorStage = new ColorStage(
            app,
            new EmbeddedResourceKey(typeof(GaussianBlurStage).Assembly, "GameFramework.PostProcessing.Effects.Shaders.Threshold.spirv"),
            new[] { layout },
            new[] { set });

        Options = new ThresholdOptions { Cutoff = 0 };
    }

    public ThresholdOptions Options
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