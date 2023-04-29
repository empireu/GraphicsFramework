using Veldrid;

namespace GameFramework.Renderer;

/// <summary>
///     Pair of a Texture and a Sampler.
/// </summary>
public readonly struct TextureSampler
{
    public TextureView Texture { get; }

    public Sampler Sampler { get; }

    public bool IsInvalid => Texture == null || Sampler == null || Texture.IsDisposed || Sampler.IsDisposed;

    public TextureSampler(TextureView texture, Sampler sampler)
    {
        Texture = texture;
        Sampler = sampler;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TextureSampler other)
        {
            return false;
        }

        return other.Texture.Equals(Texture) && other.Sampler.Equals(Sampler);
    }

    public static bool operator ==(TextureSampler a, TextureSampler b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(TextureSampler a, TextureSampler b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Texture, Sampler);
    }
}