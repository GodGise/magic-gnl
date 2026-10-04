using System.Collections;
using UnityEngine;

// Trappola a spuntoni nascosta nel terreno.
// A cosa serve: in gioco non si vede. Quando il giocatore ci passa sopra parte un breve ritardo
// (le punte spuntano appena, come avviso per chi è attento), poi gli spuntoni escono di colpo e
// colpiscono chi è ancora sopra. Il danno non si può parare, ma una schivata fatta al momento giusto
// lo evita. Poi gli spuntoni rientrano e, dopo una pausa, la trappola si riarma.
// Come montarlo:
//   1. GameObject > Create Empty, e mettilo a filo del terreno, dove vuoi la trappola.
//      Non cambiare la sua Scale: la grandezza si regola con "Dimensioni".
//   2. Nell'Inspector clicca Add Component e scegli Trappola Spuntoni.
//   3. Nella vista Scene un riquadro rosso mostra la zona che fa scattare la trappola e
//      quanto escono gli spuntoni. In gioco il riquadro non si vede.
public class TrappolaSpuntoni : MonoBehaviour
{
    [Header("Zona")]
    [Tooltip("Larghezza (X) e lunghezza (Z) della zona che fa scattare la trappola, in metri.")]
    [SerializeField] Vector2 dimensioni = new Vector2(2f, 2f);

    [Header("Tempi")]
    [Tooltip("Secondi tra quando il giocatore ci passa sopra e quando escono gli spuntoni.")]
    [SerializeField] float ritardo = 0.5f;
    [Tooltip("Secondi che impiegano gli spuntoni a uscire (pochissimi: è uno scatto).")]
    [SerializeField] float tempoSalita = 0.08f;
    [Tooltip("Per quanti secondi gli spuntoni restano fuori.")]
    [SerializeField] float tempoFuori = 1f;
    [SerializeField] float tempoDiscesa = 0.5f;
    [Tooltip("Secondi dopo la discesa prima che la trappola possa scattare di nuovo.")]
    [SerializeField] float pausaRiarmo = 2f;

    [Header("Danno e aspetto")]
    [SerializeField] float danno = 35f;
    [SerializeField] float altezzaSpuntoni = 0.7f;
    [Tooltip("Distanza tra uno spuntone e l'altro.")]
    [SerializeField] float spaziatura = 0.45f;
    [Tooltip("Se attivo, durante il ritardo le punte escono di pochi centimetri: un piccolo avviso per chi guarda per terra.")]
    [SerializeField] bool avvisoPunte = true;

    GiocatoreControllo giocatore;
    Transform spuntoni;
    bool inAzione;

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        CreaSpuntoni();
        PosizionaSpuntoni(0f);
        spuntoni.gameObject.SetActive(false); // nascosta finché non scatta
    }

    void Update()
    {
        if (inAzione || giocatore == null) return;
        if (giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return;
        if (GiocatoreSopra()) StartCoroutine(Scatta());
    }

    IEnumerator Scatta()
    {
        inAzione = true;
        spuntoni.gameObject.SetActive(true);

        // 1. Il giocatore ha messo il piede sulla trappola: breve attesa, con le punte appena visibili.
        float inizio = avvisoPunte ? 0.12f : 0f;
        PosizionaSpuntoni(inizio);
        yield return new WaitForSeconds(ritardo);

        // 2. Gli spuntoni escono di scatto.
        for (float t = 0f; t < tempoSalita; t += Time.deltaTime)
        {
            PosizionaSpuntoni(Mathf.Lerp(inizio, 1f, t / tempoSalita));
            yield return null;
        }
        PosizionaSpuntoni(1f);

        // 3. Restano fuori: chi è sopra viene colpito, una volta sola per ogni scatto.
        bool colpito = false;
        for (float t = 0f; t < tempoFuori; t += Time.deltaTime)
        {
            if (!colpito && GiocatoreSopra())
            {
                giocatore.RiceviDannoAmbiente(danno);
                colpito = true;
            }
            yield return null;
        }

        // 4. Rientrano nel terreno e la trappola torna invisibile.
        for (float t = 0f; t < tempoDiscesa; t += Time.deltaTime)
        {
            PosizionaSpuntoni(Mathf.Lerp(1f, 0f, t / tempoDiscesa));
            yield return null;
        }
        PosizionaSpuntoni(0f);
        spuntoni.gameObject.SetActive(false);

        // 5. Pausa prima di potersi riattivare.
        yield return new WaitForSeconds(pausaRiarmo);
        inAzione = false;
    }

    // Vero se il giocatore è dentro la zona della trappola (con un piccolo margine per lo spessore del corpo).
    bool GiocatoreSopra()
    {
        Vector3 relativa = Quaternion.Inverse(transform.rotation) * (giocatore.transform.position - transform.position);
        const float margine = 0.25f;
        return Mathf.Abs(relativa.x) <= dimensioni.x * 0.5f + margine
            && Mathf.Abs(relativa.z) <= dimensioni.y * 0.5f + margine
            && relativa.y > -0.5f && relativa.y < 2.5f;
    }

    // 0 = punte a filo del terreno (nascoste), 1 = spuntoni tutti fuori.
    void PosizionaSpuntoni(float frazione)
    {
        spuntoni.localPosition = new Vector3(0f, Mathf.Lerp(-altezzaSpuntoni, 0f, frazione), 0f);
    }

    // Crea una griglia di spuntoni a forma di piramide che copre la zona della trappola.
    void CreaSpuntoni()
    {
        spuntoni = new GameObject("Spuntoni").transform;
        spuntoni.SetParent(transform, false);

        Mesh punta = CreaPunta();
        Material ferro = CreaMateriale();
        int colonne = Mathf.Max(1, Mathf.FloorToInt(dimensioni.x / spaziatura));
        int righe = Mathf.Max(1, Mathf.FloorToInt(dimensioni.y / spaziatura));
        float larghezzaBase = spaziatura * 0.6f;

        for (int i = 0; i < colonne; i++)
        {
            for (int j = 0; j < righe; j++)
            {
                float x = (i + 0.5f) / colonne * dimensioni.x - dimensioni.x * 0.5f;
                float z = (j + 0.5f) / righe * dimensioni.y - dimensioni.y * 0.5f;

                var spuntone = new GameObject("Spuntone");
                spuntone.transform.SetParent(spuntoni, false);
                spuntone.transform.localPosition = new Vector3(x, 0f, z);
                spuntone.transform.localScale = new Vector3(larghezzaBase, altezzaSpuntoni, larghezzaBase);
                spuntone.AddComponent<MeshFilter>().sharedMesh = punta;
                spuntone.AddComponent<MeshRenderer>().sharedMaterial = ferro;
            }
        }
    }

    // Piramide alta 1 con base 1 x 1: quattro facce piatte, in stile low-poly. La base resta sotto terra.
    static Mesh CreaPunta()
    {
        Vector3 cima = new Vector3(0f, 1f, 0f);
        Vector3 a = new Vector3(-0.5f, 0f, -0.5f);
        Vector3 b = new Vector3(0.5f, 0f, -0.5f);
        Vector3 c = new Vector3(0.5f, 0f, 0.5f);
        Vector3 d = new Vector3(-0.5f, 0f, 0.5f);

        var mesh = new Mesh { name = "Punta" };
        mesh.vertices = new[] { a, cima, b, b, cima, c, c, cima, d, d, cima, a };
        mesh.triangles = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    // Materiale color ferro vecchio.
    static Material CreaMateriale()
    {
        Material materiale;
        Shader standard = Shader.Find("Standard");
        if (standard != null)
        {
            materiale = new Material(standard);
        }
        else
        {
            // Riserva: copia il materiale di base di Unity da una forma temporanea.
            GameObject provvisorio = GameObject.CreatePrimitive(PrimitiveType.Cube);
            materiale = new Material(provvisorio.GetComponent<Renderer>().sharedMaterial);
            Destroy(provvisorio);
        }
        materiale.color = new Color(0.32f, 0.3f, 0.28f);
        return materiale;
    }

    // Disegni visibili solo nella vista Scene: la zona che fa scattare la trappola e l'altezza degli spuntoni.
    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(1f, 0.15f, 0.1f, 0.35f);
        Gizmos.DrawCube(new Vector3(0f, 0.02f, 0f), new Vector3(dimensioni.x, 0.04f, dimensioni.y));
        Gizmos.color = new Color(1f, 0.15f, 0.1f, 1f);
        Gizmos.DrawWireCube(new Vector3(0f, altezzaSpuntoni * 0.5f, 0f), new Vector3(dimensioni.x, altezzaSpuntoni, dimensioni.y));
    }
}
