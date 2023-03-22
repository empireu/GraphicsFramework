namespace GameFramework.Renderer.Batch;

/// <summary>
///     Used by the quad batches to cache the index buffer in the <see cref="StaticDeviceBufferCache"/>
/// </summary>
public readonly struct QuadIndexBufferCacheKey
{
    public int QuadCount { get; }

    public QuadIndexBufferCacheKey(int quadCount)
    {
        QuadCount = quadCount;
    }

    public override int GetHashCode()
    {
        return QuadCount.GetHashCode();
    }

    public bool Equals(QuadIndexBufferCacheKey other)
    {
        return other.QuadCount == QuadCount;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not QuadIndexBufferCacheKey other)
        {
            return false;
        }

        return Equals(other);
    }

    public static bool operator ==(QuadIndexBufferCacheKey a, QuadIndexBufferCacheKey b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(QuadIndexBufferCacheKey a, QuadIndexBufferCacheKey b)
    {
        return !a.Equals(b);
    }
}