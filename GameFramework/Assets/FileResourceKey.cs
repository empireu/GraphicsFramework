namespace GameFramework.Assets;

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