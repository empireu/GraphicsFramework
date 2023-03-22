using System.Runtime.CompilerServices;

namespace GameFramework.Sdf;

public readonly ref struct BitGrid
{
    public ReadOnlySpan<byte> Span { get; }

    public int Width { get; }

    public int Height { get; }

    public BitGrid(ReadOnlySpan<byte> span, int width, int height)
    {
        Span = span;
        Width = width;
        Height = height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte At(
        int x,
        int y,
        out bool outOfBounds)
    {
        if (x > -1 && x < Width && y > -1 && y < Height)
        {
            outOfBounds = false;
            return Span[x + y * Width];
        }

        outOfBounds = true;
        return 0;
    }
}