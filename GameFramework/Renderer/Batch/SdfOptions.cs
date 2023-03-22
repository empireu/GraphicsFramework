using System.Numerics;
using System.Runtime.InteropServices;
using MessagePack;

namespace GameFramework.Renderer.Batch;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct SdfOptions
{
    public const uint Size = 32;

    [Key(0)]
    public float Weight;

    [Key(1)]
    public float Smoothing;

    [Key(2)]
    public float Alpha;

    [Key(3)]
    public Vector3 Color;

    public SdfOptions WithColor(Vector3 color)
    {
        return this with { Color = color };
    }
}