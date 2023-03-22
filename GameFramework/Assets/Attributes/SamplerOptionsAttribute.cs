using GameFramework.Renderer;
using Veldrid;

namespace GameFramework.Assets.Attributes;

public sealed class SamplerOptionsAttribute : AssetLoadingOptionAttribute
{
    public SamplerType Type { get; }

    public SamplerOptionsAttribute(SamplerType type)
    {
        Type = type;
    }

    public override void Validate(Type loadType)
    {
        if (loadType != typeof(Sprite))
        {
            throw new Exception($"Invalid usage of {nameof(SamplerOptionsAttribute)} on {loadType}");
        }
    }
}