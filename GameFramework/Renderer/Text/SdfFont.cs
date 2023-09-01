using System.Numerics;
using GameFramework.Renderer.Batch;
using MessagePack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Image = SixLabors.ImageSharp.Image;
using RectangleF = System.Drawing.RectangleF;

namespace GameFramework.Renderer.Text;

/// <summary>
///     An SDF font uses low resolution SDF glyphs to render high quality text.
/// </summary>
public sealed class SdfFont
{
    public SdfSheet Sheet { get; }


    public SdfOptions4 Options;
    
    public float VerticalSpacing { get; set; } = 0.01f;

    public float HorizontalSpacing { get; set; } = 0.05f;

    public float SpaceSize { get; set; } = 0.5f;

    public SdfFont(SdfSheet sheet, SdfOptions4 options)
    {
        Sheet = sheet;
        Options = options;
    }

    public float LineHeight(float size)
    {
        return Sheet.Map.Keys.Max(c => Measure(c, size, true).Y) + VerticalSpacing * size;
    }

    /// <summary>
    ///     Measures a single character.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    /// <param name="size">The font size to use.</param>
    /// <param name="withoutPadding">
    ///     If true, no padding will be applied to this measurement,
    ///     resulting in a close approximation of the character's size (as specified by the font).
    /// </param>
    /// <returns>The size of the character, if it were to be rendered.</returns>
    public Vector2 Measure(char c, float size, bool withoutPadding = false)
    {
        var properties = Sheet.GetProperties(c);

        var glyphSize = properties.SourceGlyphSize;
        var padding = properties.Padding;

        if (withoutPadding)
        {
            glyphSize -= 2 * padding * properties.SourceGlyphSize;
        }

        glyphSize = Vector2.Normalize(glyphSize);
        glyphSize *= size;

        return glyphSize;
    }

    /// <summary>
    ///     Increments a tracking position after placing a character in the grid.
    /// </summary>
    /// <param name="x">The current horizontal position in the grid.</param>
    /// <param name="y">The current vertical position in the grid.</param>
    /// <param name="size">The font size to use.</param>
    /// <param name="lineHeight">The line height, in units. This increment will be used when moving to a new row.</param>
    /// <param name="c">The character to measure.</param>
    public void PlaceCharacter(ref float x, ref float y, float size, float lineHeight, char c)
    {
        if (c == ' ')
        {
            x += size * SpaceSize;
            
            return;
        }

        if (c == '\n')
        {
            y += lineHeight;
            x = 0;

            return;
        }

        var charSize = Measure(c, size, true);

        var stride = charSize.X;
        
        x += stride + HorizontalSpacing * size;
    }

    /// <summary>
    ///     Saves this SDF font to a stream.
    /// </summary>
    /// <param name="stream">The stream to save to.</param>
    /// <exception cref="InvalidOperationException">Thrown if the glyph atlas is not available on the CPU side.</exception>
    public void Save(Stream stream)
    {
        var image = Sheet.SheetImage;

        if (image == null)
        {
            throw new InvalidOperationException("Cannot save SDF because it does not have the image stored.");
        }

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        
        MessagePackSerializer.Serialize(stream, new DiskStorage
        {
            ImageBinary = ms.ToArray(),
            Fallback = Sheet.Fallback,
            Map = Sheet.Map,
            Options = Options,
            VerticalSpacing = VerticalSpacing,
            RasterFontSize = Sheet.RasterFontSize,
            HorizontalSpacing = HorizontalSpacing,
            SpaceSize = SpaceSize
        });

        stream.Flush();
    }

    /// <summary>
    ///     Loads an SDF font from a stream.
    /// </summary>
    /// <param name="device">The graphics device that owns the graphics resources.</param>
    /// <param name="stream">The stream to load from.</param>
    /// <returns>An SDF font restored from the stream.</returns>
    public static SdfFont Load(GraphicsDevice device, Stream stream)
    {
        var storage = MessagePackSerializer.Deserialize<DiskStorage>(stream);

        using var ms = new MemoryStream(storage.ImageBinary);
        var image = Image.Load<Rgba32>(ms);
        var sheet = SdfSheet.Create(device, image, storage.Map, storage.RasterFontSize, storage.Fallback);

        return new SdfFont(sheet, storage.Options)
        {
            VerticalSpacing = storage.VerticalSpacing,
            HorizontalSpacing = storage.HorizontalSpacing,
            SpaceSize = storage.SpaceSize
        };
    }

    /// <summary>
    ///     Iterates over the text grid, providing the position of each character.
    /// </summary>
    /// <param name="text">The text to iterate over. Newlines will result in new rows being added.</param>
    /// <param name="size">The font size to use.</param>
    /// <returns>
    ///     The character at the current position,
    ///     the current position in the grid,
    ///     and a flag indicating if the character is a control character (currently, only newlines fit under this description)
    /// </returns>
    private IEnumerable<(char Char, Vector2 Translation, bool IsEmpty)> IterateGrid(string text, float size)
    {
        var y = 0f;
        var x = 0f;

        var lineHeight = LineHeight(size);

        for (var index = 0; index < text.Length; index++)
        {
            var @char = text[index];

            if (@char == '\r')
            {
                if (index + 1 < text.Length && text[index + 1] == '\n')
                {
                    continue;
                }

                @char = '\n';
            }

            var charSize = Measure(@char, size, true);

            var translation = new Vector2(x + charSize.X / 2, -(y + size / 2));

            PlaceCharacter(ref x, ref y, size, lineHeight, @char);

            yield return (@char, translation, !(@char != ' ' && @char != '\n'));
        }
    }

    /// <summary>
    ///     Renders the text to the specified batch.
    /// </summary>
    /// <param name="batch">The batch to render to.</param>
    /// <param name="position">The top left position of the text grid.</param>
    /// <param name="text">The text to render. Newlines will result in new rows being added.</param>
    /// <param name="color">The color to render with.</param>
    /// <param name="size">The font size to use.</param>
    public void Render(QuadBatch batch, Vector2 position, string text, Vector3? color = null, float size = 1)
    {
        var options = color == null 
            ? Options 
            : Options.WithColor(color.Value);

        var textureSampler = new TextureSampler(Sheet.View, batch.Device.LinearSampler);
        var transform = Matrix4x4.CreateScale(size, size, 0);

        foreach (var (@char, translation, isEmpty) in IterateGrid(text, size))
        {
            if (isEmpty)
            {
                continue;
            }

            transform.Translation = new Vector3(position.X + translation.X, position.Y + translation.Y, 0);

            batch.AddSdfQuad(
                textureSampler,
                transform,
                Sheet.GetProperties(@char).Coordinates,
                options
            );
        }
    }
    
    /// <summary>
    ///     Measures the size of a text grid.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="size">The font size to use.</param>
    /// <returns>The size of the resulting text grid, if it were to be rendered.</returns>
    public Vector2 MeasureText(string text, float size)
    {
        var min = new Vector2(float.MaxValue);
        var max = new Vector2(float.MinValue);

        var foundChars = false;
        var extent = new Vector2(size / 2f);

        foreach (var (_, translation, isEmpty) in IterateGrid(text, size))
        {
            if (isEmpty)
            {
                continue;
            }

            foundChars = true;
            min = Vector2.Min(min, translation - extent);
            max = Vector2.Max(max, translation + extent);
        }

        return foundChars ? max - min : Vector2.Zero;
    }

    [MessagePackObject]
    public struct DiskStorage
    {
        [Key(0)]
        public byte[] ImageBinary;

        [Key(1)]
        public Dictionary<char, SdfCharacterProperties> Map;

        [Key(2)]
        public char Fallback;

        [Key(3)]
        public SdfOptions4 Options;

        [Key(4)]
        public float VerticalSpacing;

        [Key(5)]
        public float HorizontalSpacing;

        [Key(6)]
        public float RasterFontSize;

        [Key(7)]
        public float SpaceSize;
    }
}