using System.Diagnostics;

namespace GameFramework.Utilities;

public static class Measurements
{
    public static long MeasureTicks(Action body)
    {
        var ts = Stopwatch.GetTimestamp();
        body();
        return Stopwatch.GetTimestamp() - ts;
    }

    public static TimeSpan MeasureTimeSpan(Action body)
    {
        return TimeSpan.FromTicks(MeasureTicks(body));
    }

    public static double MeasureSecondsD(Action body)
    {
        return MeasureTimeSpan(body).TotalSeconds;
    }

    public static float MeasureSeconds(Action body)
    {
        return (float)MeasureSecondsD(body);
    }
}