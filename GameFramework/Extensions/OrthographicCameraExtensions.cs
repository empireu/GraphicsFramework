using System.Numerics;
using GameFramework.Scene;

namespace GameFramework.Extensions;

public static class OrthographicCameraExtensions
{
    public static Vector2 MouseToWorld2D(this OrthographicCamera camera, Vector2 mousePosition, int windowWidth, int windowHeight)
    {
        var xClip = mousePosition.X / windowWidth * 2 - 1;
        var yClip = mousePosition.Y / windowHeight * 2 - 1;

        if (!Matrix4x4.Invert(camera.CameraMatrix, out var qCameraMatrix))
        {
            throw new ArgumentException("Invalid camera matrix.");
        }

        var result = Vector4.Transform(new Vector4(xClip, -yClip, 0, 1), qCameraMatrix);

        return new Vector2(result.X, result.Y);
    }
}