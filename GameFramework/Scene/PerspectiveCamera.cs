using System.Numerics;

namespace GameFramework.Scene;

public sealed class PerspectiveCamera
{
    private Vector3 _position;
    private float _aspectRatio = 1;
    private float _pitch;
    private float _yaw = -MathF.PI / 2;
    private float _fov = MathF.PI / 3f;
    private float _near = 0.001f;
    private float _far = 10_000f;

    public PerspectiveCamera()
    {
        UpdateMatrix();
    }

    public PerspectiveCamera(float fov, float aspect, float near, float far)
    {
        _fov = fov;
        _aspectRatio = aspect;
        _near = near;
        _far = far;

        UpdateMatrix();
    }
    public Vector3 Front { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 Up { get; private set; }
    public Matrix4x4 CameraMatrix { get; private set; }

    #region Settings

    public Vector3 Position
    {
        get => _position;
        set
        {
            if (!value.Equals(_position))
            {
                _position = value;
                UpdateMatrix();
            }
        }
    }

    public float Pitch
    {
        get => _pitch;
        set
        {
            if (!value.Equals(_pitch))
            {
                _pitch = value;
                UpdateMatrix();
            }

        }
    }

    public float Yaw
    {
        get => _yaw;
        set
        {
            if (!value.Equals(_yaw))
            {
                _yaw = value;
                UpdateMatrix();
            }
        }
    }

    public float AspectRatio
    {
        get => _aspectRatio;
        set
        {
            if (!value.Equals(_aspectRatio))
            {
                _aspectRatio = value;
                UpdateMatrix();
            }
        }
    }

    public float Fov
    {
        get => _fov;
        set
        {
            if (!value.Equals(_fov))
            {
                _fov = value;
                UpdateMatrix();
            }
        }
    }

    public float Near
    {
        get => _near;
        set
        {
            if (!value.Equals(_near))
            {
                _near = value;
                UpdateMatrix();
            }
        }
    }

    public float Far
    {
        get => _far;
        set
        {
            if (!value.Equals(_far))
            {
                _far = value;
                UpdateMatrix();
            }
        }
    }

    #endregion

    private void UpdateMatrix()
    {
        var worldUp = Vector3.UnitY;

        Front = Vector3.Normalize(new Vector3(
            MathF.Cos(Pitch) * MathF.Cos(Yaw),
            MathF.Sin(Pitch),
            MathF.Cos(Pitch) * MathF.Sin(Yaw)));

        Right = Vector3.Normalize(Vector3.Cross(Front, worldUp));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));

        var viewMatrix = Matrix4x4.CreateLookAt(Position, Position + Front, Up);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Fov, AspectRatio, Near, Far);
        
        CameraMatrix = viewMatrix * projectionMatrix;
    }
}