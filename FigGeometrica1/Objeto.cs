using System.Collections.Generic;
using OpenTK.Mathematics;

public class Objeto
{
    public string Nombre;
    public List<Parte> Partes = new List<Parte>();
    public Matrix4 Transform = Matrix4.Identity; // posición/orientación del objeto

    public Objeto(string nombre) { Nombre = nombre; }
}
