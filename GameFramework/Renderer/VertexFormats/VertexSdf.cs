using System.Numerics;
using GameFramework.Renderer.Batch;

namespace GameFramework.Renderer.VertexFormats;

public struct VertexSdf
{
    public Vector3 Position;
    public Vector2 TextureCoordinate;
    public SdfOptions SdfOptions;
}