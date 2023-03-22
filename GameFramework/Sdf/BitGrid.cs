using System.Runtime.CompilerServices;

namespace GameFramework.Sdf;

/// <summary>
///     Represents an immutable regular grid of bits.
/// </summary>
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

    /// <summary>
    ///     Gets the bit at the specified position.
    /// </summary>
    /// <param name="x">The horizontal position in the grid.</param>
    /// <param name="y">The vertical position in the grid.</param>
    /// <param name="outOfBounds">Set to true if the specified position was found to be outside of bounds.</param>
    /// <returns>The value at the specified position or 0, if the cell is out of bounds.</returns>
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