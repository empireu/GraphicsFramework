using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace GameFramework.Renderer;

/// <summary>
///     Wraps a ShaderProgram, encapsulating a vertex and a fragment shader.
/// </summary>
public sealed class ShaderProgram
{
    public GraphicsDevice Device { get; }

    public Shader VertexShader { get; }
    
    public Shader PixelShader { get; }

    public ShaderProgram(GraphicsDevice device, Shader vertexShader, Shader pixelShader)
    {
        Device = device;
        VertexShader = vertexShader;
        PixelShader = pixelShader;
    }

    /// <summary>
    ///     Creates a shader program from SPIR-V source code.
    /// </summary>
    /// <param name="device">The graphics device that owns the resources.</param>
    /// <param name="vertexSource">The source code of the vertex shader.</param>
    /// <param name="pixelSource">The source code of the pixel shader.</param>
    /// <param name="vertexEntryPoint">The vertex shader's entry point function. By default, this is called <code>main</code></param>
    /// <param name="pixelEntryPoint">The pixel shader's entry point function. By default, this is called <code>main</code></param>
    /// <returns></returns>
    public static ShaderProgram FromSpirv(GraphicsDevice device, byte[] vertexSource, byte[] pixelSource, string vertexEntryPoint = "main", string pixelEntryPoint = "main")
    {
        var vertexShaderDescription = new ShaderDescription(
            ShaderStages.Vertex,
            vertexSource,
            vertexEntryPoint);

        var fragmentShaderDesc = new ShaderDescription(
            ShaderStages.Fragment,
            pixelSource,
            pixelEntryPoint);

        var shaders = device.ResourceFactory.CreateFromSpirv(vertexShaderDescription, fragmentShaderDesc);

        return new ShaderProgram(device, shaders[0], shaders[1]);
    }

    public Shader[] ToArray()
    {
        return new[] { VertexShader, PixelShader };
    }
}