using System.Numerics;
using MessagePack;

namespace GameFramework.Renderer.Batch;

[Serializable]
public struct SdfOptions4
{
    [Key(0)]
    public SdfOptions S0;

    [Key(1)]
    public SdfOptions S1;

    [Key(2)]
    public SdfOptions S2;

    [Key(3)]
    public SdfOptions S3;

    public SdfOptions4(SdfOptions s0, SdfOptions s1, SdfOptions s2, SdfOptions s3)
    {
        S0 = s0;
        S1 = s1;
        S2 = s2;
        S3 = s3;
    }

    public SdfOptions4(SdfOptions options)
    {
        S0 = S1 = S2 = S3 = options;
    }

    public SdfOptions4(float weight, float smoothing, float alpha, Vector3 color) : this(new SdfOptions { Weight = weight, Smoothing = smoothing, Alpha = alpha, Color = color})
    {

    }

    public SdfOptions4 WithColors(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3)
    {
        return new SdfOptions4(
            S0.WithColor(c0),
            S1.WithColor(c1),
            S2.WithColor(c2),
            S3.WithColor(c3));
    }

    public SdfOptions4 WithColor(Vector3 color)
    {
        return WithColors(color, color, color, color);
    }

    [IgnoreMember]
    public float AverageWeight => (S0.Weight + S1.Weight + S2.Weight + S3.Weight) / 4;

    [IgnoreMember]
    public float AverageSmoothing => (S0.Smoothing + S1.Smoothing + S2.Smoothing + S3.Smoothing) / 4;

    public void SetWeight(float weight)
    {
        S0.Weight = S1.Weight = S2.Weight = S3.Weight = weight;
    }

    public void SetSmoothing(float smoothing)
    {
        S0.Smoothing = S1.Smoothing = S2.Smoothing = S3.Smoothing = smoothing;
    }
}