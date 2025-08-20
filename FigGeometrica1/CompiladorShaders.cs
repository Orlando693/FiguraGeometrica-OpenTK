using OpenTK.Graphics.OpenGL4;

public static class CompiladorShaders
{
    public static int CrearPrograma(string vertexSource, string fragmentSource)
    {
        int vShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vShader, vertexSource);
        GL.CompileShader(vShader);

        int fShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fShader, fragmentSource);
        GL.CompileShader(fShader);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vShader);
        GL.AttachShader(program, fShader);
        GL.LinkProgram(program);

        GL.DeleteShader(vShader);
        GL.DeleteShader(fShader);

        return program;
    }
}
