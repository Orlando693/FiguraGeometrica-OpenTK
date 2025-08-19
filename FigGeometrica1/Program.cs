using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public abstract class Componente2D
{
    protected float[] vertices;
    protected int vao, vbo;
    protected Vector4 color;

    public Componente2D(float x, float y, float width, float height, Vector4 color)
    {
        this.color = color;

        // Rectángulo hecho con 2 triángulos
        vertices = new float[]
        {
            // Triángulo 1
            x, y,
            x + width, y,
            x + width, y + height,

            // Triángulo 2
            x, y,
            x + width, y + height,
            x, y + height
        };

        // Crear VAO y VBO
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    public void Dibujar(int shaderProgram)
    {
        int colorLoc = GL.GetUniformLocation(shaderProgram, "ourColor");
        GL.Uniform4(colorLoc, color);

        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}

// -------- Componentes de la PC --------
public class Monitor : Componente2D
{
    public Monitor(float x, float y, float width, float height, Vector4 color)
        : base(x, y, width, height, color) { }
}

public class Teclado : Componente2D
{
    public Teclado(float x, float y, float width, float height, Vector4 color)
        : base(x, y, width, height, color) { }
}

public class Mouse : Componente2D
{
    public Mouse(float x, float y, float width, float height, Vector4 color)
        : base(x, y, width, height, color) { }
}

public class Case : Componente2D
{
    public Case(float x, float y, float width, float height, Vector4 color)
        : base(x, y, width, height, color) { }
}

// -------- Ventana principal --------
public class Ventana2D : GameWindow
{
    private int _shaderProgram;

    private Monitor monitor;
    private Teclado teclado;
    private Mouse mouse;
    private Case casePC;

    private readonly string vertexShaderSource = @"
        #version 330 core
        layout(location = 0) in vec2 aPosition;
        void main()
        {
            gl_Position = vec4(aPosition, 0.0, 1.0);
        }
    ";

    private readonly string fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        uniform vec4 ourColor;
        void main()
        {
            FragColor = ourColor;
        }
    ";

    public Ventana2D()
        : base(GameWindowSettings.Default, new NativeWindowSettings() { Title = "PC en 2D", Size = new Vector2i(800, 600) })
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(Color4.CornflowerBlue);

        // Compilar shaders
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // Crear componentes de la PC
        monitor = new Monitor(-0.5f, 0.2f, 1.0f, 0.6f, new Vector4(0.1f, 0.1f, 0.1f, 1.0f)); // gris oscuro
        teclado = new Teclado(-0.4f, -0.3f, 0.8f, 0.1f, new Vector4(0.2f, 0.2f, 0.2f, 1.0f)); // gris
        mouse = new Mouse(0.5f, -0.3f, 0.1f, 0.15f, new Vector4(0.8f, 0.8f, 0.8f, 1.0f)); // blanco
        casePC = new Case(-0.9f, -0.2f, 0.3f, 0.5f, new Vector4(0.15f, 0.15f, 0.15f, 1.0f)); // gris oscuro
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_shaderProgram);

        monitor.Dibujar(_shaderProgram);
        teclado.Dibujar(_shaderProgram);
        mouse.Dibujar(_shaderProgram);
        casePC.Dibujar(_shaderProgram);

        SwapBuffers();
    }
}

// -------- Programa principal --------
public static class Program
{
    public static void Main()
    {
        using (var ventana = new Ventana2D())
        {
            ventana.Run();
        }
    }
}
