using System.Collections.Generic;
using OpenTK.Mathematics;

public class Objeto
{
    public string Nombre;
    public List<Parte> Partes = new List<Parte>();

    // Posición global del objeto (para ubicar todo el conjunto)
    public Vector3 Posicion = Vector3.Zero;

    public Objeto(string nombre) { Nombre = nombre; }

    // Centro de masa del Objeto = promedio de centros de sus Partes (locales) + su Posición global
    public Vector3 CentroDeMasa()
    {
        if (Partes.Count == 0) return Posicion;
        Vector3 acc = Vector3.Zero;
        foreach (var parte in Partes) acc += (parte.CentroDeMasa() + parte.Posicion);
        acc /= Partes.Count;
        return acc + Posicion;
    }
}
