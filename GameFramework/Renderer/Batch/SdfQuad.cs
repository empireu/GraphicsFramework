using GameFramework.Renderer.VertexFormats;
using System.Numerics;

namespace GameFramework.Renderer.Batch;

public struct SdfQuad
{
    public VertexSdf V0;
    public VertexSdf V1;
    public VertexSdf V2;
    public VertexSdf V3;

    public static SdfQuad Create(Matrix4x4 transform, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, SdfOptions s0, SdfOptions s1, SdfOptions s2, SdfOptions s3)
    {
        var p0 = Vector4.Transform(new Vector4(+0.5f, +0.5f, 0, 1), transform);
        var p1 = Vector4.Transform(new Vector4(+0.5f, -0.5f, 0, 1), transform);
        var p2 = Vector4.Transform(new Vector4(-0.5f, -0.5f, 0, 1), transform);
        var p3 = Vector4.Transform(new Vector4(-0.5f, +0.5f, 0, 1), transform);

        return new SdfQuad
        {
            V0 = new VertexSdf { Position = new Vector3(p0.X, p0.Y, p0.Z), TextureCoordinate = uv0, SdfOptions = s0 },
            V1 = new VertexSdf { Position = new Vector3(p1.X, p1.Y, p1.Z), TextureCoordinate = uv1, SdfOptions = s1 },
            V2 = new VertexSdf { Position = new Vector3(p2.X, p2.Y, p2.Z), TextureCoordinate = uv2, SdfOptions = s2 },
            V3 = new VertexSdf { Position = new Vector3(p3.X, p3.Y, p3.Z), TextureCoordinate = uv3, SdfOptions = s3 }
        };
    }

    public static SdfQuad Create(Matrix4x4 transform, TextureCoordinate4 textureCoordinates, SdfOptions4 options)
    {
        return Create(
            transform,
            textureCoordinates.BottomRight,
            textureCoordinates.TopRight,
            textureCoordinates.TopLeft,
            textureCoordinates.BottomLeft,
            options.S0,
            options.S1,
            options.S2,
            options.S3);
    }
}