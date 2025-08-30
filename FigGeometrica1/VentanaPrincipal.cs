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

    // Escena (jerarquía que pide tu profe)
    private readonly List<Objeto> _objetos = new();

    public VentanaPrincipal()
        : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            Title = "Objeto → Parte → Cara → Vértice → Punto",
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

        // Un VAO/VBO para subir triángulos por cara
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

        // ======== CONSTRUIR LA PC EN EL GAME (jerarquía) ========
        ConstruirEscenaPC();
    }

    private void ConstruirEscenaPC()
    {
        // MONITOR (marco + pantalla + soporte + base)
        var monitor = new Objeto("Monitor") { Transform = Matrix4.CreateTranslation(0f, 3.2f, -1.5f) };
        monitor.Partes.Add(CrearParteCaja(new Vector3(3.8f, 2.5f, 0.3f), new Vector4(0.2f, 0.2f, 0.2f, 1f)));              // marco
        monitor.Partes.Add(CrearParteCajaEnOffset(new Vector3(3.4f, 2.0f, 0.1f), new Vector3(0, 0, +0.11f), new Vector4(0, 0, 0, 1f))); // pantalla
        monitor.Partes.Add(CrearParteCajaEnOffset(new Vector3(0.25f, 1.2f, 0.25f), new Vector3(0, -1.8f, 0), new Vector4(0.3f, 0.3f, 0.3f, 1f))); // soporte
        monitor.Partes.Add(CrearParteCajaEnOffset(new Vector3(1.5f, 0.2f, 1.0f), new Vector3(0, -2.5f, 0), new Vector4(0.2f, 0.2f, 0.2f, 1f)));   // base
        _objetos.Add(monitor);

        // TECLADO (base + “zona de teclas” como bloque simple)
        var teclado = new Objeto("Teclado") { Transform = Matrix4.CreateTranslation(0f, 0.5f, 2.5f) };
        teclado.Partes.Add(CrearParteCaja(new Vector3(3.0f, 0.2f, 1.2f), new Vector4(0.1f, 0.1f, 0.1f, 1f)));                          // base
        teclado.Partes.Add(CrearParteCajaEnOffset(new Vector3(2.6f, 0.1f, 1.0f), new Vector3(0, +0.15f, 0), new Vector4(0.6f, 0.6f, 0.6f, 1f))); // teclas simplificadas
        _objetos.Add(teclado);

        // MOUSE (cuerpo + “scroll”)
        var mouse = new Objeto("Mouse") { Transform = Matrix4.CreateTranslation(2.5f, 0.5f, 2.5f) };
        mouse.Partes.Add(CrearParteCaja(new Vector3(0.5f, 0.2f, 0.9f), new Vector4(0.25f, 0.25f, 0.25f, 1f)));                          // cuerpo
        mouse.Partes.Add(CrearParteCajaEnOffset(new Vector3(0.1f, 0.05f, 0.3f), new Vector3(0, +0.15f, 0), new Vector4(0.1f, 0.1f, 0.1f, 1f)));   // scroll
        _objetos.Add(mouse);

        // CASE (frente a Z+, ventana lateral, bisel)
        var casePc = new Objeto("Case") { Transform = Matrix4.CreateTranslation(-3f, 1.5f, -1.5f) };
        casePc.Partes.Add(CrearParteCaja(new Vector3(1.8f, 3.6f, 2.2f), new Vector4(0.10f, 0.10f, 0.10f, 1f)));                                  // chasis
        casePc.Partes.Add(CrearParteCajaEnOffset(new Vector3(1.4f, 2.6f, 0.06f), new Vector3(+0.1f, 0f, +1.1f - 0.05f), new Vector4(0.30f, 0.30f, 0.30f, 1f))); // ventana
        casePc.Partes.Add(CrearParteCajaEnOffset(new Vector3(1.84f, 3.6f, 0.08f), new Vector3(0f, 0f, +1.1f + 0.04f), new Vector4(0.18f, 0.18f, 0.18f, 1f)));   // bisel frontal
        _objetos.Add(casePc);
    }

    // ==== Helpers PRIVADOS dentro del Game (no son nuevas clases) ====
    private Parte CrearParteCaja(Vector3 size, Vector4 color)
        => CrearParteCajaEnOffset(size, Vector3.Zero, color);

    private Parte CrearParteCajaEnOffset(Vector3 size, Vector3 offset, Vector4 color)
    {
        var parte = new Parte { Color = color };

        float hx = size.X * 0.5f, hy = size.Y * 0.5f, hz = size.Z * 0.5f;

        // Cada cara con 4 vértices (quad) — orden para triangle fan
        // Z+
        parte.Caras.Add(CaraQuad(
            new Vector3(-hx, -hy, hz) + offset,
            new Vector3(hx, -hy, hz) + offset,
            new Vector3(hx, hy, hz) + offset,
            new Vector3(-hx, hy, hz) + offset
        ));
        // Z-
        parte.Caras.Add(CaraQuad(
            new Vector3(hx, -hy, -hz) + offset,
            new Vector3(-hx, -hy, -hz) + offset,
            new Vector3(-hx, hy, -hz) + offset,
            new Vector3(hx, hy, -hz) + offset
        ));
        // X+
        parte.Caras.Add(CaraQuad(
            new Vector3(hx, -hy, hz) + offset,
            new Vector3(hx, -hy, -hz) + offset,
            new Vector3(hx, hy, -hz) + offset,
            new Vector3(hx, hy, hz) + offset
        ));
        // X-
        parte.Caras.Add(CaraQuad(
            new Vector3(-hx, -hy, -hz) + offset,
            new Vector3(-hx, -hy, hz) + offset,
            new Vector3(-hx, hy, hz) + offset,
            new Vector3(-hx, hy, -hz) + offset
        ));
        // Y+
        parte.Caras.Add(CaraQuad(
            new Vector3(-hx, hy, hz) + offset,
            new Vector3(hx, hy, hz) + offset,
            new Vector3(hx, hy, -hz) + offset,
            new Vector3(-hx, hy, -hz) + offset
        ));
        // Y-
        parte.Caras.Add(CaraQuad(
            new Vector3(-hx, -hy, -hz) + offset,
            new Vector3(hx, -hy, -hz) + offset,
            new Vector3(hx, -hy, hz) + offset,
            new Vector3(-hx, -hy, hz) + offset
        ));

        return parte;
    }

    private Cara CaraQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var cara = new Cara();
        cara.Vertices.Add(new Vertice(new Punto(a)));
        cara.Vertices.Add(new Vertice(new Punto(b)));
        cara.Vertices.Add(new Vertice(new Punto(c)));
        cara.Vertices.Add(new Vertice(new Punto(d)));
        return cara;
    }

    // ========= Input / cámara =========
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
            foreach (var parte in obj.Partes)
            {
                // model = parteLocal * objetoTransform (suficiente para esta práctica)
                Matrix4 model = obj.Transform;
                GL.UniformMatrix4(uModel, false, ref model);
                GL.Uniform4(uColor, parte.Color);

                foreach (var cara in parte.Caras)
                {
                    if (cara.Vertices.Count < 3) continue;

                    var v0 = cara.Vertices[0].P.Pos;
                    List<float> data = new List<float>();

                    for (int i = 1; i < cara.Vertices.Count - 1; i++)
                    {
                        var v1 = cara.Vertices[i].P.Pos;
                        var v2 = cara.Vertices[i + 1].P.Pos;
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
