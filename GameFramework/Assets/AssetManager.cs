using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GameFramework.Renderer;
using GameFramework.Renderer.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using Veldrid.ImageSharp;

namespace GameFramework.Assets;

/// <summary>
///     This is a basic asset manager with routines for loading assets from the disk or embedded resources.
/// </summary>
public sealed class AssetManager
{
    public GameApplication Application { get; }

    private readonly ConcurrentDictionary<IResourceKey, object> _resources = new();

    private readonly ConcurrentDictionary<IResourceKey, TextureView> _viewCache = new();
    
    public AssetManager(GameApplication application)
    {
        Application = application;
    }

    /// <summary>
    ///     Gets or loads an SDF font from the specified resource.
    /// </summary>
    /// <param name="key">The resource key, pointing to a serialized SDF font.</param>
    public SdfFont GetOrAddFont(IResourceKey key)
    {
        return (SdfFont)_resources.GetOrAdd(key, _ => LoadFont(key));
    }

    /// <summary>
    ///     Loads an SDF font from the specified resource, using its own serialization routines.
    /// </summary>
    private SdfFont LoadFont(IResourceKey key)
    {
        using var ms = new MemoryStream(ReadBinary(key));

        return SdfFont.Load(Application.Device, ms);
    }

    /// <summary>
    ///     Gets or loads a Texture from the specified resource.
    /// </summary>
    public Texture GetOrAddTexture(IResourceKey key)
    {
        return (Texture)_resources.GetOrAdd(key, _ => LoadTexture(key));
    }

    /// <summary>
    ///     Gets or creates a Texture from the specified texture resource. Internally, this uses <see cref="GetOrAddTexture"/>
    /// </summary>
    public TextureView GetView(IResourceKey key)
    {
        return _viewCache.GetOrAdd(key, _ =>
        {
            var texture = GetOrAddTexture(key);
            
            return Application.Resources.Factory.CreateTextureView(texture);
        });
    }

    /// <summary>
    ///     Gets or creates the resources needed for a <see cref="Sprite"/>, from the specified texture resource. Internally, this uses <see cref="GetView"/>
    /// </summary>
    public Sprite GetSpriteForTexture(IResourceKey key, Sampler? sampler = null, TextureCoordinate4? coordinates = null)
    {
        var view = GetView(key);

        coordinates ??= TextureCoordinate4.Full;

        return new Sprite(new TextureSampler(view, sampler ?? Application.Device.LinearSampler), coordinates.Value);
    }

    /// <summary>
    ///     Gets or loads a shader from the specified resource.
    /// </summary>
    public Shader GetOrAddShader(IResourceKey key, ShaderStages stage)
    {
        var shader = (Shader)_resources.GetOrAdd(key, _ => LoadShader(key, stage));

        if (shader.Stage != stage)
        {
            throw new InvalidOperationException($"Invalid shader stage. Actual: {shader.Stage}");
        }

        return shader;
    }

    /// <summary>
    ///     Gets or loads a SPIR-V shader from the specified source code resources.
    /// </summary>
    public ShaderProgram GetOrAddSpirvProgram(IResourceKey vertexSource, IResourceKey fragmentSource)
    {
        var pair = new ResourceKeyPair(vertexSource, fragmentSource);

        return (ShaderProgram)_resources.GetOrAdd(pair, _ => 
            LoadSpirvShaderProgram(vertexSource, fragmentSource));
    }

    /// <summary>
    ///     Gets or loads a binary from the specified resource.
    /// </summary>
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

    /// <summary>
    ///     Opens a stream to read an embedded resource.
    /// </summary>
    /// <param name="name">The path to the embedded resource.</param>
    /// <param name="assembly">The assembly that contains the resource.</param>
    /// <returns>A stream to the resource or null, if a stream could not be opened.</returns>
    public static Stream? OpenEmbeddedResourceStream(string name, Assembly assembly)
    {
        return assembly.GetManifestResourceStream(name);
    }

    /// <summary>
    ///     Tries to read the binary data from an embedded resource.
    /// </summary>
    /// <param name="name">The path to the embedded resource.</param>
    /// <param name="assembly">The assembly that contains the resource.</param>
    /// <param name="bytes">The resulting binary data or null, if the resource could not be read.</param>
    /// <returns>True, if the resource was resolved successfully. Otherwise, false.</returns>
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

    /// <summary>
    ///     Reads the binary data from an embedded resource.
    /// </summary>
    /// <param name="name">The path to the embedded resource.</param>
    /// <param name="assembly">The assembly that contains the resource.</param>
    /// <returns>The data stored at the specified location.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified resource was not found.</exception>
    public static byte[] ReadEmbeddedResource(string name, Assembly assembly)
    {
        if (!TryReadEmbeddedResource(name, assembly, out var bytes))
        {
            throw new KeyNotFoundException($"Failed to get embedded resource {name} in {assembly.FullName}");
        }
      
        return bytes;
    }

    /// <summary>
    ///     Reads the binary data from the specified resource (embedded or file).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the resource could not be read.</exception>
    public static byte[] ReadBinary(IResourceKey key)
    {
        return key switch
        {
            FileResourceKey fileKey => File.ReadAllBytes(fileKey.Path),
            EmbeddedResourceKey embeddedResourceKey => ReadEmbeddedResource(embeddedResourceKey.Path, embeddedResourceKey.Assembly),
            _ => throw new InvalidOperationException($"Unknown key type {key}")
        };
    }

    /// <summary>
    ///     Gets or adds (caches) a resource.
    /// </summary>
    /// <typeparam name="T">The type of resource to store.</typeparam>
    /// <param name="key">The key identifier of this resource.</param>
    /// <param name="loader">A loader function, that will be called if the resource is not present in the cache.</param>
    /// <param name="saver">Called when the resource is first created.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if a resource that maps to the specified key cannot be cast back to <see cref="T"/></exception>
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