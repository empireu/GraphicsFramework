using System.Reflection;

namespace GameFramework.Assets.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public abstract class AssetLoadingOptionAttribute : Attribute
{
    public abstract void Validate(Type loadType);
}