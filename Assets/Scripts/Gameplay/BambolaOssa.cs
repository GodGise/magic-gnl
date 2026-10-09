using UnityEngine;

// Bambola di ossa, evocazione dello Stregone (Docs/incantesimi-stregone.md). Una bambola fatta di ossa piantata a
// terra: non attacca, ma i nemici entro "Raggio Attrazione" metri vanno da lei e attaccano lei invece dei giocatori
// (vedi ObiettiviNemici.Esca). Con due bambole ogni nemico va da quella più vicina.
//   - Se i nemici la distruggono scoppia in una nube di gas larga "Raggio" metri: chi è dentro rallenta e prende
//     danno al secondo per "Durata Rallentamento" secondi.
//   - Se nessuno la distrugge per "Tempo Trasformazione" secondi, si anima in uno scheletro che attacca il nemico
//     più vicino ("Danno Scheletro" ogni "Intervallo Scheletro" s) e dura al massimo "Durata Scheletro" secondi.
//     Lo scheletro, morendo, non fa il gas, e non attira più i nemici.
// Una sola per giocatore (due con l'amuleto Occhio del lago).
// Come montarlo: non si monta. La crea MagiaStregone con BambolaOssa.Crea.
public class BambolaOssa : Evocazione, IEscaNemici
{
    static readonly Color Osso = new Color(0.86f, 0.82f, 0.7f);
    bool scheletro;
    float prossimoColpo;
    Transform figura;

    public float RaggioAttrazione => scheletro ? 0f : dati.raggioAttrazione;

    public static BambolaOssa Crea(MagiaStregone chi, DatiIncantesimo inc, Vector3 posto)
    {
        if (Physics.Raycast(posto + Vector3.up * 2f, Vector3.down, out RaycastHit terra, 6f, ~0, QueryTriggerInteraction.Ignore)) posto = terra.point;
        var oggetto = new GameObject("Bambola di ossa");
        oggetto.transform.SetPositionAndRotation(posto, chi.transform.rotation);
        var bambola = oggetto.AddComponent<BambolaOssa>();
        bambola.figura = new GameObject("Figura").transform;
        bambola.figura.SetParent(oggetto.transform, false);
        // Bastone piantato, croce delle braccia, teschio: piccola e storta.
        Forma(PrimitiveType.Cylinder, bambola.figura, new Vector3(0f, 0.45f, 0f), new Vector3(0.08f, 0.45f, 0.08f), Osso, 0.3f);
        Forma(PrimitiveType.Cylinder, bambola.figura, new Vector3(0f, 0.7f, 0f), new Vector3(0.06f, 0.3f, 0.06f), Osso, 0.3f)
            .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        Forma(PrimitiveType.Sphere, bambola.figura, new Vector3(0f, 1f, 0f), new Vector3(0.22f, 0.25f, 0.22f), Osso, 0.3f);
        bambola.figura.localRotation = Quaternion.Euler(0f, 0f, 8f);
        // La durata della bambola è il tempo prima di animarsi (le evocazioni più lunghe aspettano di più).
        bambola.Prepara(chi, inc, inc.vitaEvocazione, inc.tempoTrasformazione);
        return bambola;
    }

    protected override void Aggiorna()
    {
        if (!scheletro)
        {
            // Trema un poco, per farsi notare.
            figura.localRotation = Quaternion.Euler(0f, 0f, 8f + 3f * Mathf.Sin(Time.time * 7f));
            return;
        }
        Bersaglio nemico = NemicoVicino(15f);
        if (nemico == null) return;
        CamminaVerso(nemico.transform.position, 3.5f, 1.4f);
        Vector3 a = transform.position, b = nemico.transform.position;
        a.y = b.y = 0f;
        if (Time.time >= prossimoColpo && Vector3.Distance(a, b) <= 1.8f)
        {
            prossimoColpo = Time.time + dati.intervalloScheletro;
            padrone.DannoNelTempo(dati, nemico, dati.dannoScheletro, 1f, transform.position, true);
            Suoni.Suona(Suono.ImpattoColpo, nemico.transform.position + Vector3.up, 0.6f, 1.4f);
        }
    }

    // Nessuno l'ha distrutta: si anima in uno scheletro.
    protected override void Scade()
    {
        if (scheletro) { Termina(false); return; }
        scheletro = true;
        vitaMassima = vita = padrone.VitaEvocazione(Mathf.Max(1f, dati.vitaScheletro));
        fineDurata = Time.time + padrone.DurataEvocazione(Mathf.Max(1f, dati.durataScheletro));
        name = "Scheletro";
        // Da bambola a scheletro: si alza in piedi.
        figura.localRotation = Quaternion.identity;
        figura.localScale = new Vector3(1f, 1.7f, 1f);
        Forma(PrimitiveType.Cylinder, figura, new Vector3(-0.08f, 0.15f, 0f), new Vector3(0.05f, 0.18f, 0.05f), Osso, 0.3f);
        Forma(PrimitiveType.Cylinder, figura, new Vector3(0.08f, 0.15f, 0f), new Vector3(0.05f, 0.18f, 0.05f), Osso, 0.3f);
        Suoni.Suona(Suono.Rinascita, transform.position + Vector3.up, 0.6f, 0.6f);
    }

    // Distrutta dai nemici: la bambola scoppia nel gas, lo scheletro no.
    protected override void Muori()
    {
        if (!scheletro && padrone != null) ZonaMagica.CreaGas(padrone, dati, transform.position);
        Termina(true);
    }
}
