using Veldrid;

namespace GameFramework.Renderer;

public readonly struct Sprite
{
    public TextureSampler Texture { get; }

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