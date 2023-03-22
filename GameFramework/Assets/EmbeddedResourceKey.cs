using System.Reflection;

namespace GameFramework.Assets;

public readonly struct EmbeddedResourceKey : IResourceKey
{
    public Assembly Assembly { get; }

    public string Path { get; }

    public EmbeddedResourceKey(Assembly assembly, string path)
    {
        Assembly = assembly;
        Path = path;
    }

    public EmbeddedResourceKey(string path)
    {
        Assembly = Assembly.GetCallingAssembly();
        Path = path;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Assembly, Path);
    }

    public bool Equals(EmbeddedResourceKey other)
    {
        return Assembly == other.Assembly && Path == other.Path;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EmbeddedResourceKey other)
        {
            return false;
        }

        return Equals(other);
    }

    public static bool operator ==(EmbeddedResourceKey a, EmbeddedResourceKey b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(EmbeddedResourceKey a, EmbeddedResourceKey b)
    {
        return !a.Equals(b);
    }
}