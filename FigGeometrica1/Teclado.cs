using OpenTK.Mathematics;

public class Teclado
{
    private Vector3 _posicion;

    public Teclado(Vector3 posicion)
    {
        _posicion = posicion;
    }

    public void Draw(int shaderProgram, Matrix4 view, Matrix4 projection)
    {
        Matrix4 model = Matrix4.CreateScale(3.0f, 0.2f, 1.0f) * Matrix4.CreateTranslation(_posicion);
        CubeMesh.DrawCube(shaderProgram, model, view, projection, new Vector4(0.15f, 0.15f, 0.15f, 1f));
    }
}
