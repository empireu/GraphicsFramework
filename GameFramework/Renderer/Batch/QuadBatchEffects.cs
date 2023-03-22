using System.Numerics;
using Veldrid;

namespace GameFramework.Renderer.Batch;

/// <summary>
///     Encapsulates data for some basic quad batch shader effects.
/// </summary>
public struct QuadBatchEffects
{
    public QuadBatchEffects(Matrix4x4 transform, RgbaFloat tint)
    {
        Transform = transform;
        Tint = tint;
    }

    /// <summary>
    ///     Adds tint to the output colors. By default, the colors are not tinted.
    /// </summary>
    /// <param name="tint">The tint to use.</param>
    /// <returns>A new instance of <see cref="QuadBatchEffects"/>, with the specified <code>tint</code> applied.</returns>
    public static QuadBatchEffects Tinted(RgbaFloat tint)
    {
        return new QuadBatchEffects(Matrix4x4.Identity, tint);
    }

    /// <summary>
    ///     Transforms the scene geometry. By default, an identity matrix is used.
    /// </summary>
    /// <param name="transform">The transformation to apply.</param>
    /// <returns>A new instance of <see cref="QuadBatchEffects"/>, with the specified <code>transform</code> applied.</returns>
    public static QuadBatchEffects Transformed(Matrix4x4 transform)
    {
        return new QuadBatchEffects(transform, new RgbaFloat(1, 1, 1, 1));
    }

    /// <summary>
    ///     Returns a new instance of <see cref="QuadBatchEffects"/>, with an identity transformation and identity tint.
    /// </summary>
    public static QuadBatchEffects None => new(Matrix4x4.Identity, new RgbaFloat(1, 1, 1, 1));
    
    public Matrix4x4 Transform { get; set; }

    public RgbaFloat Tint { get; set; }
}