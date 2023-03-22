using System.Numerics;
using Veldrid;

namespace GameFramework.Renderer.Batch;

public struct QuadBatchEffects
{
    public QuadBatchEffects(Matrix4x4 transform, RgbaFloat tint)
    {
        Transform = transform;
        Tint = tint;
    }

    public static QuadBatchEffects Tinted(RgbaFloat tint)
    {
        return new QuadBatchEffects(Matrix4x4.Identity, tint);
    }

    public static QuadBatchEffects Transformed(Matrix4x4 transform)
    {
        return new QuadBatchEffects(transform, new RgbaFloat(1, 1, 1, 1));
    }

    public static QuadBatchEffects None => new(Matrix4x4.Identity, new RgbaFloat(1, 1, 1, 1));
    
    public Matrix4x4 Transform { get; set; }

    public RgbaFloat Tint { get; set; }
}