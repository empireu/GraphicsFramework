using System.Numerics;

namespace GameFramework.PostProcessing.Effects;

public struct ThresholdOptions
{
    public const uint Size = 32;

    public float UpperThreshold;
    public float Cutoff;
    
    private long _padding;

    public Vector4 MidColor;
}