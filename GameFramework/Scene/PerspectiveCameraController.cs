using System.Numerics;
using Veldrid;

namespace GameFramework.Scene;

public sealed class PerspectiveCameraController
{
    public enum TranslationControl
    {
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown
    }

    public PerspectiveCameraController(PerspectiveCamera camera, IReadOnlyDictionary<Key, TranslationControl> actions)
    {
        Camera = camera;
        Actions = new Dictionary<Key, TranslationControl>(actions);
    }

    public static PerspectiveCameraController CreateDefault()
    {
        var camera = new PerspectiveCamera();

        var actions = new Dictionary<Key, TranslationControl>()
        {
            { Key.W, TranslationControl.MoveForward },
            { Key.A, TranslationControl.MoveLeft },
            { Key.S, TranslationControl.MoveBackward },
            { Key.D, TranslationControl.MoveRight },
            { Key.Space, TranslationControl.MoveUp },
            { Key.C, TranslationControl.MoveDown }
        };

        return new PerspectiveCameraController(camera, actions);
    }

    public PerspectiveCamera Camera { get; }

    public float LookSensitivity { get; set; } = 10f;

    public float MoveSpeed { get; set; } = 1f;

    public void SetAspect(float aspect)
    {
        Camera.AspectRatio = aspect;
    }

    public void SetAspect(float width, float height)
    {
        Camera.AspectRatio = width / height;
    }

    public Dictionary<Key, TranslationControl> Actions { get; }

    public void ProcessMouse(Vector2 mouseDelta, float dt)
    {
        var factor = LookSensitivity * dt * (MathF.PI / 180f);
        Camera.Yaw += mouseDelta.X * factor;
        Camera.Pitch = Math.Clamp(Camera.Pitch - mouseDelta.Y * factor, -MathF.PI * 0.49f, MathF.PI * 0.49f);
    }

    public void ProcessKey(Key key, float dt)
    {
        if (!Actions.TryGetValue(key, out var action))
        {
            return;
        }
        
        var factor = MoveSpeed * dt;

        switch (action)
        {
            case TranslationControl.MoveForward:
                Camera.Position += Camera.Front * factor;
                break;
            case TranslationControl.MoveLeft:
                Camera.Position -= Camera.Right * factor;
                break;
            case TranslationControl.MoveBackward:
                Camera.Position -= Camera.Front * factor;
                break;
            case TranslationControl.MoveRight:
                Camera.Position += Camera.Right * factor;
                break;
            case TranslationControl.MoveUp:
                Camera.Position += Camera.Up * factor;
                break;
            case TranslationControl.MoveDown:
                Camera.Position -= Camera.Up * factor;
                break;
            default:
                break;
        }
    }
}