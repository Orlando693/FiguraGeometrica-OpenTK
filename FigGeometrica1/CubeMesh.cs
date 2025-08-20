using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class CubeMesh
{
    private static int _vao;
    private static int _vbo;
    private static int _ebo;

    private static readonly float[] _vertices =
    {
        // posiciones
        -0.5f, -0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f, -0.5f,  0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f
    };

    private static readonly uint[] _indices =
    {
        0, 1, 2, 2, 3, 0, // atrás
        4, 5, 6, 6, 7, 4, // frente
        0, 4, 7, 7, 3, 0, // izquierda
        1, 5, 6, 6, 2, 1, // derecha
        3, 2, 6, 6, 7, 3, // arriba
        0, 1, 5, 5, 4, 0  // abajo
    };

    public static void Init()
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    public static void DrawCube(int shaderProgram, Matrix4 model, Matrix4 view, Matrix4 projection, Vector4 color)
    {
        GL.UseProgram(shaderProgram);

        int modelLoc = GL.GetUniformLocation(shaderProgram, "model");
        int viewLoc = GL.GetUniformLocation(shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(shaderProgram, "projection");
        int colorLoc = GL.GetUniformLocation(shaderProgram, "ourColor");

        GL.UniformMatrix4(modelLoc, false, ref model);
        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(projLoc, false, ref projection);
        GL.Uniform4(colorLoc, color);

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }
}
