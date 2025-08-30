using OpenTK.Mathematics;

public struct Punto
{
    public Vector3 Pos;
    public Punto(float x, float y, float z) => Pos = new Vector3(x, y, z);
    public Punto(Vector3 p) => Pos = p;
}
