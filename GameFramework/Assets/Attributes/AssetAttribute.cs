namespace GameFramework.Assets.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class AssetAttribute : Attribute
{
    public string Path { get; }
    public bool Embedded { get; }
    public string AssemblyPath { get; }

    public AssetAttribute(string path, bool embedded = true, string assemblyPath = "")
    {
        Path = path;
        Embedded = embedded;
        AssemblyPath = assemblyPath;
    }
}