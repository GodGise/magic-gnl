using UnityEngine;

// La bussola in alto al centro dello schermo, come una striscia: mostra i punti cardinali nella direzione in cui
// guarda la camera e un rombo color fiamma dove si trova l'obiettivo attuale (MetaObiettivo), con la distanza in metri.
// Se l'obiettivo è dietro, il rombo resta sul bordo della striscia, dal lato verso cui girarsi.
// Si vede solo nelle scene della storia (dove c'è il componente Missioni), non in pausa, con l'inventario aperto o
// durante un dialogo. Il nord è l'asse Z del mondo.
// Come montarlo: non si monta, si crea da solo all'avvio del gioco.
public class Bussola : MonoBehaviour
{
    const float Larghezza = 640f, Alto = 30f, Cima = 18f;
    const float CampoVisivo = 90f;   // gradi a destra e a sinistra del centro mostrati sulla striscia

    static readonly string[] Cardinali = { "hud.nord", "hud.est", "hud.sud", "hud.ovest" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (FindFirstObjectByType<Bussola>() != null) return;
        var oggetto = new GameObject("Bussola");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<Bussola>();
    }

    GiocatoreControllo giocatore;

    void OnGUI()
    {
        var missioni = Missioni.Istanza;
        if (missioni == null || !HudGioco.Attivo || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();

        GUI.depth = 10;
        GraficaMenu.PreparaStili();
        float larghezza = GraficaMenu.FoglioIntero();
        float cx = larghezza * 0.5f;
        var striscia = new Rect(cx - Larghezza / 2f, Cima, Larghezza, Alto);
        GraficaMenu.Riempi(striscia, new Color(0.02f, 0.02f, 0.03f, 0.7f));
        GraficaMenu.Bordo(striscia, 1.5f, GraficaMenu.Con(GraficaMenu.Bronzo, 0.8f));
        GraficaMenu.Rombo(new Vector2(cx, striscia.yMax), 8f, GraficaMenu.Bronzo);   // dove guardi

        float imbardata = cam.transform.eulerAngles.y;

        // Tacche ogni 15 gradi, più lunghe ogni 45; lettere sui punti cardinali.
        var stileLettera = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        for (int a = 0; a < 360; a += 15)
        {
            float rel = Mathf.DeltaAngle(imbardata, a);
            if (Mathf.Abs(rel) > CampoVisivo) continue;
            float x = cx + rel / CampoVisivo * (Larghezza / 2f - 12f);
            float alfa = 1f - Mathf.Pow(Mathf.Abs(rel) / CampoVisivo, 2f);
            if (a % 90 == 0)
            {
                GraficaMenu.Scritta(new Rect(x - 20f, striscia.y, 40f, Alto), Lingua.T(Cardinali[a / 90]), stileLettera,
                    a == 0 ? GraficaMenu.Selezione : GraficaMenu.Testo, alfa);
                continue;
            }
            float h = a % 45 == 0 ? 10f : 5f;
            GraficaMenu.Riempi(new Rect(x - 1f, striscia.yMax - h - 3f, 2f, h), GraficaMenu.Con(GraficaMenu.Bronzo, alfa));
        }

        // L'obiettivo.
        Transform meta = missioni.Meta;
        if (meta == null) return;
        Vector3 da = giocatore != null ? giocatore.transform.position : cam.transform.position;
        Vector3 verso = meta.position - da;
        verso.y = 0f;
        float relMeta = Mathf.DeltaAngle(imbardata, Mathf.Atan2(verso.x, verso.z) * Mathf.Rad2Deg);
        bool fuori = Mathf.Abs(relMeta) > CampoVisivo;
        float xm = cx + Mathf.Clamp(relMeta / CampoVisivo, -1f, 1f) * (Larghezza / 2f - 12f);
        float pulsa = fuori ? 0.6f + 0.4f * Mathf.Sin(Time.time * 5f) : 1f;
        GraficaMenu.Rombo(new Vector2(xm, striscia.center.y), 18f, GraficaMenu.Con(GraficaMenu.Riquadro, 1f));
        GraficaMenu.Rombo(new Vector2(xm, striscia.center.y), 14f, GraficaMenu.Con(GraficaMenu.Selezione, pulsa));
        var stileDistanza = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        GraficaMenu.Scritta(new Rect(xm - 50f, striscia.yMax + 4f, 100f, 26f), Mathf.RoundToInt(verso.magnitude) + " m",
            stileDistanza, GraficaMenu.Testo, 0.9f);
    }
}
