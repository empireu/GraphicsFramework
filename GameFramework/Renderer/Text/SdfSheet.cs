using System.Collections.Concurrent;
using System.Numerics;
using GameFramework.Sdf;
using GameFramework.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using Veldrid.ImageSharp;

namespace GameFramework.Renderer.Text;

public sealed class SdfSheet
{
    public SdfSheet(Texture texture, TextureView view, Dictionary<char, SdfCharacterProperties> map, float rasterFontSize, char fallback = '?')
    {
        if (!map.ContainsKey(fallback))
        {
            throw new InvalidOperationException("Map does not contain fallback.");
        }

        Fallback = fallback;

        Texture = texture;
        Map = map;
        RasterFontSize = rasterFontSize;
        View = view;
    }

    public Image<Rgba32>? SheetImage { get; set; }

    public Texture Texture { get; }

    public TextureView View { get; }

    public Dictionary<char, SdfCharacterProperties> Map { get; }

    public char Fallback { get; }

    public float RasterFontSize { get;}

    public static SdfSheet Create(GraphicsDevice device, Image<Rgba32> sdf, Dictionary<char, SdfCharacterProperties> map, float rasterSize, char fallback = '?')
    {
        var imageSharp = new ImageSharpTexture(sdf, false);
        var texture = imageSharp.CreateDeviceTexture(device, device.ResourceFactory);
        var view = device.ResourceFactory.CreateTextureView(texture);

        return new SdfSheet(texture, view, map, rasterSize, fallback)
        {
            SheetImage = sdf
        };
    }

    public static SdfSheet Compute(
        GraphicsDevice device,
        HashSet<char> charset,
        Font font,
        char fallback = '?',
        int maxRowWidth = 256,
        int upscaleSize = 64,
        int sdfSize = 64,
        int padding = 32,
        ParallelOptions? options = null)
    {
        CreateSdfSheet(
            charset, 
            font, 
            maxRowWidth, 
            upscaleSize, 
            sdfSize,
            padding,
            out var sheet,
            out var properties, options);

        var result = Create(device, sheet, properties, font.Size, fallback);

        result.SheetImage = sheet;

        return result;
    }

    public static void CreateSdfSheet(
        HashSet<char> charset,
        Font font,
        int maxRowWidth,
        int upscaleSize,
        int sdfSize,
        int padding,
        out Image<Rgba32> sheet,
        out Dictionary<char, SdfCharacterProperties> properties,
        ParallelOptions? parallelOptions = null,
        bool flip = true)
    {
        var glyphSdfMap = new ConcurrentDictionary<char, Image<Rgba32>>();
      
        var rendererOptions = new RendererOptions(font);
        
        parallelOptions ??= new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        Parallel.ForEach(charset, parallelOptions, c =>
        {
            var text = $"{c}";

            var charRectangle = TextMeasurer.Measure(text, rendererOptions);
            var raster = new Image<Rgba32>((int)Math.Ceiling(charRectangle.Width), (int)Math.Ceiling(charRectangle.Height));

            raster.Mutate(context =>
            {
                context.DrawText(text, font, Color.White, PointF.Empty);

                if (flip)
                {
                    context.Flip(FlipMode.Vertical);
                }
            });

            var sdf = SdfCompute.GenerateSignedDistanceField(raster, upscaleSize, sdfSize,padding);

            glyphSdfMap.TryAdd(c, sdf);
        });

        // now pack them into an atlas

        var boxes = new List<RectanglePacker.RectangleHolder<(char, Image<Rgba32>)>>();

        foreach (var (@char, image) in glyphSdfMap)
        {
            boxes.Add(new RectanglePacker.RectangleHolder<(char, Image<Rgba32>)>((@char, image), image.Width, image.Height));
        }

        RectanglePacker.Pack(boxes, maxRowWidth);

        var atlasWidth = boxes.Max(x => x.X + x.Width);
        var atlasHeight = boxes.Max(x => x.Y + x.Height);

        // map texture properties

        var map = new Dictionary<char, SdfCharacterProperties>();

        sheet = new Image<Rgba32>(atlasWidth, atlasHeight, Color.Black);

        sheet.Mutate(context =>
        {
            foreach (var box in boxes)
            {
                var (@char, image) = box.Instance;

                var x = box.X / (float)atlasWidth;
                var y = box.Y / (float)atlasHeight;
                var width = box.Width / (float)atlasWidth;
                var height = box.Height / (float)atlasHeight;

                var paddingSize = new Vector2(padding / (float)box.Width, padding / (float)box.Height);

                var coordinates = new TextureCoordinate4(
                    new Vector2(x + width, y + height),
                    new Vector2(x + width, y),
                    new Vector2(x, y),
                    new Vector2(x, y + height));


                map.Add(@char, new SdfCharacterProperties(paddingSize, coordinates, new Vector2(box.Width, box.Height)));

                context.DrawImage(image, new SixLabors.ImageSharp.Point(box.X, box.Y), 1);
            }
        });

        properties = map;
    }

    public SdfCharacterProperties GetProperties(char c)
    {
        if (!Map.TryGetValue(c, out var result))
        {
            result = Map[Fallback];
        }

        return result;
    }
}