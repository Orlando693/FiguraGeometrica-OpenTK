using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Cubo3D : GameWindow
{
    private int _vertexArrayObject;
    private int _vertexBufferObject;
    private int _elementBufferObject;
    private int _lineElementBufferObject;
    private int _shaderProgram;

    private Matrix4 _model;
    private Matrix4 _view;
    private Matrix4 _projection;

    private float _cameraX = 2f, _cameraY = 2f, _cameraZ = 3f;

    private readonly float[] _vertices =
    {
        // posiciones XYZ
        -0.5f, -0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f, -0.5f,  0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f
    };

    // Índices para las caras (triángulos)
    private readonly uint[] _indices =
    {
        0, 1, 2, 2, 3, 0, // atrás
        4, 5, 6, 6, 7, 4, // frente
        0, 4, 7, 7, 3, 0, // izquierda
        1, 5, 6, 6, 2, 1, // derecha
        3, 2, 6, 6, 7, 3, // arriba
        0, 1, 5, 5, 4, 0  // abajo
    };

    // Índices solo para las aristas del cubo
    private readonly uint[] _lineIndices =
    {
        0, 1, 1, 2, 2, 3, 3, 0, // atrás
        4, 5, 5, 6, 6, 7, 7, 4, // frente
        0, 4, 1, 5, 2, 6, 3, 7  // conexiones
    };

    private readonly string _vertexShaderSource = @"
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

    private readonly string _fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        uniform vec4 ourColor;
        void main()
        {
            FragColor = ourColor;
        }
    ";

    public Cubo3D()
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(800, 600));
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.CornflowerBlue);
        GL.Enable(EnableCap.DepthTest);

        // VAO
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        // VBO
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        // EBO para caras
        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        // Atributos
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // EBO para líneas
        _lineElementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _lineElementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _lineIndices.Length * sizeof(uint), _lineIndices, BufferUsageHint.StaticDraw);

        // Shaders
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

        // Matrices iniciales
        _model = Matrix4.Identity;
        _view = Matrix4.LookAt(new Vector3(_cameraX, _cameraY, _cameraZ), Vector3.Zero, Vector3.UnitY);
        _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var input = KeyboardState;
        float speed = 1.5f * (float)args.Time;

        // Movimiento con W S A D Q E
        if (input.IsKeyDown(Keys.W) || input.IsKeyDown(Keys.Up)) _cameraZ -= speed;
        if (input.IsKeyDown(Keys.S) || input.IsKeyDown(Keys.Down)) _cameraZ += speed;
        if (input.IsKeyDown(Keys.A) || input.IsKeyDown(Keys.Left)) _cameraX -= speed;
        if (input.IsKeyDown(Keys.D) || input.IsKeyDown(Keys.Right)) _cameraX += speed;
        if (input.IsKeyDown(Keys.Q)) _cameraY -= speed;
        if (input.IsKeyDown(Keys.E)) _cameraY += speed;

        _view = Matrix4.LookAt(new Vector3(_cameraX, _cameraY, _cameraZ), Vector3.Zero, Vector3.UnitY);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(_shaderProgram);

        int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
        int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        int colorLoc = GL.GetUniformLocation(_shaderProgram, "ourColor");

        GL.UniformMatrix4(modelLoc, false, ref _model);
        GL.UniformMatrix4(viewLoc, false, ref _view);
        GL.UniformMatrix4(projLoc, false, ref _projection);

        // --- Dibujar caras ---
        GL.BindVertexArray(_vertexArrayObject);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.Uniform4(colorLoc, new Vector4(0.8f, 0.3f, 0.3f, 1.0f));
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

        // --- Dibujar aristas ---
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _lineElementBufferObject);
        GL.Uniform4(colorLoc, new Vector4(0f, 0f, 0f, 1f));
        GL.DrawElements(PrimitiveType.Lines, _lineIndices.Length, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }
}

public static class Program
{
    public static void Main()
    {
        var nativeSettings = new NativeWindowSettings()
        {
            Title = "Cubo 3D Pintado con Aristas",
            Size = new Vector2i(800, 600)
        };
        using (var window = new Cubo3D())
        {
            window.Run();
        }
    }
}
