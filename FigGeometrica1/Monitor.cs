using OpenTK.Mathematics;

public class Monitor
{
    private Vector3 _pos;

    public Monitor(Vector3 posicion)
    {
        _pos = posicion;
    }

    public void Draw(int shaderProgram, Matrix4 view, Matrix4 projection)
    {
        // --- Marco del monitor (gris oscuro) ---
        Matrix4 marco = Matrix4.CreateScale(3.8f, 2.5f, 0.3f)
                      * Matrix4.CreateTranslation(_pos);
        CubeMesh.DrawCube(shaderProgram, marco, view, projection, new Vector4(0.2f, 0.2f, 0.2f, 1f));

        // --- Pantalla interna (negra) ---
        Matrix4 pantalla = Matrix4.CreateScale(3.4f, 2.0f, 0.1f)
                         * Matrix4.CreateTranslation(_pos + new Vector3(0, 0, 0.11f));
        CubeMesh.DrawCube(shaderProgram, pantalla, view, projection, new Vector4(0.0f, 0.0f, 0.0f, 1f));

        // --- Soporte vertical (gris medio) ---
        Matrix4 soporte = Matrix4.CreateScale(0.25f, 1.2f, 0.25f)
                        * Matrix4.CreateTranslation(_pos + new Vector3(0, -1.8f, 0));
        CubeMesh.DrawCube(shaderProgram, soporte, view, projection, new Vector4(0.3f, 0.3f, 0.3f, 1f));

        // --- Base del soporte (gris oscuro, ancha) ---
        Matrix4 baseSoporte = Matrix4.CreateScale(1.5f, 0.2f, 1.0f)
                            * Matrix4.CreateTranslation(_pos + new Vector3(0, -2.5f, 0));
        CubeMesh.DrawCube(shaderProgram, baseSoporte, view, projection, new Vector4(0.2f, 0.2f, 0.2f, 1f));
    }
}
