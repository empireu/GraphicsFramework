using System.Reflection;

namespace GameFramework.Assets;

public readonly struct ResourceKeyPair : IResourceKey
{
    public ResourceKeyPair(IResourceKey a, IResourceKey b)
    {
        A = a;
        B = b;
    }

    public IResourceKey A { get; }

    public IResourceKey B { get; }
}

/// <summary>
///     Represents a resource key that points towards a resource stored in an assembly.
/// </summary>
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

/// <summary>
///     Represents a resource key that points towards a file on the disk.
/// </summary>
public readonly struct FileResourceKey : IResourceKey
{
    public string Path { get; }

    public FileResourceKey(string path)
    {
        Path = path;
    }

    public bool Exists()
    {
        return File.Exists(Path);
    }

    public override int GetHashCode()
    {
        return Path.GetHashCode();
    }

    public bool Equals(FileResourceKey other)
    {
        return Path == other.Path;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FileResourceKey other)
        {
            return false;
        }

        return Equals(other);
    }

    public static bool operator ==(FileResourceKey a, FileResourceKey b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(FileResourceKey a, FileResourceKey b)
    {
        return !a.Equals(b);
    }
}

/// <summary>
///     Represents a generic resource locator.
/// </summary>
public interface IResourceKey
{
}