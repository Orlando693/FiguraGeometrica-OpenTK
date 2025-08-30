using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Linq;

public class Poligono
{
    public List<Punto> Puntos = new List<Punto>();

    // Centro de masa simple: promedio de todos los puntos del polígono
    public Vector3 CentroDeMasa()
    {
        if (Puntos.Count == 0) return Vector3.Zero;
        Vector3 acc = Vector3.Zero;
        foreach (var p in Puntos) acc += p.Pos;
        return acc / Puntos.Count;
    }
}
