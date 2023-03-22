using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GameFramework.Assets.Attributes;
using GameFramework.Renderer;
using GameFramework.Renderer.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using Veldrid.ImageSharp;

namespace GameFramework.Assets;

public sealed class AssetManager
{
    public GameApplication Application { get; }

    private readonly ConcurrentDictionary<IResourceKey, object> _resources = new();
   
    private readonly ConcurrentDictionary<IResourceKey, TextureView> _viewCache = new();
    
    public AssetManager(GameApplication application)
    {
        Application = application;

    }

    public SdfFont GetOrAddFont(IResourceKey key)
    {
        return (SdfFont)_resources.GetOrAdd(key, _ => LoadFont(key));
    }

    private SdfFont LoadFont(IResourceKey key)
    {
        using var ms = new MemoryStream(ReadBinary(key));

        return SdfFont.Load(Application.Device, ms);
    }

    public Texture GetOrAddTexture(IResourceKey key)
    {
        return (Texture)_resources.GetOrAdd(key, _ => LoadTexture(key));
    }

    public TextureView GetView(IResourceKey key)
    {
        return _viewCache.GetOrAdd(key, _ =>
        {
            var texture = GetOrAddTexture(key);
            
            return Application.Resources.Factory.CreateTextureView(texture);
        });
    }

    public Sprite GetSpriteForTexture(IResourceKey key, Sampler? sampler = null, TextureCoordinate4? coordinates = null)
    {
        var view = GetView(key);

        coordinates ??= TextureCoordinate4.Full;

        return new Sprite(new TextureSampler(view, sampler ?? Application.Device.LinearSampler), coordinates.Value);
    }

    public Shader GetOrAddShader(IResourceKey key, ShaderStages stage)
    {
        var shader = (Shader)_resources.GetOrAdd(key, _ => LoadShader(key, stage));

        if (shader.Stage != stage)
        {
            throw new InvalidOperationException($"Invalid shader stage. Actual: {shader.Stage}");
        }

        return shader;
    }

    public ShaderProgram GetOrAddSpirvProgram(IResourceKey vertexSource, IResourceKey fragmentSource)
    {
        var pair = new ResourceKeyPair(vertexSource, fragmentSource);

        return (ShaderProgram)_resources.GetOrAdd(pair, _ => 
            LoadSpirvShaderProgram(vertexSource, fragmentSource));
    }

    public byte[] GetOrAddBinary(IResourceKey key)
    {
        return (byte[])_resources.GetOrAdd(key, _ => ReadBinary(key));
    }

    private Texture LoadTexture(IResourceKey key)
    {
        var binary = ReadBinary(key);

        using var ms = new MemoryStream(binary);
        var image = Image.Load<Rgba32>(ms);

        image.Mutate(context =>
        {
            context.Flip(FlipMode.Vertical);
        });

        var imageSharp = new ImageSharpTexture(image);

        return imageSharp.CreateDeviceTexture(Application.Resources.Device, Application.Resources.Factory);
    }

    private Shader LoadShader(IResourceKey key, ShaderStages stage)
    {
        var binary = ReadBinary(key);

        return Application.Resources.Factory.CreateShader(new ShaderDescription(stage, binary, "main"));
    }

    private ShaderProgram LoadSpirvShaderProgram(IResourceKey vertexSource, IResourceKey fragmentSource)
    {
        return ShaderProgram.FromSpirv(Application.Device, ReadBinary(vertexSource), ReadBinary(fragmentSource));
    }

    public static Stream? OpenEmbeddedResourceStream(string name, Assembly assembly)
    {
        return assembly.GetManifestResourceStream(name);
    }


    public static bool TryReadEmbeddedResource(string name, Assembly assembly, [NotNullWhen(true)] out byte[]? bytes)
    {
        var stream = OpenEmbeddedResourceStream(name, assembly);

        if (stream == null)
        {
            bytes = null;
            return false;
        }

        bytes = new byte[stream.Length];
        using var ms = new MemoryStream(bytes);
        stream.CopyTo(ms);

        return true;
    }

    public static byte[] ReadEmbeddedResource(string name, Assembly assembly)
    {
        if (!TryReadEmbeddedResource(name, assembly, out var bytes))
        {
            throw new KeyNotFoundException($"Failed to get embedded resource {name} in {assembly.FullName}");
        }
      
        return bytes;
    }

    public static byte[] ReadBinary(IResourceKey key)
    {
        return key switch
        {
            FileResourceKey fileKey => File.ReadAllBytes(fileKey.Path),
            EmbeddedResourceKey embeddedResourceKey => ReadEmbeddedResource(embeddedResourceKey.Path, embeddedResourceKey.Assembly),
            _ => throw new InvalidOperationException($"Unknown key type {key}")
        };
    }

    public T GetOrAdd<T>(IResourceKey key, Func<IResourceKey, T> loader, Action<IResourceKey, T>? saver = null)
    {
        var obj = _resources.GetOrAdd(key, _ =>
        {
            var instance = loader(key);

            if (instance == null)
            {
                throw new InvalidOperationException($"Loader created null {typeof(T)}");
            }

            saver?.Invoke(key, instance);

            return instance;
        });

        if (obj is not T result)
        {
            throw new InvalidOperationException(
                $"An object of type {obj.GetType()} was loaded for {typeof(T)}, with key {key}");
        }

        return result;
    }
}