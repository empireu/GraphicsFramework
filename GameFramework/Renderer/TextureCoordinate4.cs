using System.Numerics;
using MessagePack;

namespace GameFramework.Renderer;

[MessagePackObject]
public struct TextureCoordinate4
{
    public static readonly Vector2 Uv0Full = new(1, 1);
    public static readonly Vector2 Uv1Full = new(1, 0);
    public static readonly Vector2 Uv2Full = new(0, 0);
    public static readonly Vector2 Uv3Full = new(0, 1);


    [Key(0)]
    public Vector2 BottomRight;

    [Key(1)]
    public Vector2 TopRight;
    
    [Key(2)]
    public Vector2 TopLeft;
    
    [Key(3)]
    public Vector2 BottomLeft;

    [IgnoreMember]
    public Vector2 Extents => BottomRight - TopLeft;

    [IgnoreMember]
    public float Width => Extents.X;

    [IgnoreMember]
    public float Height => Extents.Y;

    public TextureCoordinate4(Vector2 bottomRight, Vector2 topRight, Vector2 topLeft, Vector2 bottomLeft)
    {
        BottomRight = bottomRight;
        TopRight = topRight;
        TopLeft = topLeft;
        BottomLeft = bottomLeft;
    }

    public static TextureCoordinate4 Full => new(Uv0Full, Uv1Full, Uv2Full, Uv3Full);
}