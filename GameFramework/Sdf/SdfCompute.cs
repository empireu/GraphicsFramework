using GameFramework.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace GameFramework.Sdf;

/// <summary>
///     Class with methods for computing SDF glyphs.
/// </summary>
public static class SdfCompute
{
    /// <summary>
    ///     Thresholds the image to generate a bit grid.
    /// </summary>
    /// <param name="bitmap">The bitmap to threshold.</param>
    /// <returns>A memory that can be used to create a <see cref="BitGrid"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static IMemoryOwner<byte> ExtractMask(Image<Rgba32> bitmap)
    {
        var length = bitmap.Width * bitmap.Height;
        
        var buffer = MemoryPool<byte>.Shared.Rent(length);

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap[x, y];

                ref var @byte = ref buffer.Memory.Span[y * bitmap.Width + x];

                @byte = pixel.R + pixel.G + pixel.B > 100 ? (byte)1 : (byte)0;
            }
        }

        return buffer;
    }

    /// <summary>
    ///     Generates a signed distance field from the specified image.
    /// </summary>
    /// <param name="glyph">The image to generate from.</param>
    /// <param name="upscaleResolution">The resolution to query cells at. This affects the density field.</param>
    /// <param name="size">Size hint for the final SDF image.</param>
    /// <param name="padding">Padding pixels. Small values will cause the distance field to be cut off.</param>
    /// <returns>A signed distance field image.</returns>
    public static Image<Rgba32> GenerateSignedDistanceField(
        Image<Rgba32> glyph,
        int upscaleResolution,
        int size,
        int padding = 12)
    {
        var spread = upscaleResolution / 2;

        using var monochromeGlyphMemory = ExtractMask(glyph);

        var glyphWidth = glyph.Width;
        var glyphHeight = glyph.Height;

        var monoGlyph = monochromeGlyphMemory
            .Memory
            .Span[..(glyphWidth * glyphHeight)];

        var glyphWidthRatio = glyphWidth / (float)upscaleResolution;
        var glyphHeightRatio = glyphHeight / (float)upscaleResolution;

        var characterWidth = (int)(size * glyphWidthRatio);
        var characterHeight = (int)(size * glyphHeightRatio);

        var bitmapWidth = characterWidth + padding * 2;
        var bitmapHeight = characterHeight + padding * 2;

        var bitmapScaleX = glyphWidth / (float)characterWidth;
        var bitmapScaleY = glyphHeight / (float)characterHeight;

        var resultData = new byte[bitmapWidth * bitmapHeight];

        var minScaledPadX = -padding * bitmapScaleX;
        var minScaledPadY = -padding * bitmapScaleY;

        var paddedCharacterWidth = characterWidth + padding;
        var paddedCharacterHeight = characterHeight + padding;

        var bitfield = new BitGrid(monoGlyph, glyphWidth, glyphHeight);

        var litPixels = new List<(int, int)>();

        for (var y = 0; y < bitfield.Height; y++)
        {
            for (var x = 0; x < bitfield.Width; x++)
            {
                if (bitfield.At(x, y, out _) == 1)
                {
                    litPixels.Add((x, y));
                }
            }
        }

        for (var y = -padding; y < characterHeight + padding; y++)
        {
            for (var x = -padding; x < characterWidth + padding; x++)
            {
                var glyphX = (int)MathUtilities.MapRange(
                    x,
                    -padding, paddedCharacterWidth,
                    minScaledPadX, paddedCharacterWidth * bitmapScaleX);

                var glyphY = (int)MathUtilities.MapRange(
                    y,
                    -padding, paddedCharacterHeight,
                    minScaledPadY, paddedCharacterHeight * bitmapScaleY);

                var distance = Spiral(
                    bitfield,
                    glyphX,
                    glyphY,
                    spread,
                    litPixels);

                resultData[x + padding + (y + padding) * bitmapWidth] = (byte)(distance * 255f);
            }
        }

        var result = new Image<Rgba32>(bitmapWidth, bitmapHeight);

        for (var y = 0; y < bitmapHeight; y++)
        {
            for (var x = 0; x < bitmapWidth; x++)
            {
                var resultValue = resultData[y * bitmapWidth + x];

                result[x, y] = new Rgba32(resultValue, resultValue, resultValue, 255);
            }
        }

        return result;
    }

    /// <summary>
    ///     Searches the bit grid for neighbor candidates in a spiral pattern.
    ///     This is generally faster than a brute force search, assuming that the neighbor is close to the pixel being searched. The pattern causes cache misses for large search areas.
    /// </summary>
    /// <param name="bitGrid">The bit grid to search in.</param>
    /// <param name="pxX">The horizontal position of the target cell.</param>
    /// <param name="pxY">The vertical position of the target cell.</param>
    /// <param name="spread">The search size, in pixels.</param>
    /// <param name="litPixels">All lit pixels in the grid. Used as a fallback for situations where a pixel that is out of bounds is searched.</param>
    /// <returns>The distance to the closest pixel of a different state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static float Spiral(
        BitGrid bitGrid,
        int pxX,
        int pxY,
        int spread,
        List<(int, int)> litPixels)
    {
        var state = bitGrid.At(pxX, pxY, out var outOfBounds);
        float minSqr = spread * spread;

        if (outOfBounds)
        {
            // do not convert to foreach
            for (var i = 0; i < litPixels.Count; i++)
            {
                var (x, y) = litPixels[i];

                var dx = pxX - x;
                var dy = pxY - y;

                var dSqr = dx * dx + dy * dy;

                if (dSqr < minSqr)
                {
                    minSqr = dSqr;
                }
            }

            goto finish;
        }

        var spiral = new SpiralIterator();
        var findingLayer = -1;

        for (var i = 0; i < spread * spread * 4; i++)
        {
            if (findingLayer != -1 && spiral.Level > findingLayer)
            {
                break;
            }

            var px = pxX + spiral.X;
            var py = pxY + spiral.Y;

            if (bitGrid.At(px, py, out _) != state)
            {
                var dSqr = spiral.X * spiral.X + spiral.Y * spiral.Y;
                findingLayer = spiral.Level;

                if (dSqr < minSqr)
                {
                    minSqr = dSqr;
                }
            }

            spiral.Next();
        }

        finish:

        var min = (float)(Math.Sqrt(minSqr));
        var output = (min - 0.5f) / (spread - 0.5f);
        output *= state == 0 ? -1f : 1f;
        return (output + 1) * 0.5f;
    }
}