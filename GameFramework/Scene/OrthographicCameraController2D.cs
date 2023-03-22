using System.Numerics;
using Veldrid;
using Vortice.Mathematics;

namespace GameFramework.Scene;

/// <summary>
///     Manages an <see cref="OrthographicCamera"/> and offers wrappers for 2D cameras and input handling.
/// </summary>
public sealed class OrthographicCameraController2D
{
    public float TranslationInterpolate { get; set; }
    
    public float RotationInterpolate { get; set; }
    
    public float ZoomInterpolate { get; set; }

    public OrthographicCamera Camera { get; }
    
    /// <summary>
    ///     This maps keys to translation directions.
    /// </summary>
    public Dictionary<Key, Direction2D> TranslationControls { get; }

    /// <summary>
    ///     This maps keys to rotation angles.
    /// </summary>
    public Dictionary<Key, float> RotationControls { get; }

    public Vector3 FuturePosition { get; set; } = Vector3.Zero;

    public Vector2 FuturePosition2
    {
        get => new(FuturePosition.X, FuturePosition.Y);
        set => FuturePosition = new Vector3(value.X, value.Y, FuturePosition.Z);
    }

    public float FutureRotation { get; set; } = 0;

    public float FutureZoom { get; set; } = 1;

    private float _currentRotation;

    /// <summary>
    ///     Creates a new instance of the OrthographicCameraController2D class.
    /// </summary>
    /// <param name="camera">The camera to wrap around.</param>
    /// <param name="translationControls">The input keys to use for translation. If null, WASD keys are used.</param>
    /// <param name="rotationControls">The input keys to use for rotation. If null, QE keys are used.</param>
    /// <param name="translationInterpolate">The interpolation speed for translation.</param>
    /// <param name="rotationInterpolate">The interpolation speed for rotation.</param>
    /// <param name="zoomInterpolate">The interpolation speed for zoom.</param>
    public OrthographicCameraController2D(
        OrthographicCamera camera, 
        Dictionary<Key, Direction2D>? translationControls = null, 
        Dictionary<Key, float>? rotationControls = null,
        float translationInterpolate = 0,
        float rotationInterpolate = 0,
        float zoomInterpolate = 0)
    {
        TranslationInterpolate = translationInterpolate;
        RotationInterpolate = rotationInterpolate;
        ZoomInterpolate = zoomInterpolate;
        Camera = camera;

        translationControls ??= new Dictionary<Key, Direction2D>
        {
            { Key.W, Direction2D.Up },
            { Key.S, Direction2D.Down },
            { Key.A, Direction2D.Left },
            { Key.D, Direction2D.Right },
        };

        rotationControls ??= new Dictionary<Key, float>()
        {
            { Key.Q, 1 },
            { Key.E, -1 }
        };

        TranslationControls = translationControls;
        RotationControls = rotationControls;

        Update(1, true);
    }

    /// <summary>
    ///     This is called to process a set of inputs and update the camera state.
    /// </summary>
    /// <param name="key">The key being pressed.</param>
    /// <param name="translateCoefficient">Todo</param>
    /// <param name="rotateCoefficient">Todo</param>
    public void ProcessKey(Key key, float translateCoefficient, float rotateCoefficient)
    {
        if (TranslationControls.TryGetValue(key, out var direction))
        {
            var offset = direction switch
            {
                Direction2D.Left => MathF.PI,
                Direction2D.Right => 0,
                Direction2D.Up => MathF.PI / 2,
                Direction2D.Down => -MathF.PI / 2,
                _ => throw new ArgumentOutOfRangeException()
            };

          
            FuturePosition = new Vector3(
                FuturePosition.X + MathF.Cos(_currentRotation + offset) * translateCoefficient,
                FuturePosition.Y + MathF.Sin(_currentRotation + offset) * translateCoefficient,
                FuturePosition.Z);
        }

        if (RotationControls.TryGetValue(key, out var rotation))
        {
            FutureRotation += rotation * rotateCoefficient;
        }
    }

    // Todo: change the interpolation scheme to be time-independent
    public void Update(float factor, bool snap = false)
    {
        Camera.Position = snap || TranslationInterpolate == 0
            ? FuturePosition 
            : Vector3.Lerp(Camera.Position, FuturePosition, TranslationInterpolate * factor);

        _currentRotation = snap || RotationInterpolate == 0 
            ? FutureRotation 
            : MathHelper.Lerp(_currentRotation, FutureRotation, RotationInterpolate * factor);

        Camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _currentRotation);

        Camera.Zoom = snap || ZoomInterpolate == 0
            ? FutureZoom
            : MathHelper.Lerp(Camera.Zoom, FutureZoom, ZoomInterpolate * factor);
    }
}