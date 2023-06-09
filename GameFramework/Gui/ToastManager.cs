﻿using System.Diagnostics;
using System.Numerics;
using GameFramework.Extensions;
using GameFramework.Renderer;
using GameFramework.Renderer.Batch;
using GameFramework.Renderer.Text;
using GameFramework.Utilities;

namespace GameFramework.Gui;

public enum ToastNotificationType
{
    Information,
    Error
}

public sealed class ToastNotification
{
    public ToastNotificationType Type { get; }
    public string Message { get; }
    public double Duration { get; }

    public double AddedTimestamp { get; set; }

    public ToastNotification(ToastNotificationType type, string message, double duration = 2)
    {
        Type = type;
        Message = message;
        Duration = duration;

        DisplayedString = $"{Type}!\n{Message}";
    }

    public string DisplayedString { get; }
}

public sealed class ToastManagerOptions
{
    public float DrawMargin = 0.01f;
    public float BorderThickness = 0.005f;
    public float YOffset = 0.01f;
    public Vector4 BorderColor = new(0.1f, 0.3f, 0.5f, 0.2f);
    public Vector4 InitialColor = new(1, 0.2f, 0.4f, 0.95f);
    public Vector4 FinalColor = new(0.1f, 1f, 0f, 0.1f);
    public const float DefaultEaseIn = 1;

    public ToastManager.AnimationDelegate? Animation = (notification, elapsed, position, size) =>
    {
        double EaseOutElastic(double t)
        {
            var t2 = (t - 1) * (t - 1);
            return 1 - t2 * t2 * Math.Cos(t * Math.PI * 4.5);
        }

        double EaseInQuad(double t)
        {
            return t * t;
        }

        const double easeOut = 0.5;

        if (elapsed < DefaultEaseIn)
        {
            return new Vector2((float)(size.X - EaseOutElastic(
                MathUtilities.MapRange(elapsed, 0, DefaultEaseIn, 0, 1)) * size.X), 0);
        }

        if (elapsed > notification.Duration - easeOut)
        {
            var t = MathUtilities.MapRange(
                elapsed, 
                notification.Duration - easeOut, 
                notification.Duration, 
                0, 
                1);

            return new Vector2((float)(size.X * EaseInQuad(t)), 0);
        }

        return Vector2.Zero;
    };
}

public sealed class ToastManager
{
    public delegate Vector2 AnimationDelegate(ToastNotification notification, double elapsed, Vector2 position, Vector2 size);

    private readonly SdfFont _font;
    private readonly List<ToastNotification> _notifications = new();
    private readonly Queue<ToastNotification> _removeQueue = new();
    public Stopwatch StopWatch { get; } = Stopwatch.StartNew();
    public ToastManagerOptions Options { get; }

    public void Remove(ToastNotification notification)
    {
        _notifications.Remove(notification);
    }

    public void ResetTimer(ToastNotification notification)
    {
        notification.AddedTimestamp = StopWatch.Elapsed.TotalSeconds;
    }

    public ToastManager(SdfFont font, ToastManagerOptions? options = null)
    {
        options ??= new ToastManagerOptions();

        _font = font;
        Options = options;
    }

    public void Add(ToastNotification notification)
    {
        _notifications.Add(notification);

        ResetTimer(notification);
    }

    public void Render(QuadBatch batch, float fontSize, Vector2 startPos, float edgeX)
    {
        var y = startPos.Y;

        foreach (var toastNotification in _notifications)
        {
            var elapsed = StopWatch.Elapsed.TotalSeconds - toastNotification.AddedTimestamp;

            if (elapsed > toastNotification.Duration)
            {
                _removeQueue.Enqueue(toastNotification);
                continue;
            }

            var progress = (float)(elapsed / toastNotification.Duration);

            var size = _font.MeasureText(toastNotification.DisplayedString, fontSize) * 1.1f;
            var position = new Vector2(edgeX - size.X - Options.DrawMargin, y);

            if (Options.Animation != null)
            {
                position += Options.Animation(toastNotification, elapsed, position, size);
            }

            batch.Quad(position, size, Vector4.Lerp(Options.InitialColor, Options.FinalColor, progress), align: AlignMode.TopLeft);

            _font.Render(batch, position + new Vector2(Options.DrawMargin, Options.DrawMargin), toastNotification.DisplayedString, size: fontSize);

            // upper border
            batch.Quad(
                new Vector2(position.X, position.Y),
                new Vector2(size.X + Options.BorderThickness, Options.BorderThickness),
                Options.BorderColor, align: AlignMode.TopLeft);

            // lower border
            batch.Quad(
                new Vector2(position.X, position.Y - size.Y),
                new Vector2(size.X + Options.BorderThickness, Options.BorderThickness),
                Options.BorderColor, align: AlignMode.TopLeft);

            // left border
            batch.Quad(
                new Vector2(position.X, position.Y),
                new Vector2(Options.BorderThickness, size.Y + Options.BorderThickness),
                Options.BorderColor, align: AlignMode.TopLeft);

            // right border
            batch.Quad(
                new Vector2(position.X + size.X, position.Y),
                new Vector2(Options.BorderThickness, size.Y + Options.BorderThickness),
                Options.BorderColor, align: AlignMode.TopLeft);


            y += size.Y + Options.YOffset;
        }

        while (_removeQueue.TryDequeue(out var removed))
        {
            _notifications.Remove(removed);
        }
    }
}