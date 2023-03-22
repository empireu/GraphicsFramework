using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GameFramework.Utilities;

public static class Assert
{
    [DoesNotReturn]
    public static void Fail(string? message = null)
    {
        message ??= "Assertion failed.";

        Trace.Fail(message);

        while (true) { }
    }

    public static void IsTrue([DoesNotReturnIf(false)] bool value, string? message = null)
    {
        if (value)
        {
            return;
        }

        message ??= "Assertion failed. Value was false.";

        Fail(message);
    }

    
    public static void NotNull<T>([NotNull] ref T? instance, string? message = null)
    {
        instance = NotNull(instance, message) ?? throw new Exception();
    }

    public static T NotNull<T>(T? instance, string? message = null)
    {
        if (instance != null)
        {
            return instance;
        }

        message ??= "Assertion failed. Instance was null.";

        Fail(message);

        while (true) { }
    }

    public static T NotNull<T>(T? instance, string? message = null) where T : struct
    {
        if (instance != null)
        {
            return instance.Value;
        }

        message ??= "Assertion failed. Instance was null.";

        Fail(message);

        while (true) { }
    }

    public static T Is<T, TInput>(TInput? input, string? message = null)
    {
        if (input is not T result)
        {
            Fail(message ?? $"Assertion failed. {(input == null ? "Null" : input)} was not {typeof(T)}");
            while (true) { }
        }

        return result;
    }

    public static T Is<T>(object? obj, string? message = null)
    {
        return Is<T, object?>(obj, message);
    }
}