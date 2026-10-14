using UnityEngine;

// Lo scheletro vivente (quest 5, la cripta della prima guardia): nemico base, fragile ma pericoloso (Lorenzo, 13 ottobre).
// Numeri di partenza: 45 di vita, 12 di danno a colpo, un colpo ogni 1,3 secondi (sono in Bersaglio: "Vita Massima",
// "Danno Attacco"; l'intervallo è qui sotto).
// Come combatte: dorme in piedi, curvo, finché il giocatore non arriva vicino (e lo vede) o non lo colpisce; si sveglia
// con uno scatto e sveglia anche gli scheletri vicini. Poi:
//   1. si avvicina fino a portata;
//   2. colpisce con uno dei suoi colpi, a turno: FENDENTE (orizzontale, preavviso medio), ROVESCIO (veloce, preavviso
//      corto: arriva subito dopo il fendente) e CALATA (dall'alto, preavviso lungo, con un affondo in avanti e più
//      portata). Il preavviso è il rosso di Bersaglio: alla fine del rosso si para o si schiva;
//   3. si riposiziona: due passi indietro e di lato (alterna destra e sinistra), così il giocatore ha un attimo per
//      rispondere;
//   4. torna all'attacco con il colpo dopo.
// Al massimo 2 scheletri caricano un colpo nello stesso momento (gli altri aspettano girando intorno).
// Se il giocatore scappa lontano (o non lo vede per qualche secondo) torna al suo posto e si riaddormenta.
// Morendo lascia qualche moneta (Borsa).
// Vita, morte, aggancio, colpi ricevuti e difficoltà co-op sono quelli di Bersaglio (con "Attacca Il Giocatore" spento:
// gli attacchi li comanda questo componente con Bersaglio.AttaccaOra). Stordito o bloccato dagli incantesimi non si
// muove; rallentato va più piano.
// Come montarlo: su un cilindro di Unity (alto 2 m) con Bersaglio; la figura a blocchi la crea Bersaglio, qui si fa
// più magra (ossa). Il comando "magic-gnl > Cripta: crea la scena della cripta" mette gli scheletri pronti.
// Co-op: pensa solo sul PC di chi ospita (come gli altri nemici); la cripta per ora si gioca da soli (prologo).
[RequireComponent(typeof(Bersaglio))]
public class ScheletroVivente : MonoBehaviour
{
    [Header("Attacchi")]
    [Tooltip("Secondi minimi fra un colpo e il successivo (accorciati dalla difficoltà co-op).")]
    [SerializeField] float intervalloColpi = 1.3f;
    [Tooltip("Preavviso (rosso) del fendente, del rovescio e della calata, in secondi.")]
    [SerializeField] Vector3 preavvisi = new Vector3(0.45f, 0.3f, 0.6f);
    [Tooltip("Portata del fendente e del rovescio, in metri.")]
    [SerializeField] float portata = 2f;
    [Tooltip("Portata della calata (con l'affondo), in metri.")]
    [SerializeField] float portataCalata = 2.6f;
    [Tooltip("Di quanto avanza con l'affondo della calata, in metri.")]
    [SerializeField] float affondo = 0.7f;

    [Header("Riposizionamento")]
    [Tooltip("Di quanti metri si allontana dopo un colpo (indietro e di lato).")]
    [SerializeField] float distanzaRiposizionamento = 1.6f;
    [SerializeField] float durataRiposizionamento = 0.5f;

    [Header("Movimento")]
    [SerializeField] float velocita = 3.2f;
    [Tooltip("Distanza a cui si ferma per colpire.")]
    [SerializeField] float distanzaColpo = 1.6f;
    [Tooltip("Distanza a cui aspetta il suo turno quando altri due stanno già colpendo.")]
    [SerializeField] float distanzaAttesa = 3.2f;

    [Header("Risveglio")]
    [Tooltip("Si sveglia se il giocatore arriva a questa distanza (e lo vede).")]
    [SerializeField] float distanzaRisveglio = 9f;
    [Tooltip("Svegliandosi sveglia gli scheletri entro questi metri.")]
    [SerializeField] float raggioSveglia = 7f;
    [Tooltip("Rinuncia se il giocatore è più lontano di così dal suo posto, o se non lo vede per 6 secondi.")]
    [SerializeField] float distanzaMassimaDalPosto = 22f;

    [Header("Bottino")]
    [Tooltip("Monete lasciate morendo.")]
    [SerializeField, Min(0)] int lacrime = 3;

    const int MassimoInCarica = 2;
    static int inCarica;   // quanti scheletri stanno caricando un colpo adesso
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Azzera() => inCarica = 0;   // valore rimasto da un Play precedente

    enum Fase { Dorme, Sveglia, Avvicina, Colpisce, Riposiziona, Torna }

    Bersaglio bersaglio;
    Fase fase = Fase.Dorme;
    float inizioFase, prossimoColpo, ultimaVista;
    int colpo;              // 0 fendente, 1 rovescio, 2 calata
    float lato = 1f;
    Vector3 posto, versoRiposizionamento;
    Quaternion rotazionePosto;
    IObiettivoNemico preso;
    bool contatoInCarica, affondoFatto, bottinoDato;
    float altezzaCentro = 1f;
    Transform figura;

    void Awake()
    {
        bersaglio = GetComponent<Bersaglio>();
        bersaglio.attaccaIlGiocatore = false;
        bersaglio.Colpito += Svegliati;
        posto = transform.position;
        rotazionePosto = transform.rotation;
        altezzaCentro = transform.lossyScale.y;   // cilindro di Unity: alto 2 volte la scala, centro a metà
        lato = Random.value < 0.5f ? -1f : 1f;
        colpo = Random.Range(0, 3);
    }

    void OnDestroy()
    {
        if (bersaglio != null) bersaglio.Colpito -= Svegliati;
        LasciaCarica();
    }

    void OnDisable() => LasciaCarica();

    void Start()
    {
        // Ossa: braccia e gambe più magre, costole scure sul busto, testa un po' più piccola.
        figura = transform.Find("Aspetto umanoide");
        if (figura != null)
        {
            foreach (Transform t in figura.GetComponentsInChildren<Transform>())
            {
                if (t.name.StartsWith("Braccio") || t.name.StartsWith("Gamba"))
                    t.localScale = new Vector3(t.localScale.x * 0.5f, t.localScale.y, t.localScale.z * 0.5f);
                else if (t.name == "Busto") t.localScale = new Vector3(t.localScale.x * 0.8f, t.localScale.y, t.localScale.z * 0.6f);
                else if (t.name == "Testa") t.localScale *= 0.9f;
            }
            var busto = figura.Find("Busto");
            if (busto != null)
                for (int i = 0; i < 3; i++)
                {
                    var costola = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(costola.GetComponent<Collider>());
                    costola.name = "Costola";
                    costola.transform.SetParent(busto, false);
                    costola.transform.localPosition = new Vector3(0f, 0.25f - i * 0.2f, 0.52f);
                    costola.transform.localScale = new Vector3(0.85f, 0.06f, 0.05f);
                    var r = costola.GetComponent<Renderer>();
                    r.sharedMaterial = new Material(busto.GetComponent<Renderer>().sharedMaterial) { color = new Color(0.25f, 0.22f, 0.18f) };
                }
        }
        Dormi();
    }

    float Intervallo => intervalloColpi / DifficoltaCoop.FrequenzaAttacchi;

    void Update()
    {
        if (Rete.Ospite) return;
        if (bersaglio.Morto)
        {
            LasciaCarica();
            if (!bottinoDato && lacrime > 0) DaiBottino();
            return;
        }

        var effetti = EffettiNemico.Di(bersaglio);
        float lento = effetti != null ? effetti.MoltiplicatoreVelocita : 1f;
        float t = Time.time - inizioFase;

        if (fase == Fase.Dorme)
        {
            var vicino = ObiettiviNemici.PiuVicino(transform.position);
            if (vicino != null && Vector3.Distance(vicino.Corpo.position, transform.position) <= distanzaRisveglio && Vede(vicino))
                Svegliati();
            return;
        }
        if (fase == Fase.Sveglia)
        {
            // si raddrizza con uno scatto, poi parte
            if (figura != null) figura.localRotation = Quaternion.Slerp(Quaternion.Euler(25f, 0f, 0f), Quaternion.identity, Mathf.Clamp01(t / 0.5f));
            if (t >= 0.6f) Cambia(Fase.Avvicina);
            return;
        }
        if (lento <= 0f) return;   // stordito o bloccato

        if (fase == Fase.Torna)
        {
            Vector3 versoPosto = Piatto(posto - transform.position);
            if (versoPosto.magnitude < 0.4f) { transform.rotation = rotazionePosto; Dormi(); return; }
            Guarda(versoPosto);
            Muovi(versoPosto.normalized * velocita * 0.7f * lento * Time.deltaTime);
            // ritornando, se rivede il giocatore vicino si risveglia
            var vicino = ObiettiviNemici.PiuVicino(transform.position);
            if (vicino != null && Vector3.Distance(vicino.Corpo.position, transform.position) <= distanzaRisveglio * 0.6f && Vede(vicino))
                Cambia(Fase.Avvicina);
            return;
        }

        if (preso == null || !ObiettiviNemici.Valido(preso)) preso = ObiettiviNemici.PiuVicino(transform.position);
        if (preso == null) { Cambia(Fase.Torna); return; }
        Vector3 versoBersaglio = Piatto(preso.Corpo.position - transform.position);
        float distanza = versoBersaglio.magnitude;
        if (Vede(preso)) ultimaVista = Time.time;
        if (Vector3.Distance(preso.Corpo.position, posto) > distanzaMassimaDalPosto || Time.time - ultimaVista > 6f)
        {
            preso = null;
            Cambia(Fase.Torna);
            return;
        }

        switch (fase)
        {
            case Fase.Avvicina:
                Guarda(versoBersaglio);
                bool mioTurno = inCarica < MassimoInCarica;
                float voluta = mioTurno ? distanzaColpo : distanzaAttesa;
                Vector3 passo = Vector3.zero;
                if (distanza > voluta + 0.2f) passo = versoBersaglio.normalized;
                else if (distanza < voluta - 0.8f) passo = -versoBersaglio.normalized * 0.6f;
                if (!mioTurno) passo += Vector3.Cross(Vector3.up, versoBersaglio.normalized) * lato * 0.5f;   // gira intorno aspettando
                Muovi(passo * velocita * lento * Time.deltaTime);
                float portataColpo = colpo == 2 ? portataCalata : portata;
                if (mioTurno && Time.time >= prossimoColpo && distanza <= portataColpo - 0.1f
                    && bersaglio.AttaccaOra(preso, preavvisi[colpo], portataColpo, colpo))
                {
                    inCarica++;
                    contatoInCarica = true;
                    affondoFatto = false;
                    Cambia(Fase.Colpisce);
                }
                break;

            case Fase.Colpisce:
                // durante il rosso resta fermo; la calata finisce con un affondo in avanti
                if (colpo == 2 && !affondoFatto && t >= preavvisi[2] - 0.08f)
                {
                    affondoFatto = true;
                    Muovi(transform.forward * affondo);
                }
                if (!bersaglio.StaAttaccando && t > 0.05f)
                {
                    LasciaCarica();
                    prossimoColpo = Time.time + Intervallo - durataRiposizionamento * 0.5f;
                    // indietro e di lato, alternando
                    versoRiposizionamento = (-versoBersaglio.normalized + Vector3.Cross(Vector3.up, versoBersaglio.normalized) * lato).normalized;
                    lato = -lato;
                    colpo = (colpo + 1) % 3;
                    Cambia(Fase.Riposiziona);
                }
                break;

            case Fase.Riposiziona:
                Guarda(versoBersaglio);
                Muovi(versoRiposizionamento * (distanzaRiposizionamento / durataRiposizionamento) * lento * Time.deltaTime);
                if (t >= durataRiposizionamento) Cambia(Fase.Avvicina);
                break;
        }
    }

    // Si sveglia (anche se colpito mentre dorme) e sveglia gli scheletri vicini.
    void Svegliati()
    {
        if (bersaglio == null || bersaglio.Morto || (fase != Fase.Dorme && fase != Fase.Torna)) return;
        bool dormiva = fase == Fase.Dorme;
        ultimaVista = Time.time;
        prossimoColpo = Time.time + 0.8f;
        Cambia(dormiva ? Fase.Sveglia : Fase.Avvicina);
        bersaglio.SegnaInseguimento();
        if (dormiva) Suoni.Suona(Suono.Fendente, transform.position + Vector3.up, 0.5f, 0.55f);   // ossa che scricchiolano
        foreach (var altro in FindObjectsByType<ScheletroVivente>(FindObjectsSortMode.None))
            if (altro != this && altro.fase == Fase.Dorme && Vector3.Distance(altro.transform.position, transform.position) <= raggioSveglia)
                altro.Svegliati();
    }

    void Dormi()
    {
        Cambia(Fase.Dorme);
        if (figura != null) figura.localRotation = Quaternion.Euler(25f, 0f, 0f);   // curvo, testa bassa
    }

    void Cambia(Fase nuova)
    {
        fase = nuova;
        inizioFase = Time.time;
        if (nuova != Fase.Dorme && nuova != Fase.Sveglia && figura != null) figura.localRotation = Quaternion.identity;
        if (nuova == Fase.Avvicina || nuova == Fase.Colpisce || nuova == Fase.Riposiziona) bersaglio.SegnaInseguimento();
    }

    void LasciaCarica()
    {
        if (!contatoInCarica) return;
        contatoInCarica = false;
        inCarica = Mathf.Max(0, inCarica - 1);
    }

    void DaiBottino()
    {
        bottinoDato = true;
        var giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        Borsa.Di(giocatore).Aggiungi(lacrime);
        MessaggiSchermo.Mostra(Lingua.T("hud.ricevuto") + ": " + Borsa.Testo(lacrime), 2f);
    }

    // Vede il giocatore se non c'è un muro in mezzo.
    bool Vede(IObiettivoNemico chi)
    {
        Vector3 occhi = transform.position + Vector3.up * altezzaCentro * 0.7f;
        Vector3 la = chi.Corpo.position + Vector3.up * 0.5f;
        Vector3 d = la - occhi;
        foreach (var c in Physics.RaycastAll(occhi, d.normalized, d.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c.transform.IsChildOf(transform)) continue;
            if (c.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            if (c.collider.GetComponentInParent<Bersaglio>() != null) continue;
            return false;
        }
        return true;
    }

    void Guarda(Vector3 verso)
    {
        if (verso.sqrMagnitude > 0.0001f && !bersaglio.StaAttaccando)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(verso.normalized), 10f * Time.deltaTime);
    }

    void Muovi(Vector3 spostamento)
    {
        if (spostamento.sqrMagnitude < 1e-8f) return;
        Vector3 nuova = Appoggia(transform.position + spostamento);
        if (!Bloccato(transform.position, nuova)) transform.position = nuova;
    }

    // Vero se fra due punti c'è un muro o un oggetto solido.
    bool Bloccato(Vector3 da, Vector3 a)
    {
        Vector3 d = a - da;
        if (d.sqrMagnitude < 1e-6f) return false;
        foreach (var c in Physics.SphereCastAll(da, 0.35f, d.normalized, d.magnitude + 0.15f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c.transform.IsChildOf(transform) || c.distance <= 0f) continue;
            if (c.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            if (c.collider.GetComponentInParent<Bersaglio>() != null) continue;
            if (c.normal.y > 0.6f) continue;   // pavimento
            return true;
        }
        return false;
    }

    // Tiene lo scheletro appoggiato al pavimento.
    Vector3 Appoggia(Vector3 punto)
    {
        Vector3 inizio = new Vector3(punto.x, transform.position.y + 0.5f, punto.z);
        float y = punto.y, migliore = float.MaxValue;
        foreach (var c in Physics.RaycastAll(inizio, Vector3.down, altezzaCentro + 2f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c.transform.IsChildOf(transform) || c.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            if (c.collider.GetComponentInParent<Bersaglio>() != null) continue;
            if (c.distance < migliore) { migliore = c.distance; y = c.point.y + altezzaCentro; }
        }
        punto.y = y;
        return punto;
    }

    static Vector3 Piatto(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
