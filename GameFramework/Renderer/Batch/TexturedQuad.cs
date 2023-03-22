using GameFramework.Renderer.VertexFormats;
using System.Numerics;

namespace GameFramework.Renderer.Batch;

public struct TexturedQuad
{
    public VertexPosition3TextureCoordinate2 V0;
    public VertexPosition3TextureCoordinate2 V1;
    public VertexPosition3TextureCoordinate2 V2;
    public VertexPosition3TextureCoordinate2 V3;

    public static TexturedQuad Create(Matrix4x4 transform, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        var p0 = Vector4.Transform(new Vector4(+0.5f, +0.5f, 0, 1), transform);
        var p1 = Vector4.Transform(new Vector4(+0.5f, -0.5f, 0, 1), transform);
        var p2 = Vector4.Transform(new Vector4(-0.5f, -0.5f, 0, 1), transform);
        var p3 = Vector4.Transform(new Vector4(-0.5f, +0.5f, 0, 1), transform);

        return new TexturedQuad
        {
            V0 = new VertexPosition3TextureCoordinate2 { Position = new Vector3(p0.X, p0.Y, p0.Z), TextureCoordinate = uv0 },
            V1 = new VertexPosition3TextureCoordinate2 { Position = new Vector3(p1.X, p1.Y, p1.Z), TextureCoordinate = uv1 },
            V2 = new VertexPosition3TextureCoordinate2 { Position = new Vector3(p2.X, p2.Y, p2.Z), TextureCoordinate = uv2 },
            V3 = new VertexPosition3TextureCoordinate2 { Position = new Vector3(p3.X, p3.Y, p3.Z), TextureCoordinate = uv3 }
        };
    }

    public static TexturedQuad Create(Matrix4x4 transform, TextureCoordinate4 textureCoordinates)
    {
        return Create(
            transform,
            textureCoordinates.BottomRight,
            textureCoordinates.TopRight,
            textureCoordinates.TopLeft,
            textureCoordinates.BottomLeft);
    }
}