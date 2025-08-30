using System;
using System.Collections.Generic;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class VentanaPrincipal : GameWindow
{
    // Shaders / buffers
    private int _program;
    private int _vao, _vbo;

    // Cámara
    private Vector3 _camPos = new Vector3(6, 6, 10);
    private Vector3 _camFront = -Vector3.UnitZ;
    private Vector3 _camUp = Vector3.UnitY;
    private float _yaw = -90f, _pitch = 0f;
    private bool _firstMouse = true;
    private float _lastX, _lastY;
    private Matrix4 _view, _proj;

    // Escena
    private readonly List<Objeto> _objetos = new();

    public VentanaPrincipal()
        : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            Title = "Objeto → Parte → Poligono → Punto (con Centro de Masa)",
            Size = new Vector2i(1000, 800)
        })
    { }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(Color4.CornflowerBlue);
        GL.Enable(EnableCap.DepthTest);

        // ===== Shaders básicos =====
        string vsrc = @"
            #version 330 core
            layout(location = 0) in vec3 aPos;
            uniform mat4 model, view, projection;
            void main(){
                gl_Position = projection * view * model * vec4(aPos, 1.0);
            }";
        string fsrc = @"
            #version 330 core
            out vec4 FragColor;
            uniform vec4 uColor;
            void main(){
                FragColor = uColor;
            }";

        int vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, vsrc); GL.CompileShader(vs);

        int fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, fsrc); GL.CompileShader(fs);

        _program = GL.CreateProgram();
        GL.AttachShader(_program, vs);
        GL.AttachShader(_program, fs);
        GL.LinkProgram(_program);
        GL.DeleteShader(vs); GL.DeleteShader(fs);

        // VAO/VBO dinámico (subimos polígonos triangulados por draw)
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.BindVertexArray(0);

        _proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);
        CursorState = CursorState.Grabbed;

        // ===== Construir objetos en el Game =====
        ConstruirPC();
    }

    private void ConstruirPC()
    {
        // MONITOR (objeto)
        var monitor = new Objeto("Monitor") { Posicion = new Vector3(0f, 3.0f, -1.5f) };

        // Parte: marco (caja)
        monitor.Partes.Add(CrearParteCaja(
            size: new Vector3(3.8f, 2.5f, 0.3f),
            color: new Vector4(0.2f, 0.2f, 0.2f, 1f),
            posicionLocal: Vector3.Zero
        ));

        // Parte: pantalla
        monitor.Partes.Add(CrearParteCaja(
            size: new Vector3(3.4f, 2.0f, 0.1f),
            color: new Vector4(0f, 0f, 0f, 1f),
            posicionLocal: new Vector3(0, 0, +0.11f)
        ));

        // Parte: soporte vertical
        monitor.Partes.Add(CrearParteCaja(
            size: new Vector3(0.25f, 1.2f, 0.25f),
            color: new Vector4(0.3f, 0.3f, 0.3f, 1f),
            posicionLocal: new Vector3(0, -1.7f, 0)
        ));

        // Parte: base
        monitor.Partes.Add(CrearParteCaja(
            size: new Vector3(1.5f, 0.2f, 1.0f),
            color: new Vector4(0.2f, 0.2f, 0.2f, 1f),
            posicionLocal: new Vector3(0, -2.4f, 0)
        ));

        _objetos.Add(monitor);

        // TECLADO (objeto)
        var teclado = new Objeto("Teclado") { Posicion = new Vector3(0f, 0.5f, 2.5f) };
        // base
        teclado.Partes.Add(CrearParteCaja(
            new Vector3(3.0f, 0.2f, 1.2f),
            new Vector4(0.1f, 0.1f, 0.1f, 1f),
            Vector3.Zero
        ));
        // bloque teclas
        teclado.Partes.Add(CrearParteCaja(
            new Vector3(2.6f, 0.1f, 1.0f),
            new Vector4(0.6f, 0.6f, 0.6f, 1f),
            new Vector3(0, +0.15f, 0)
        ));
        _objetos.Add(teclado);

        // MOUSE (objeto)
        var mouse = new Objeto("Mouse") { Posicion = new Vector3(2.4f, 0.5f, 2.5f) };
        mouse.Partes.Add(CrearParteCaja(
            new Vector3(0.5f, 0.2f, 0.9f),
            new Vector4(0.25f, 0.25f, 0.25f, 1f),
            Vector3.Zero
        ));
        mouse.Partes.Add(CrearParteCaja(
            new Vector3(0.1f, 0.05f, 0.3f),
            new Vector4(0.1f, 0.1f, 0.1f, 1f),
            new Vector3(0, +0.15f, 0)
        ));
        _objetos.Add(mouse);

        // CASE (objeto) – frente a Z+
        var casePc = new Objeto("Case") { Posicion = new Vector3(-3f, 1.5f, -1.5f) };
        casePc.Partes.Add(CrearParteCaja(
            new Vector3(1.8f, 3.6f, 2.2f),
            new Vector4(0.10f, 0.10f, 0.10f, 1f),
            Vector3.Zero
        ));
        // ventana lateral (Z+)
        casePc.Partes.Add(CrearParteCaja(
            new Vector3(1.4f, 2.6f, 0.06f),
            new Vector4(0.30f, 0.30f, 0.30f, 1f),
            new Vector3(+0.1f, 0f, +1.1f - 0.05f)
        ));
        // bisel frontal (Z+)
        casePc.Partes.Add(CrearParteCaja(
            new Vector3(1.84f, 3.6f, 0.08f),
            new Vector4(0.18f, 0.18f, 0.18f, 1f),
            new Vector3(0f, 0f, +1.1f + 0.04f)
        ));
        _objetos.Add(casePc);

        // (Opcional) Podés imprimir centros de masa para mostrar al profe:
        // foreach (var obj in _objetos)
        //     Console.WriteLine($"{obj.Nombre} - Centro de masa: {obj.CentroDeMasa()}");
    }

    // ====== Helpers PRIVADOS dentro del Game (no son clases nuevas) ======
    // Crea una PARTE tipo “caja” con 6 polígonos (cada cara = cuadrilátero de 4 puntos),
    // posicionado a 'posicionLocal' respecto al Objeto que la contiene.
    private Parte CrearParteCaja(Vector3 size, Vector4 color, Vector3 posicionLocal)
    {
        var parte = new Parte { Color = color, Posicion = posicionLocal };

        float hx = size.X * 0.5f, hy = size.Y * 0.5f, hz = size.Z * 0.5f;

        // Z+
        parte.Poligonos.Add(Quad(
            new Vector3(-hx, -hy, hz),
            new Vector3(hx, -hy, hz),
            new Vector3(hx, hy, hz),
            new Vector3(-hx, hy, hz)
        ));
        // Z-
        parte.Poligonos.Add(Quad(
            new Vector3(hx, -hy, -hz),
            new Vector3(-hx, -hy, -hz),
            new Vector3(-hx, hy, -hz),
            new Vector3(hx, hy, -hz)
        ));
        // X+
        parte.Poligonos.Add(Quad(
            new Vector3(hx, -hy, hz),
            new Vector3(hx, -hy, -hz),
            new Vector3(hx, hy, -hz),
            new Vector3(hx, hy, hz)
        ));
        // X-
        parte.Poligonos.Add(Quad(
            new Vector3(-hx, -hy, -hz),
            new Vector3(-hx, -hy, hz),
            new Vector3(-hx, hy, hz),
            new Vector3(-hx, hy, -hz)
        ));
        // Y+
        parte.Poligonos.Add(Quad(
            new Vector3(-hx, hy, hz),
            new Vector3(hx, hy, hz),
            new Vector3(hx, hy, -hz),
            new Vector3(-hx, hy, -hz)
        ));
        // Y-
        parte.Poligonos.Add(Quad(
            new Vector3(-hx, -hy, -hz),
            new Vector3(hx, -hy, -hz),
            new Vector3(hx, -hy, hz),
            new Vector3(-hx, -hy, hz)
        ));

        return parte;
    }

    private Poligono Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var poly = new Poligono();
        poly.Puntos.Add(new Punto(a));
        poly.Puntos.Add(new Punto(b));
        poly.Puntos.Add(new Punto(c));
        poly.Puntos.Add(new Punto(d));
        return poly;
    }

    // ====== Input / cámara ======
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        float speed = 5f * (float)args.Time;
        var k = KeyboardState;

        if (k.IsKeyDown(Keys.W)) _camPos += speed * _camFront;
        if (k.IsKeyDown(Keys.S)) _camPos -= speed * _camFront;
        if (k.IsKeyDown(Keys.A)) _camPos -= Vector3.Normalize(Vector3.Cross(_camFront, _camUp)) * speed;
        if (k.IsKeyDown(Keys.D)) _camPos += Vector3.Normalize(Vector3.Cross(_camFront, _camUp)) * speed;
        if (k.IsKeyDown(Keys.Q)) _camPos.Y -= speed;
        if (k.IsKeyDown(Keys.E)) _camPos.Y += speed;

        _view = Matrix4.LookAt(_camPos, _camPos + _camFront, _camUp);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        if (_firstMouse) { _lastX = e.X; _lastY = e.Y; _firstMouse = false; return; }

        float xoff = (e.X - _lastX) * 0.2f;
        float yoff = (_lastY - e.Y) * 0.2f;
        _lastX = e.X; _lastY = e.Y;

        _yaw += xoff;
        _pitch += yoff;
        _pitch = Math.Clamp(_pitch, -89f, 89f);

        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        _camFront = Vector3.Normalize(front);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(_program);
        int uModel = GL.GetUniformLocation(_program, "model");
        int uView = GL.GetUniformLocation(_program, "view");
        int uProj = GL.GetUniformLocation(_program, "projection");
        int uColor = GL.GetUniformLocation(_program, "uColor");

        GL.UniformMatrix4(uView, false, ref _view);
        GL.UniformMatrix4(uProj, false, ref _proj);

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        foreach (var obj in _objetos)
        {
            // Model base del Objeto (solo translación global)
            Matrix4 modelObj = Matrix4.CreateTranslation(obj.Posicion);

            foreach (var parte in obj.Partes)
            {
                // Model = parte local * objeto global
                Matrix4 model = Matrix4.CreateTranslation(parte.Posicion) * modelObj;
                GL.UniformMatrix4(uModel, false, ref model);
                GL.Uniform4(uColor, parte.Color);

                foreach (var poly in parte.Poligonos)
                {
                    if (poly.Puntos.Count < 3) continue;

                    // Triangulación tipo triangle fan (v0, v(i), v(i+1))
                    var v0 = poly.Puntos[0].Pos;
                    List<float> data = new List<float>();

                    for (int i = 1; i < poly.Puntos.Count - 1; i++)
                    {
                        var v1 = poly.Puntos[i].Pos;
                        var v2 = poly.Puntos[i + 1].Pos;
                        data.AddRange(new float[] { v0.X, v0.Y, v0.Z, v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z });
                    }

                    GL.BufferData(BufferTarget.ArrayBuffer, data.Count * sizeof(float), data.ToArray(), BufferUsageHint.DynamicDraw);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, data.Count / 3);
                }
            }
        }

        SwapBuffers();
    }
}
