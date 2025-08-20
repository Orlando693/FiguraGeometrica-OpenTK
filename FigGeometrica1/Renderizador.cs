using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Renderizador
{
    private int _shaderProgram;

    private readonly string _vertexShader = @"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        void main()
        {
            gl_Position = projection * view * model * vec4(aPosition, 1.0);
        }
    ";

    private readonly string _fragmentShader = @"
        #version 330 core
        out vec4 FragColor;
        uniform vec4 ourColor;
        void main()
        {
            FragColor = ourColor;
        }
    ";

    public void Inicializar()
    {
        CubeMesh.Init();

        int vShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vShader, _vertexShader);
        GL.CompileShader(vShader);

        int fShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fShader, _fragmentShader);
        GL.CompileShader(fShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vShader);
        GL.AttachShader(_shaderProgram, fShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(vShader);
        GL.DeleteShader(fShader);
    }

    public int ObtenerShader() => _shaderProgram;
}
