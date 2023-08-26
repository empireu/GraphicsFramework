using System.Numerics;
using System.Runtime.CompilerServices;
using GameFramework.Renderer;
using GameFramework.Renderer.Batch;
using GameFramework.Renderer.VertexFormats;
using GameFramework.Utilities.Extensions;
using Veldrid;

namespace GameFramework.Extensions;

public readonly struct QuadColors
{
    public RgbaFloat C0 { get; init; }
    public RgbaFloat C1 { get; init; }
    public RgbaFloat C2 { get; init; }
    public RgbaFloat C3 { get; init; }

    public QuadColors(RgbaFloat f)
    {
        C0 = f;
        C1 = f;
        C2 = f;
        C3 = f;
    }
   
    public QuadColors(Vector4 f)
    {
        var c = new RgbaFloat(f);
        C0 = c;
        C1 = c;
        C2 = c;
        C3 = c;
    }

    public QuadColors(float r, float g, float b, float a)
    {
        var c = new RgbaFloat(r, g, b, a);
        C0 = c;
        C1 = c;
        C2 = c;
        C3 = c;
    }

    public static QuadColors FromZOrder(
        RgbaFloat topLeft,
        RgbaFloat topRight,
        RgbaFloat bottomLeft,
        RgbaFloat bottomRight)
    {
        return new QuadColors
        {
            C0 = topRight,
            C1 = bottomRight,
            C2 = bottomLeft,
            C3 = topLeft
        };
    }

    public static implicit operator RgbaFloat4(QuadColors colors) => new(colors.C0, colors.C1, colors.C2, colors.C3);
}

public static class QuadBatchExtensions
{
    public static void ApplyAlignMode(ref Vector2 position, Vector2 size, AlignMode mode)
    {
        switch (mode)
        {
            case AlignMode.TopLeft:
                position.X += size.X * 0.5f;
                position.Y -= size.Y * 0.5f;
                break;
        }
    }

    #region Colored Quads

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Quad(this QuadBatch batch, Matrix4x4 transform, RgbaFloat4 color)
    {
        batch.AddColoredQuad(transform, color.C0, color.C1, color.C2, color.C3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Quad(this QuadBatch batch, Matrix4x4 transform, in RgbaFloat4 color)
    {
        batch.AddColoredQuad(transform, color.C0, color.C1, color.C2, color.C3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Quad(this QuadBatch batch, Vector2 position, RgbaFloat4 color, float zOffset = 0, AlignMode align = AlignMode.Center)
    {
        ApplyAlignMode(ref position, Vector2.One, align);

        batch.Quad(Matrix4x4.CreateTranslation(position.X, position.Y, zOffset), color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Quad(this QuadBatch batch, Vector2 position, Vector2 scale, RgbaFloat4 color, float zOffset = 0, AlignMode align = AlignMode.Center)
    {
        ApplyAlignMode(ref position, scale, align);

        batch.Quad(Matrix4x4.CreateScale(scale.X, scale.Y, 0) * Matrix4x4.CreateTranslation(position.X, position.Y, zOffset), color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Quad(this QuadBatch batch, Vector2 position, Vector2 scale, float rotation, RgbaFloat4 color, float zOffset = 0, AlignMode align = AlignMode.Center)
    {
        ApplyAlignMode(ref position, scale, align);

        batch.Quad(Matrix4x4.CreateScale(scale.X, scale.Y, 0) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position.X, position.Y, zOffset), color);
    }

    #endregion

    #region Textured Quads

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TexturedQuad(this QuadBatch batch, Vector2 position, TextureSampler texture, AlignMode align = AlignMode.Center)
    {
        ApplyAlignMode(ref position, Vector2.One, align);

        batch.AddTexturedQuad(texture, Matrix4x4.CreateTranslation(position.X, position.Y, 0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TexturedQuad(this QuadBatch batch, Vector2 position, Vector2 scale, TextureSampler texture, AlignMode align = AlignMode.Center)
    {
        ApplyAlignMode(ref position, scale, align);

        batch.AddTexturedQuad(texture, Matrix4x4.CreateScale(scale.X, scale.Y, 0) * Matrix4x4.CreateTranslation(position.X, position.Y, 0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TexturedQuad(this QuadBatch batch, Vector2 position, Vector2 scale, float rotation, TextureSampler texture, AlignMode align = AlignMode.Center)
    {
        ApplyAlignMode(ref position, scale, align);

        batch.AddTexturedQuad(texture, Matrix4x4.CreateScale(scale.X, scale.Y, 0) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position.X, position.Y, 0));
    }

    #endregion

    #region Lines

    public static void Line(this QuadBatch batch, Vector2 start, Vector2 end, RgbaFloat4 color, float thickness = 1)
    {
        var extrude = Vector2.Normalize((end - start).Perpendicular());

        extrude *= thickness * 0.5f;

        var p1Left = start + extrude;
        var p1Right = start - extrude;

        var p2Left = end + extrude;
        var p2Right = end - extrude;

        var coloredQuad = new ColoredQuad
        {
            V0 = new VertexPosition3Color4 { Color = color.C0, Position = new Vector3(p1Right, 0) },
            V1 = new VertexPosition3Color4 { Color = color.C1, Position = new Vector3(p1Left, 0) },
            V2 = new VertexPosition3Color4 { Color = color.C2, Position = new Vector3(p2Left, 0) },
            V3 = new VertexPosition3Color4 { Color = color.C3, Position = new Vector3(p2Right, 0) }
        };
        
        batch.AddColoredQuad(ref coloredQuad);
    }

    #endregion
}