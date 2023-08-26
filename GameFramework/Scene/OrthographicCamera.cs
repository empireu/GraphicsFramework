using System.Numerics;

namespace GameFramework.Scene;

public sealed class OrthographicCamera
{
    private Vector3 _position = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private float _aspectRatio = 1f;
    private float _near = 0.001f;
    private float _far = 10000f;
    private float _zoom = 1f;

    public OrthographicCamera()
    {
        UpdateMatrix();
    }

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

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            if (!value.Equals(_rotation))
            {
                _rotation = value;
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

    public float Zoom
    {
        get => _zoom;
        set
        {
            if (!value.Equals(_zoom))
            {
                _zoom = value;
                UpdateMatrix();
            }
        }
    }
    
    #endregion

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