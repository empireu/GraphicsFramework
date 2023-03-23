using System.Drawing;
using Veldrid;

namespace GameFramework.PostProcessing;

public abstract class PostProcessingStage : IDisposable
{
    public bool Enabled { get; set; } = true;

    public abstract void UpdateOutput(OutputDescription outputDescription);

    public abstract void Process(PostProcessingFramebuffer inputFramebuffer, PostProcessingFramebuffer outputBuffer);

    public abstract void Dispose();
}