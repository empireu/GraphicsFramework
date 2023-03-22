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