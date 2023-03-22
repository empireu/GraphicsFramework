using Veldrid;

namespace GameFramework;

/// <summary>
///     Encapsulates some rendering performance stats.
/// </summary>
public readonly struct FrameStatus
{
    public FrameStatus(float beforeUpdateTime, float updateTime, float renderTime, float afterRenderTime, float swapBuffersTime, float inputTime)
    {
        BeforeUpdateTime = beforeUpdateTime;
        UpdateTime = updateTime;
        RenderTime = renderTime;
        AfterRenderTime = afterRenderTime;
        SwapBuffersTime = swapBuffersTime;
        InputTime = inputTime;
    }

    /// <summary>
    ///     Gets the time spent in <see cref="GameApplication.BeforeUpdate"/>
    /// </summary>
    public float BeforeUpdateTime { get; }
    /// <summary>
    ///     Gets the time spent in <see cref="GameApplication.Update"/>
    /// </summary>
    public float UpdateTime { get; }
    /// <summary>
    ///     Gets the time spent in <see cref="GameApplication.Render"/>
    /// </summary>
    public float RenderTime { get; }
    /// <summary>
    ///     Gets the time spent in <see cref="GameApplication.AfterRender"/>
    /// </summary>
    public float AfterRenderTime { get; }
    /// <summary>
    ///     Gets the time spent swapping buffers with the native window (<see cref="GraphicsDevice.SwapBuffers()"/>)
    /// </summary>
    public float SwapBuffersTime { get; }
    /// <summary>
    ///     Gets the time spent in <see cref="GameApplication.ProcessInput"/>
    /// </summary>
    public float InputTime { get; }
    /// <summary>
    ///     Gets the total time spent in user functions.
    ///     <see cref="BeforeUpdateTime"/>
    ///     <see cref="UpdateTime"/>
    ///     <see cref="RenderTime"/>
    ///     <see cref="AfterRenderTime"/>
    /// </summary>
    public float UserTime => BeforeUpdateTime + UpdateTime + RenderTime + AfterRenderTime;
}