using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace GameFramework.Renderer;

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