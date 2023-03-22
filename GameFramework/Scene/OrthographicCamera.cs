using System.Numerics;

namespace GameFramework.Scene;

public sealed class OrthographicCamera
{
    private Vector3 _position;
    private Quaternion _rotation;
    private float _aspectRatio;
    private float _near;
    private float _far;
    private float _zoom;

    public OrthographicCamera(float aspectRatio, float near, float far)
    {
        _position = Vector3.Zero;
        _rotation = Quaternion.Identity;
        _aspectRatio = aspectRatio;
        _near = near;
        _far = far;
        _zoom = 1;

        UpdateMatrix();
    }
    
    public Matrix4x4 CameraMatrix { get; private set; }

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            UpdateMatrix();
        }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            UpdateMatrix();
        }
    }

    public float AspectRatio
    {
        get => _aspectRatio;
        set
        {
            _aspectRatio = value;
            UpdateMatrix();
        }
    }

    public float Near
    {
        get => _near;
        set
        {
            _near = value;
            UpdateMatrix();
        }
    }

    public float Far
    {
        get => _far;
        set
        {
            _far = value;
            UpdateMatrix();
        }
    }

    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            UpdateMatrix();
        }
    }

    private void UpdateMatrix()
    {
        var left = -_zoom * _aspectRatio * 0.5f;
        var right = _zoom * _aspectRatio * 0.5f;
        var bottom = -_zoom * 0.5f;
        var top = _zoom * 0.5f;

        if (!Matrix4x4.Invert(Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position), out var viewMatrix))
        {
            throw new InvalidOperationException("Invalid camera parameters.");
        }

        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, _near, _far);

        CameraMatrix = viewMatrix * projectionMatrix;
    }
}