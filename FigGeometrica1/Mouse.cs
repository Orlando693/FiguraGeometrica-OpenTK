using OpenTK.Mathematics;

public class Mouse
{
    private Vector3 _pos;

    public Mouse(Vector3 posicion)
    {
        _pos = posicion;
    }

    public void Draw(int shaderProgram, Matrix4 view, Matrix4 projection)
    {
        // Cuerpo principal (forma básica del mouse)
        Matrix4 cuerpo = Matrix4.CreateScale(0.5f, 0.2f, 0.9f)
                       * Matrix4.CreateTranslation(_pos);
        CubeMesh.DrawCube(shaderProgram, cuerpo, view, projection, new Vector4(0.25f, 0.25f, 0.25f, 1f));

        // Botón/scroll (algo chiquito arriba al centro)
        Matrix4 boton = Matrix4.CreateScale(0.1f, 0.05f, 0.3f)
                      * Matrix4.CreateTranslation(_pos + new Vector3(0f, 0.15f, 0f));
        CubeMesh.DrawCube(shaderProgram, boton, view, projection, new Vector4(0.1f, 0.1f, 0.1f, 1f));
    }
}
