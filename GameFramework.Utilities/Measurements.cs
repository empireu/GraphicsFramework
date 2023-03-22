using System.Diagnostics;

namespace GameFramework.Utilities;

/// <summary>
///     Utility class for time measurements.
/// </summary>
public static class Measurements
{
    /// <summary>
    ///     Measures the number of ticks spent in the action body.
    /// </summary>
    public static long MeasureTicks(Action body)
    {
        var ts = Stopwatch.GetTimestamp();
        body();
        return Stopwatch.GetTimestamp() - ts;
    }

    /// <summary>
    ///     Measures the time spent in the action body. Internally, this uses <see cref="MeasureTicks"/>
    /// </summary>
    public static TimeSpan MeasureTimeSpan(Action body)
    {
        return TimeSpan.FromTicks(MeasureTicks(body));
    }

    /// <summary>
    ///     Measures the time spent in the action body, in seconds. Internally, this uses <see cref="MeasureTimeSpan"/>
    /// </summary>
    public static double MeasureSecondsD(Action body)
    {
        return MeasureTimeSpan(body).TotalSeconds;
    }

    /// <summary>
    ///     Measures the time spent in the action body, in seconds. Internally, this uses <see cref="MeasureSecondsD"/>
    /// </summary>
    public static float MeasureSeconds(Action body)
    {
        return (float)MeasureSecondsD(body);
    }
}