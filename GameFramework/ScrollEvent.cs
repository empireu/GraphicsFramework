namespace GameFramework;

public readonly struct ScrollEvent
{
    public ScrollEvent(float delta)
    {
        Delta = delta;
    }

    public float Delta { get; }
}