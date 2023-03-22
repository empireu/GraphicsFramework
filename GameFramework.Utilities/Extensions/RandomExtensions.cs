using System;
using System.Numerics;

namespace GameFramework.Utilities.Extensions;

public static class RandomExtensions
{
    public static double NextDouble(this Random random, double min = -1, double max = 1)
    {
        return MathUtilities.MapRange(random.NextDouble(), 0d, 1d, min, max);
    }

    public static float NextFloat(this Random random, float min = -1, float max = 1)
    {
        return (float)random.NextDouble(min, max);
    }

    public static Vector2 NextVector2(this Random random, float min = -1, float max = 1)
    {
        return new Vector2(
            random.NextFloat(min, max), 
            random.NextFloat(min, max));
    }

    public static Vector4 NextVector4(this Random random, float min = -1, float max = 1)
    {
        return new Vector4(
            random.NextFloat(min,max),
            random.NextFloat(min,max),
            random.NextFloat(min,max),
            random.NextFloat(min,max));
    }
}