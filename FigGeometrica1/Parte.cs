using System.Collections.Generic;
using OpenTK.Mathematics;

public class Parte
{
    public List<Poligono> Poligonos = new List<Poligono>();
    public Vector4 Color = new Vector4(0.7f, 0.7f, 0.7f, 1f);

    // Posición local de la parte respecto al Objeto (para colocarla en el mundo)
    public Vector3 Posicion = Vector3.Zero;

    // Opcional: escala local si querés usarla
    public Vector3 Escala = Vector3.One;

    // Centro de masa de la Parte = promedio de centros de sus polígonos (en coords locales)
    public Vector3 CentroDeMasa()
    {
        if (Poligonos.Count == 0) return Vector3.Zero;
        Vector3 acc = Vector3.Zero;
        foreach (var poly in Poligonos) acc += poly.CentroDeMasa();
        return acc / Poligonos.Count;
    }
}
