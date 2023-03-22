using System.Numerics;

namespace GameFramework.Scene;

/// <summary>
///     Manages an orthographic projection matrix.
/// </summary>
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

    /// <summary>
    ///     Sets the position of the view matrix. This will cause the camera matrix to be re-calculated.
    /// </summary>
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            UpdateMatrix();
        }
    }

    /// <summary>
    ///     Sets the rotation of the view matrix. This will cause the camera matrix to be re-calculated.
    /// </summary>
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            UpdateMatrix();
        }
    }

    /// <summary>
    ///     Sets the aspect ratio of the projection matrix. This will cause the matrix to be re-calculated.
    /// </summary>
    public float AspectRatio
    {
        get => _aspectRatio;
        set
        {
            _aspectRatio = value;
            UpdateMatrix();
        }
    }

    /// <summary>
    ///     Sets the near plane distance of the projection matrix. This will cause the matrix to be re-calculated.
    /// </summary>
    public float Near
    {
        get => _near;
        set
        {
            _near = value;
            UpdateMatrix();
        }
    }

    /// <summary>
    ///     Sets the far plane distance of the projection matrix. This will cause the matrix to be re-calculated.
    /// </summary>
    public float Far
    {
        get => _far;
        set
        {
            _far = value;
            UpdateMatrix();
        }
    }

    /// <summary>
    ///     Sets the zoom of the projection matrix. This will cause the matrix to be re-calculated.
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            UpdateMatrix();
        }
    }

    /// <summary>
    ///     Rebuilds the matrices using the latest state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the parameters caused the matrix to fail to build.</exception>
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