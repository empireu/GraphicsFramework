using System.Numerics;
using Veldrid;
using Point = System.Drawing.Point;

namespace GameFramework.Renderer;

public sealed class UniformTextureAtlas
{
    public Texture Texture { get; }
    
    public uint CellWidth { get; }
    
    public uint CellHeight { get; }

    public uint Columns { get; }

    public uint Rows { get; }

    public uint Width { get; }

    public uint Height { get; }

    public UniformTextureAtlas(Texture texture, uint cellWidth, uint cellHeight)
    {
        Texture = texture;
        Width = Texture.Width;
        Height = Texture.Height;

        if (Width % cellWidth != 0)
        {
            throw new ArgumentException(nameof(cellWidth));
        }

        if (Height % cellHeight != 0)
        {
            throw new ArgumentException(nameof(cellHeight));
        }

        CellWidth = cellWidth;
        CellHeight = cellHeight;

        Columns = Width / cellWidth;
        Rows = Height / cellHeight;
    }

    public TextureCoordinate4 Coordinates(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= Columns || point.Y >= Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(point));
        }

        //   v1 =  0.5f,  0.5f,
        var topRight = new Vector2(
            (point.X + 1) * CellWidth / (float)Width,
            (point.Y + 1) * CellHeight / (float)Height);

        //   v2 =  0.5f, -0.5f,
        var bottomRight = new Vector2(
            (point.X + 1) * CellWidth / (float)Width,
            point.Y * CellHeight / (float)Height);

        //   v3 = -0.5f, -0.5f,
        var bottomLeft = new Vector2(
            point.X * CellWidth / (float)Width,
            point.Y * CellHeight / (float)Height);

        //   v4 = -0.5f,  0.5f,
        var topLeft = new Vector2(
            point.X * CellWidth / (float)Width,
            (point.Y + 1) * CellHeight / (float)Height);


        return new TextureCoordinate4(topRight, bottomRight, bottomLeft, topLeft);
    }
}