using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Baule da aprire con E, con un oggetto dentro.
// A cosa serve: quando il giocatore è vicino compare "E  Apri il baule"; premendo E il coperchio si
// alza, dal baule esce una luce dorata e il giocatore riceve il contenuto (oggi: il bastone magico).
// Si apre una volta sola.
// Come montarlo:
//   1. GameObject > Create Empty, mettilo a terra e giralo (Rotation Y) con il davanti verso chi arriva.
//   2. Add Component > Baule e scegli il Contenuto.
//   L'aspetto del baule (legno, fasce di ferro, coperchio) si crea da solo quando parte il gioco;
//   nella vista Scene è segnato da un riquadro marrone.
public class Baule : MonoBehaviour
{
    public enum Contenuto { BastoneMagico }

    [SerializeField] Contenuto contenuto = Contenuto.BastoneMagico;
    [Tooltip("Distanza (in orizzontale) entro cui si può aprire.")]
    [SerializeField] float raggioInterazione = 2f;
    [Tooltip("Secondi che impiega il coperchio ad aprirsi.")]
    [SerializeField] float durataApertura = 0.7f;

    GiocatoreControllo giocatore;
    InputAction comandoInteragisci;
    Transform coperchio;
    bool aperto;

    void Awake()
    {
        comandoInteragisci = new InputAction("Interagisci", InputActionType.Button);
        comandoInteragisci.AddBinding("<Keyboard>/e");
        comandoInteragisci.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable() => comandoInteragisci.Enable();
    void OnDisable() => comandoInteragisci.Disable();
    void OnDestroy() => comandoInteragisci.Dispose();

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        CreaAspetto();
    }

    void Update()
    {
        if (PuoAprire() && comandoInteragisci.WasPressedThisFrame()) StartCoroutine(Apri());
    }

    bool PuoAprire()
    {
        if (aperto || giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        Vector3 distanza = giocatore.transform.position - transform.position;
        distanza.y = 0f;
        return distanza.magnitude <= raggioInterazione;
    }

    IEnumerator Apri()
    {
        aperto = true;
        Suoni.Suona(Suono.BauleAperto, transform.position + Vector3.up * 0.5f);

        // Luce dorata che esce dal baule mentre si apre, poi si spegne piano.
        Light luce = new GameObject("Luce baule").AddComponent<Light>();
        luce.transform.SetParent(transform, false);
        luce.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        luce.type = LightType.Point;
        luce.color = new Color(1f, 0.8f, 0.4f);
        luce.range = 5f;

        // Il coperchio ruota all'indietro attorno al bordo posteriore.
        for (float t = 0f; t < durataApertura; t += Time.deltaTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, t / durataApertura);
            coperchio.localRotation = Quaternion.Euler(110f * s, 0f, 0f);
            luce.intensity = 3f * s;
            yield return null;
        }
        coperchio.localRotation = Quaternion.Euler(110f, 0f, 0f);

        DaiContenuto();

        for (float t = 0f; t < 1.5f; t += Time.deltaTime)
        {
            luce.intensity = Mathf.Lerp(3f, 0.6f, t / 1.5f);
            yield return null;
        }
    }

    void DaiContenuto()
    {
        switch (contenuto)
        {
            case Contenuto.BastoneMagico:
                giocatore.SbloccaBastone();
                MessaggiSchermo.Mostra("Hai trovato: Bastone magico.  2 = bastone,  1 = spada", 5f);
                break;
        }
    }

    // Baule a blocchi: cassa di legno con fasce di ferro e coperchio incernierato dietro.
    // Il davanti del baule guarda verso -Z (la direzione opposta alla freccia blu dell'oggetto).
    void CreaAspetto()
    {
        Transform aspetto = new GameObject("Aspetto baule").transform;
        aspetto.SetParent(transform, false);

        Material legno = null, ferro = null;
        legno = Parte(aspetto, "Cassa", new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 0.7f), new Color(0.32f, 0.2f, 0.1f), legno, true);
        ferro = Parte(aspetto, "Fascia sinistra", new Vector3(-0.4f, 0.3f, 0f), new Vector3(0.08f, 0.62f, 0.72f), new Color(0.3f, 0.3f, 0.32f), ferro, false);
        Parte(aspetto, "Fascia destra", new Vector3(0.4f, 0.3f, 0f), new Vector3(0.08f, 0.62f, 0.72f), Color.white, ferro, false);

        // Il perno del coperchio sta sul bordo alto posteriore della cassa.
        coperchio = new GameObject("Coperchio").transform;
        coperchio.SetParent(aspetto, false);
        coperchio.localPosition = new Vector3(0f, 0.6f, 0.35f);
        Parte(coperchio, "Tavola coperchio", new Vector3(0f, 0.11f, -0.35f), new Vector3(1.22f, 0.22f, 0.72f), Color.white, legno, false);
        Parte(coperchio, "Fascia coperchio", new Vector3(0f, 0.11f, -0.35f), new Vector3(0.1f, 0.24f, 0.74f), Color.white, ferro, false);
        Parte(coperchio, "Serratura", new Vector3(0f, 0.02f, -0.72f), new Vector3(0.14f, 0.16f, 0.04f), Color.white, ferro, false);
    }

    // Crea un pezzo; se "materiale" è vuoto ne crea uno nuovo del colore dato e lo restituisce, per riusarlo.
    Material Parte(Transform genitore, string nome, Vector3 posizione, Vector3 scala, Color colore, Material materiale, bool tieniCollider)
    {
        GameObject parte = GameObject.CreatePrimitive(PrimitiveType.Cube);
        parte.name = nome;
        if (!tieniCollider) DestroyImmediate(parte.GetComponent<Collider>());
        parte.transform.SetParent(genitore, false);
        parte.transform.localPosition = posizione;
        parte.transform.localScale = scala;

        Renderer r = parte.GetComponent<Renderer>();
        if (materiale == null) materiale = new Material(r.sharedMaterial) { color = colore };
        r.sharedMaterial = materiale;
        return materiale;
    }

    void OnGUI()
    {
        if (!PuoAprire()) return;
        var stile = new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(Screen.width * 0.5f - 130f, Screen.height * 0.75f, 260f, 40f), "E   Apri il baule", stile);
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.7f, 0.45f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f));
    }
}
