using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GameFramework.Utilities;

/// <summary>
///     Assertions for explosive state validation.
/// Basically, assertions will blow up the entire program if failed, regardless of error handling.
/// </summary>
public static class Assert
{
    /// <summary>
    ///     Terminates the application, with the specified error message.
    /// </summary>
    [DoesNotReturn]
    public static void Fail(string? message = null)
    {
        message ??= "Assertion failed.";

        Trace.Fail(message);

        while (true) { }
    }

    /// <summary>
    ///     Asserts that the value is true.
    /// </summary>
    /// <param name="value">The value to assert. If false, <see cref="Fail"/> will be called.</param>
    /// <param name="message">An optional message to pass on.</param>
    public static void IsTrue([DoesNotReturnIf(false)] bool value, string? message = null)
    {
        if (value)
        {
            return;
        }

        message ??= "Assertion failed. Value was false.";

        Fail(message);
    }

    /// <summary>
    ///     Asserts that the value is not null.
    /// </summary>
    public static void NotNull<T>([NotNull] ref T? instance, string? message = null)
    {
        instance = NotNull(instance, message) ?? throw new Exception();
    }

    /// <summary>
    ///     Asserts that the value is not null.
    /// </summary>
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

    /// <summary>
    ///     Asserts that the value is not null.
    /// </summary>
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

    /// <summary>
    ///     Asserts that the value is <see cref="T"/>.
    /// </summary>
    public static T Is<T, TInput>(TInput? input, string? message = null)
    {
        if (input is not T result)
        {
            Fail(message ?? $"Assertion failed. {(input == null ? "Null" : input)} was not {typeof(T)}");
            while (true) { }
        }

        return result;
    }

    /// <summary>
    ///     Asserts that the value is <see cref="T"/>.
    /// </summary>
    public static T Is<T>(object? obj, string? message = null)
    {
        return Is<T, object?>(obj, message);
    }
}