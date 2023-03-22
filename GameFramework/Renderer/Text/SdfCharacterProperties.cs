using System.Numerics;
using MessagePack;

namespace GameFramework.Renderer.Text;

[MessagePackObject]
public readonly struct SdfCharacterProperties
{
    [Key(0)]
    public Vector2 Padding { get; }

    [Key(1)]
    public TextureCoordinate4 Coordinates { get; }

    [Key(2)]
    public Vector2 SourceGlyphSize { get; }

    public SdfCharacterProperties(Vector2 padding, TextureCoordinate4 coordinates, Vector2 sourceGlyphSize)
    {
        Padding = padding;
        Coordinates = coordinates;
        SourceGlyphSize = sourceGlyphSize;
    }
}