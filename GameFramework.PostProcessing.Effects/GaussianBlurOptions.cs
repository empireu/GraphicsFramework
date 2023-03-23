using System.Numerics;

namespace GameFramework.PostProcessing.Effects;

public struct GaussianBlurOptions
{
    public const uint Size = 32;

    public static GaussianBlurOptions Default(Vector2 textureSize) => new()
    {
        TextureSize = textureSize,
        BlurDirections = 16,
        BlurQuality = 5,
        BlurSize = 12
    };

    public Vector2 TextureSize;
    public float BlurDirections;
    public float BlurQuality;
    public float BlurSize;

}