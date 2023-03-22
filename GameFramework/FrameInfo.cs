namespace GameFramework;

/// <summary>
///     Encapsulates per-frame information passed down from the game loop.
/// </summary>
public readonly struct FrameInfo
{
    /// <summary>
    ///     Gets the time passed since the last update.
    /// </summary>
    public float DeltaTime { get; }

    public FrameInfo(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}