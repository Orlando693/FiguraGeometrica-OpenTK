using OpenTK.Mathematics;

public class EscenaPC
{
    public Monitor monitor;
    public Teclado teclado;
    public Mouse mouse;
    public CasePC casePc;

    public EscenaPC()
    {
        // Ajusté las posiciones para que no choquen
        monitor = new Monitor(new Vector3(1, 2.5f, -1));    // más arriba y atrás
        teclado = new Teclado(new Vector3(1, 0.5f, 2.5f));     // centrado adelante
        mouse = new Mouse(new Vector3(3.5f, 0.5f, 2.5f));     // a la derecha del teclado
        casePc = new CasePC(new Vector3(-3f, 1.5f, -1.5f));    // a la izquierda y abajo
    }

    public void Draw(int shaderProgram, Matrix4 view, Matrix4 projection)
    {
        monitor.Draw(shaderProgram, view, projection);
        teclado.Draw(shaderProgram, view, projection);
        mouse.Draw(shaderProgram, view, projection);
        casePc.Draw(shaderProgram, view, projection);
    }
}
