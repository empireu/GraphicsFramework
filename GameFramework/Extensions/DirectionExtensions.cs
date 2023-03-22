using System.Numerics;
using System.Runtime.CompilerServices;
using GameFramework.Scene;

namespace GameFramework.Extensions;

public static class DirectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 Unit(this Direction2D direction)
    {
        return direction switch
        {
            Direction2D.Left => -Vector2.UnitX,
            Direction2D.Right => Vector2.UnitX,
            Direction2D.Up => Vector2.UnitY,
            Direction2D.Down => -Vector2.UnitY,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction.")
        };
    }
}