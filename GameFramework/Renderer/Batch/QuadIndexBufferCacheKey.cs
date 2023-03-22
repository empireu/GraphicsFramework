namespace GameFramework.Renderer.Batch;

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