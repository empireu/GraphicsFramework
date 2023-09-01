using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace GameFramework.Renderer;

/// <summary>
///     Holds a set of 4 <see cref="RgbaFloat"/>.
/// </summary>
public struct RgbaFloat4
{
    public RgbaFloat C0;
    public RgbaFloat C1;
    public RgbaFloat C2;
    public RgbaFloat C3;

    public float R
    {
        set
        {
            C0 = new RgbaFloat(value, C0.G, C0.B, C0.A);
            C1 = new RgbaFloat(value, C1.G, C1.B, C1.A);
            C2 = new RgbaFloat(value, C2.G, C2.B, C2.A);
            C3 = new RgbaFloat(value, C3.G, C3.B, C3.A);
        }
    }

    public float G
    {
        set
        {
            C0 = new RgbaFloat(C0.R, value, C0.B, C0.A);
            C1 = new RgbaFloat(C1.R, value, C1.B, C1.A);
            C2 = new RgbaFloat(C2.R, value, C2.B, C2.A);
            C3 = new RgbaFloat(C3.R, value, C3.B, C3.A);
        }
    }

    public float B
    {
        set
        {
            C0 = new RgbaFloat(C0.R, C0.G, value, C0.A);
            C1 = new RgbaFloat(C1.R, C1.G, value, C1.A);
            C2 = new RgbaFloat(C2.R, C2.G, value, C2.A);
            C3 = new RgbaFloat(C3.R, C3.G, value, C3.A);
        }
    }

    public float A
    {
        set
        {
            C0 = new RgbaFloat(C0.R, C0.G, C0.B, value);
            C1 = new RgbaFloat(C1.R, C1.G, C1.B, value);
            C2 = new RgbaFloat(C2.R, C2.G, C2.B, value);
            C3 = new RgbaFloat(C3.R, C3.G, C3.B, value);
        }
    }

    public RgbaFloat4(float r, float g, float b, float a)
    {
        C0 = C1 = C2 = C3 = new RgbaFloat(r, g, b, a);
    }

    public RgbaFloat4(RgbaFloat rgba) : this(rgba.R, rgba.G, rgba.B, rgba.A)
    {
        
    }

    public RgbaFloat4(RgbaFloat c0, RgbaFloat c1, RgbaFloat c2, RgbaFloat c3)
    {
        C0 = c0;
        C1 = c1;
        C2 = c2;
        C3 = c3;
    }

    public RgbaFloat4(Vector4 c0, Vector4 c1, Vector4 c2, Vector4 c3)
    {
        C0 = new RgbaFloat(c0);
        C1 = new RgbaFloat(c1);
        C2 = new RgbaFloat(c2);
        C3 = new RgbaFloat(c3);
    }

    /// <summary>
    /// Red (1, 0, 0, 1)
    /// </summary>
    public static readonly RgbaFloat4 Red = new RgbaFloat4(1, 0, 0, 1);
    /// <summary>
    /// Dark Red (0.6f, 0, 0, 1)
    /// </summary>
    public static readonly RgbaFloat4 DarkRed = new RgbaFloat4(0.6f, 0, 0, 1);
    /// <summary>
    /// Green (0, 1, 0, 1)
    /// </summary>
    public static readonly RgbaFloat4 Green = new RgbaFloat4(0, 1, 0, 1);
    /// <summary>
    /// Blue (0, 0, 1, 1)
    /// </summary>
    public static readonly RgbaFloat4 Blue = new RgbaFloat4(0, 0, 1, 1);
    /// <summary>
    /// Yellow (1, 1, 0, 1)
    /// </summary>
    public static readonly RgbaFloat4 Yellow = new RgbaFloat4(1, 1, 0, 1);
    /// <summary>
    /// Grey (0.25f, 0.25f, 0.25f, 1)
    /// </summary>
    public static readonly RgbaFloat4 Grey = new RgbaFloat4(.25f, .25f, .25f, 1);
    /// <summary>
    /// Light Grey (0.65f, 0.65f, 0.65f, 1)
    /// </summary>
    public static readonly RgbaFloat4 LightGrey = new RgbaFloat4(.65f, .65f, .65f, 1);
    /// <summary>
    /// Cyan (0, 1, 1, 1)
    /// </summary>
    public static readonly RgbaFloat4 Cyan = new RgbaFloat4(0, 1, 1, 1);
    /// <summary>
    /// White (1, 1, 1, 1)
    /// </summary>
    public static readonly RgbaFloat4 White = new RgbaFloat4(1, 1, 1, 1);
    /// <summary>
    /// Cornflower Blue (0.3921f, 0.5843f, 0.9294f, 1)
    /// </summary>
    public static readonly RgbaFloat4 CornflowerBlue = new RgbaFloat4(0.3921f, 0.5843f, 0.9294f, 1);
    /// <summary>
    /// Clear (0, 0, 0, 0)
    /// </summary>
    public static readonly RgbaFloat4 Clear = new RgbaFloat4(0, 0, 0, 0);
    /// <summary>
    /// Black (0, 0, 0, 1)
    /// </summary>
    public static readonly RgbaFloat4 Black = new RgbaFloat4(0, 0, 0, 1);
    /// <summary>
    /// Pink (1, 0.45f, 0.75f, 1)
    /// </summary>
    public static readonly RgbaFloat4 Pink = new RgbaFloat4(1f, 0.45f, 0.75f, 1);
    /// <summary>
    /// Orange (1, 0.36f, 0, 1)
    /// </summary>
    public static readonly RgbaFloat4 Orange = new RgbaFloat4(1f, 0.36f, 0f, 1);

    public static implicit operator RgbaFloat4(RgbaFloat color)
    {
        return new RgbaFloat4(color);
    }

    /// <summary>
    /// Element-wise equality.
    /// </summary>
    /// <param name="other">The instance to compare to.</param>
    /// <returns>True if all elements are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(RgbaFloat4 other)
    {
        return C0.Equals(other.C0) && C1.Equals(other.C1) && C2.Equals(other.C2) && C2.Equals(other.C2);
    }

    public override bool Equals(object? obj)
    {
        return obj is RgbaFloat4 other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(C0, C1, C2, C3);
    }

    /// <summary>
    /// Returns a string representation of this color.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"A:{C0}, B:{C1}, C:{C2}, D:{C3}";
    }

    /// <summary>
    /// Element-wise equality.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RgbaFloat4 left, RgbaFloat4 right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Element-wise inequality.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RgbaFloat4 left, RgbaFloat4 right)
    {
        return !left.Equals(right);
    }

    public static implicit operator RgbaFloat4(Vector4 vector)
    {
        return new RgbaFloat4(vector.X, vector.Y, vector.Z, vector.W);
    }
}