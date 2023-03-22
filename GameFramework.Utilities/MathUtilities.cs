using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameFramework.Utilities;

public static class MathUtilities
{
    /// <summary>
    ///     Maps a value from a source range to a value in a destination range range.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <param name="srcMin">The minimum value in the source range.</param>
    /// <param name="srcMax">The maximum value in the source range.</param>
    /// <param name="dstMin">The minimum value in the destination range.</param>
    /// <param name="dstMax">The maximum value in the destination range.</param>
    /// <returns>A value mapped in the destination range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MapRange<T>(T value, T srcMin, T srcMax, T dstMin, T dstMax) where T : IFloatingPoint<T>
    {
        return (value - srcMin) * (dstMax - dstMin) / (srcMax - srcMin) + dstMin;
    }

    // Fast power of two algorithm
    public static int NextPow2(int v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;

        return v;
    }
}