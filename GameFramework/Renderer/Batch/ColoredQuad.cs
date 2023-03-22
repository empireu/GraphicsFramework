using System.Numerics;
using GameFramework.Renderer.VertexFormats;
using Veldrid;

namespace GameFramework.Renderer.Batch;

public struct ColoredQuad
{
    public VertexPosition3Color4 V0;
    public VertexPosition3Color4 V1;
    public VertexPosition3Color4 V2;
    public VertexPosition3Color4 V3;

    public static ColoredQuad Create(Matrix4x4 transform, RgbaFloat c0, RgbaFloat c1, RgbaFloat c2, RgbaFloat c3)
    {
        var p0 = Vector4.Transform(new Vector4(+0.5f, +0.5f, 0, 1), transform);
        var p1 = Vector4.Transform(new Vector4(+0.5f, -0.5f, 0, 1), transform);
        var p2 = Vector4.Transform(new Vector4(-0.5f, -0.5f, 0, 1), transform);
        var p3 = Vector4.Transform(new Vector4(-0.5f, +0.5f, 0, 1), transform);

        return new ColoredQuad
        {
            V0 = new VertexPosition3Color4 { Position = new Vector3(p0.X, p0.Y, p0.Z), Color = c0 },
            V1 = new VertexPosition3Color4 { Position = new Vector3(p1.X, p1.Y, p1.Z), Color = c1 },
            V2 = new VertexPosition3Color4 { Position = new Vector3(p2.X, p2.Y, p2.Z), Color = c2 },
            V3 = new VertexPosition3Color4 { Position = new Vector3(p3.X, p3.Y, p3.Z), Color = c3 }
        };
    }
}