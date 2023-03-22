using Veldrid;

namespace GameFramework.Renderer;

/// <summary>
///     Encapsulates the data needed to render a sprite. Vertex data is not included here, only texture-related data is.
/// </summary>
public readonly struct Sprite
{
    /// <summary>
    ///     The texture and sampler to use.
    /// </summary>
    public TextureSampler Texture { get; }

    /// <summary>
    ///     The coordinates in this sprite's texture.
    /// </summary>
    public TextureCoordinate4 Coordinate4 { get; }

    public Sprite(TextureSampler ts, TextureCoordinate4 coordinate4)
    {
        Texture = ts;
        Coordinate4 = coordinate4;
    }

    public Sprite(TextureSampler ts) : this(ts, TextureCoordinate4.Full)
    {

    }
}