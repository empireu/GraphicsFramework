using System.Numerics;
using Veldrid;
using Vortice.Mathematics;

namespace GameFramework.Scene;

public sealed class OrthographicCameraController2D
{
    public float TranslationInterpolate { get; set; }
    
    public float RotationInterpolate { get; set; }
    
    public float ZoomInterpolate { get; set; }

    public OrthographicCamera Camera { get; }
    
    public Dictionary<Key, Direction2D> TranslationControls { get; }

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