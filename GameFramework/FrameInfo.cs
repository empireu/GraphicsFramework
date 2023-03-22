namespace GameFramework;

public readonly struct FrameInfo
{
    public float DeltaTime { get; }

    public FrameInfo(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}