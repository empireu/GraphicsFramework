namespace GameFramework;

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

    public float BeforeUpdateTime { get; }
    public float UpdateTime { get; }
    public float RenderTime { get; }
    public float AfterRenderTime { get; }
    public float SwapBuffersTime { get; }
    public float InputTime { get; }
    public float UserTime => BeforeUpdateTime + UpdateTime + RenderTime + AfterRenderTime;
}