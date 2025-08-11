using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Ventana : GameWindow
{
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private int _shaderProgram;

    // Vértices para un triangulo
    private readonly float[] _vertices =
    {
        //(X,Y) ya que Z es en 3D
         0.0f,  0.5f, 0.0f, // arriba izq
        -0.5f, -0.5f, 0.0f, // abajo izq
         0.5f, -0.5f, 0.0f, // abajo der

    };

    /*
     * Tecnica para dibujar
    
                (-0.5,  0.5)      (0.5,  0.5)
                     A ┌───────────┐ B
                       │           │
                       │     ■     │   (■ = centro (0,0))
                       │           │
                     D └───────────┘ C
                (-0.5, -0.5)      (0.5, -0.5)


     */

    // Vertex shader con uScale y uOffset
    private readonly string _vertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 aPosition;
        uniform float uScale;
        uniform vec2  uOffset;
        void main()
        {
            vec2 pos = aPosition.xy * uScale + uOffset;
            gl_Position = vec4(pos, aPosition.z, 1.0);
        }
    ";

    // Fragment shader (color fijo rojo)
    private readonly string _fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        void main()
        {
            FragColor = vec4(5.0, 5.0, 0.0, 8.0); 
        }
    "; //RGB

    public Ventana()
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.CornflowerBlue);

        // Crear VBO
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        // Crear VAO
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Compilar shaders
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, _vertexShaderSource);
        GL.CompileShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, _fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_shaderProgram);

        // Pasar valores de uScale y uOffset
        int uScaleLoc = GL.GetUniformLocation(_shaderProgram, "uScale");
        int uOffsetLoc = GL.GetUniformLocation(_shaderProgram, "uOffset");

        GL.Uniform1(uScaleLoc, 0.9f);        // Escala: 90% del tamaño original
        GL.Uniform2(uOffsetLoc, 0.0f, 0.0f); // Offset: mover (x,y)

        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 4); // 6 vértices = 2 triángulos

        SwapBuffers();
    }
}

public static class Program
{
    public static void Main()
    {
        var nativeSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "Cuadrado con uScale y uOffset"
        };

        using (var ventana = new Ventana())
        {
            ventana.Run();
        }
    }
}
