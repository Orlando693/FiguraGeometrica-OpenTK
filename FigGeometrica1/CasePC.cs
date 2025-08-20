using OpenTK.Mathematics;

public class CasePC
{
    private Vector3 _pos; // centro del gabinete

    public CasePC(Vector3 posicion)
    {
        _pos = posicion;
    }

    public void Draw(int shaderProgram, Matrix4 view, Matrix4 projection)
    {
        // --- Parámetros base del chasis ---
        // Tamaño general del gabinete (ancho X, alto Y, fondo Z)
        Vector3 chasisSize = new Vector3(1.8f, 3.6f, 2.2f);
        Vector4 negroMate = new Vector4(0.10f, 0.10f, 0.10f, 1f);
        Vector4 grisOscuro = new Vector4(0.18f, 0.18f, 0.18f, 1f);
        Vector4 grisMedio = new Vector4(0.30f, 0.30f, 0.30f, 1f);
        Vector4 acentoAzul = new Vector4(0.10f, 0.35f, 0.80f, 1f);
        Vector4 acentoVerde = new Vector4(0.10f, 0.8f, 0.25f, 1f);
        Vector4 acentoRojo = new Vector4(0.85f, 0.15f, 0.15f, 1f);

        // Medias dimensiones (para ubicar detalles en bordes)
        float hx = chasisSize.X * 0.5f; // 0.9
        float hy = chasisSize.Y * 0.5f; // 1.8
        float hz = chasisSize.Z * 0.5f; // 1.1

        // ---------- 1) Chasis principal ----------
        Matrix4 mChasis = Matrix4.CreateScale(chasisSize) * Matrix4.CreateTranslation(_pos);
        CubeMesh.DrawCube(shaderProgram, mChasis, view, projection, negroMate);

        // ---------- 2) Ventana lateral (lado derecho) ----------
        // Una “ventana” plana a la derecha (Z positivo)
        Vector3 winSize = new Vector3(1.4f, 2.6f, 0.06f);
        Vector3 winPos = _pos + new Vector3(0.1f, 0.0f, hz - 0.05f);
        Matrix4 mWin = Matrix4.CreateScale(winSize) * Matrix4.CreateTranslation(winPos);
        CubeMesh.DrawCube(shaderProgram, mWin, view, projection, grisMedio);

        // ---------- 3) Bisel frontal (frente hacia Z positivo) ----------
        Vector3 frenteSize = new Vector3(chasisSize.X + 0.04f, chasisSize.Y, 0.08f);
        Vector3 frentePos = _pos + new Vector3(0f, 0f, hz + frenteSize.Z * 0.5f - 0.02f);
        Matrix4 mFrente = Matrix4.CreateScale(frenteSize) * Matrix4.CreateTranslation(frentePos);
        CubeMesh.DrawCube(shaderProgram, mFrente, view, projection, grisOscuro);

        // ---------- 4) Tira vertical de acento en el frente ----------
        Vector3 tiraSize = new Vector3(0.25f, chasisSize.Y * 0.9f, 0.09f);
        Vector3 tiraPos = frentePos + new Vector3(hx - (tiraSize.X * 0.6f), 0f, 0.0f);
        Matrix4 mTira = Matrix4.CreateScale(tiraSize) * Matrix4.CreateTranslation(tiraPos);
        CubeMesh.DrawCube(shaderProgram, mTira, view, projection, grisMedio);

        // ---------- 5) Botón de encendido (power) ----------
        Vector3 powerSize = new Vector3(0.20f, 0.20f, 0.10f);
        Vector3 powerPos = frentePos + new Vector3(hx - 0.35f, 0.9f, 0.0f);
        Matrix4 mPower = Matrix4.CreateScale(powerSize) * Matrix4.CreateTranslation(powerPos);
        CubeMesh.DrawCube(shaderProgram, mPower, view, projection, grisMedio);

        // LEDs indicadores cerca del botón
        Vector3 ledSize = new Vector3(0.06f, 0.06f, 0.10f);
        Matrix4 mLed1 = Matrix4.CreateScale(ledSize) * Matrix4.CreateTranslation(frentePos + new Vector3(hx - 0.35f, 1.25f, 0f));
        Matrix4 mLed2 = Matrix4.CreateScale(ledSize) * Matrix4.CreateTranslation(frentePos + new Vector3(hx - 0.20f, 1.25f, 0f));
        CubeMesh.DrawCube(shaderProgram, mLed1, view, projection, acentoVerde);
        CubeMesh.DrawCube(shaderProgram, mLed2, view, projection, acentoRojo);

        // ---------- 6) Puertos USB (dos rectángulos azules) ----------
        Vector3 usbSize = new Vector3(0.30f, 0.10f, 0.10f);
        Matrix4 mUsb1 = Matrix4.CreateScale(usbSize) * Matrix4.CreateTranslation(frentePos + new Vector3(hx - 0.35f, 0.45f, 0f));
        Matrix4 mUsb2 = Matrix4.CreateScale(usbSize) * Matrix4.CreateTranslation(frentePos + new Vector3(hx - 0.35f, 0.20f, 0f));
        CubeMesh.DrawCube(shaderProgram, mUsb1, view, projection, acentoAzul);
        CubeMesh.DrawCube(shaderProgram, mUsb2, view, projection, acentoAzul);


        // ---------- 7) Rejilla superior (como una tapa delgada) ----------
        Vector3 rejillaSize = new Vector3(1.4f, 0.06f, 1.0f);
        Vector3 rejillaPos = _pos + new Vector3(0f, hy - rejillaSize.Y * 0.5f, 0.2f);
        Matrix4 mTop = Matrix4.CreateScale(rejillaSize) * Matrix4.CreateTranslation(rejillaPos);
        CubeMesh.DrawCube(shaderProgram, mTop, view, projection, grisMedio);

        // ---------- 8) Patitas inferiores ----------
        Vector3 pieSize = new Vector3(0.25f, 0.12f, 0.6f);
        float yPie = -hy - pieSize.Y * 0.5f + 0.04f;  // apenas por debajo
        Matrix4 mPieFL = Matrix4.CreateScale(pieSize) * Matrix4.CreateTranslation(_pos + new Vector3(-hx + 0.30f, yPie, hz - 0.35f));
        Matrix4 mPieFR = Matrix4.CreateScale(pieSize) * Matrix4.CreateTranslation(_pos + new Vector3(hx - 0.30f, yPie, hz - 0.35f));
        Matrix4 mPieBL = Matrix4.CreateScale(pieSize) * Matrix4.CreateTranslation(_pos + new Vector3(-hx + 0.30f, yPie, -hz + 0.35f));
        Matrix4 mPieBR = Matrix4.CreateScale(pieSize) * Matrix4.CreateTranslation(_pos + new Vector3(hx - 0.30f, yPie, -hz + 0.35f));
        CubeMesh.DrawCube(shaderProgram, mPieFL, view, projection, grisOscuro);
        CubeMesh.DrawCube(shaderProgram, mPieFR, view, projection, grisOscuro);
        CubeMesh.DrawCube(shaderProgram, mPieBL, view, projection, grisOscuro);
        CubeMesh.DrawCube(shaderProgram, mPieBR, view, projection, grisOscuro);
    }
}
