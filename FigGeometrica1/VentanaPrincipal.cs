using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class VentanaPrincipal : GameWindow
{
    private Renderizador _renderizador;
    private EscenaPC _escena;
    private Matrix4 _view;
    private Matrix4 _projection;

    private Vector3 _cameraPos = new Vector3(6, 6, 10);
    private Vector3 _cameraFront = -Vector3.UnitZ; // mira hacia adelante
    private Vector3 _cameraUp = Vector3.UnitY;

    private float _yaw = -90f;   // rotación horizontal
    private float _pitch = 0f;   // rotación vertical
    private float _lastX, _lastY;
    private bool _firstMove = true;

    public VentanaPrincipal()
        : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Title = "PC en 3D con Cámara Controlada",
            Size = new Vector2i(1000, 800)
        })
    { }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.CornflowerBlue);
        GL.Enable(EnableCap.DepthTest);

        _renderizador = new Renderizador();
        _renderizador.Inicializar();

        _escena = new EscenaPC();

        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            Size.X / (float)Size.Y,
            0.1f,
            100f
        );

        // Capturar el mouse dentro de la ventana
        CursorState = CursorState.Grabbed;
    }


    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var input = KeyboardState;
        float cameraSpeed = 5f * (float)args.Time; // velocidad de movimiento

        // Movimiento de cámara con WASD + QE
        if (input.IsKeyDown(Keys.W)) _cameraPos += cameraSpeed * _cameraFront;
        if (input.IsKeyDown(Keys.S)) _cameraPos -= cameraSpeed * _cameraFront;
        if (input.IsKeyDown(Keys.A)) _cameraPos -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * cameraSpeed;
        if (input.IsKeyDown(Keys.D)) _cameraPos += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * cameraSpeed;
        if (input.IsKeyDown(Keys.Q)) _cameraPos.Y -= cameraSpeed;
        if (input.IsKeyDown(Keys.E)) _cameraPos.Y += cameraSpeed;

        // Actualizar matriz de vista
        _view = Matrix4.LookAt(_cameraPos, _cameraPos + _cameraFront, _cameraUp);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        if (_firstMove)
        {
            _lastX = e.X;
            _lastY = e.Y;
            _firstMove = false;
        }
        else
        {
            float xOffset = e.X - _lastX;
            float yOffset = _lastY - e.Y; // invertido para naturalidad
            _lastX = e.X;
            _lastY = e.Y;

            float sensitivity = 0.2f;
            xOffset *= sensitivity;
            yOffset *= sensitivity;

            _yaw += xOffset;
            _pitch += yOffset;

            // Limitar pitch para no voltear demasiado
            if (_pitch > 89f) _pitch = 89f;
            if (_pitch < -89f) _pitch = -89f;

            // Recalcular vector dirección
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            _cameraFront = Vector3.Normalize(front);
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _escena.Draw(_renderizador.ObtenerShader(), _view, _projection);

        SwapBuffers();
    }
}
