using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// La magia dello Stregone (Docs/incantesimi-stregone.md): caselle degli incantesimi, scelta con i tasti numerici,
// attese, costi e lancio dei 12 incantesimi.
// A cosa serve: lo Stregone non ha la spada. Il tasto d'attacco lancia l'incantesimo della casella scelta:
//   - tasti 1-6 (croce sinistra e destra sul pad) scelgono la casella; le caselle sono 4, fino a 6 con certi libri;
//   - ogni incantesimo costa mana, ha una carica, un recupero e un'attesa prima di poterlo rilanciare;
//   - bastone, libro, veste e amuleto cambiano questi numeri con i loro pro e contro (ModificatoriMagia, sommati
//     da Equipaggiamento);
//   - evocazioni (Fuoco fatuo, Spirito del lupo, Bambola di ossa): l'attesa parte quando l'evocazione precedente
//     finisce o muore; ce ne possono essere al massimo "Massimo Insieme" (lupo e bambola: 2 con l'Occhio del lago).
// Il personaggio (carica, recupero, animazione, mana) lo gestisce GiocatoreControllo, che chiede qui cosa lanciare
// (PuoLanciare, IniziaLancio) e quando la carica finisce chiama Lancia.
// Regole di danno: i moltiplicatori (Dardo d'ombra ×2 su un nemico ignaro, ×2,5 del Passo d'ombra) seguono la regola
// di Lorenzo (vedi Moltiplicatori); il ×2,5 vale solo per il colpo diretto, non per scia ed evocazioni.
// Il furto di vita degli amuleti non vale per gli incantesimi; il Cuore del lago nero ridà mana (RubaMana).
// Co-op: il danno passa da Bersaglio.RiceviColpo e gli effetti da EffettiNemico.Applica, che in rete vanno all'host.
// Le evocazioni per ora esistono solo sul PC di chi le lancia (vedi Evocazione).
// Come montarlo: non si monta. GiocatoreControllo lo aggiunge da solo quando la classe scelta è lo Stregone.
[RequireComponent(typeof(GiocatoreControllo))]
public class MagiaStregone : MonoBehaviour
{
    GiocatoreControllo giocatore;
    Equipaggiamento equipaggiamento;
    Statistiche statistiche;
    AggancioBersaglio aggancio;

    int scelta;                                   // casella scelta (0 = tasto 1)
    DatiIncantesimo inCorso;                      // l'incantesimo che si sta caricando
    float moltiplicatorePasso = 1f;               // ×2,5 se la carica è partita durante il Passo d'ombra
    readonly Dictionary<DatiIncantesimo, float> prontoDa = new Dictionary<DatiIncantesimo, float>();
    readonly Dictionary<DatiIncantesimo, List<Evocazione>> evocazioni = new Dictionary<DatiIncantesimo, List<Evocazione>>();
    float prossimoAvviso;

    public int Scelta => scelta;
    public DatiIncantesimo Scelto => equipaggiamento != null ? equipaggiamento.Incantesimo(scelta) : null;
    public ModificatoriMagia Magia => equipaggiamento != null ? equipaggiamento.Magia : new ModificatoriMagia();

    void Awake()
    {
        giocatore = GetComponent<GiocatoreControllo>();
        equipaggiamento = GetComponent<Equipaggiamento>();
        if (equipaggiamento == null) equipaggiamento = gameObject.AddComponent<Equipaggiamento>();
        statistiche = Statistiche.Di(this);
        aggancio = GetComponent<AggancioBersaglio>();
    }

    void Update()
    {
        if (InventarioGioco.Aperto || MenuPausa.InPausa || equipaggiamento == null) return;
        var tastiera = Keyboard.current;
        int n = equipaggiamento.NumeroCaselle;
        if (tastiera != null)
        {
            Key[] tasti = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6 };
            for (int i = 0; i < n && i < tasti.Length; i++)
                if (tastiera[tasti[i]].wasPressedThisFrame) Scegli(i);
        }
        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.dpad.right.wasPressedThisFrame) Scegli((scelta + 1) % n);
            if (pad.dpad.left.wasPressedThisFrame) Scegli((scelta - 1 + n) % n);
        }
        if (scelta >= n) scelta = 0;
    }

    void Scegli(int casella)
    {
        if (casella == scelta) return;
        scelta = casella;
        Suoni.Suona(Suono.CambioArma, transform.position + Vector3.up, 0.5f, 1.2f);
    }

    // ---------- numeri con i pro e contro ----------

    float VelocitaLancio => 1f - (statistiche != null ? statistiche.VelocitaAttacco : 0f) / 100f;   // amuleti: velocità d'attacco

    public float Carica(DatiIncantesimo inc)
    {
        var m = Magia;
        float percento = m.caricaPercento + (inc.scuola == ScuolaMagia.Evocazione ? m.caricaEvocazioniPercento : 0f);
        return Mathf.Max(0.05f, (inc.carica + m.caricaSecondi) * (1f + percento / 100f) * VelocitaLancio);
    }

    public float Recupero(DatiIncantesimo inc) => Mathf.Max(0.05f, inc.recupero * (1f + Magia.recuperoPercento / 100f) * VelocitaLancio);
    public float Attesa(DatiIncantesimo inc) => Mathf.Max(0f, inc.attesa * (1f + Magia.attesaPercento / 100f));
    public float Costo(DatiIncantesimo inc) => Mathf.Max(0f, inc.costoMana * (1f + Magia.costoPercento / 100f));

    // Danno di base dell'incantesimo con i pro e contro (prima di critico, armatura e moltiplicatori).
    public float Danno(DatiIncantesimo inc, float base_)
    {
        var m = Magia;
        float percento = m.Di(inc.scuola).dannoPercento + (inc.scuola == ScuolaMagia.Evocazione ? 0f : m.dannoPercento);
        return Mathf.Max(0f, base_ * (1f + percento / 100f));
    }

    // Durata di rallentamenti, blocchi e stordimenti della scuola dell'incantesimo.
    public float DurataEffetto(DatiIncantesimo inc, float base_) => Mathf.Max(0f, base_ * (1f + Magia.Di(inc.scuola).durataEffettiPercento / 100f));
    public float VitaEvocazione(float base_) => Mathf.Max(1f, base_ * (1f + Magia.vitaEvocazioniPercento / 100f));
    public float DurataEvocazione(float base_) => Mathf.Max(1f, base_ * (1f + Magia.durataEvocazioniPercento / 100f));

    public int MassimoEvocazioni(DatiIncantesimo inc)
    {
        int massimo = Mathf.Max(1, inc.massimoInsieme);
        bool doppiabile = inc.effetto == DatiIncantesimo.Effetto.Lupo || inc.effetto == DatiIncantesimo.Effetto.Bambola;
        return doppiabile && Magia.doppiaEvocazione ? massimo * 2 : massimo;
    }

    int EvocazioniAttive(DatiIncantesimo inc)
    {
        if (!evocazioni.TryGetValue(inc, out var lista)) return 0;
        lista.RemoveAll(e => e == null);
        return lista.Count;
    }

    bool EEvocazione(DatiIncantesimo inc) => inc.effetto == DatiIncantesimo.Effetto.FuocoFatuo
        || inc.effetto == DatiIncantesimo.Effetto.Lupo || inc.effetto == DatiIncantesimo.Effetto.Bambola;

    // Da 0 (appena lanciato) a 1 (pronto): per l'HUD.
    public float Pronto(DatiIncantesimo inc)
    {
        if (inc == null) return 0f;
        if (EEvocazione(inc) && EvocazioniAttive(inc) >= MassimoEvocazioni(inc)) return 0f;
        if (!prontoDa.TryGetValue(inc, out float quando) || Time.time >= quando) return 1f;
        float totale = Attesa(inc);
        return totale <= 0f ? 1f : Mathf.Clamp01(1f - (quando - Time.time) / totale);
    }

    // ---------- lancio (chiamato da GiocatoreControllo) ----------

    // Si può lanciare l'incantesimo scelto? Se no avvisa una volta al secondo (casella vuota, non pronto, poco mana).
    public bool PuoLanciare()
    {
        var inc = Scelto;
        string avviso = null;
        if (inc == null) avviso = "hud.nessun_incantesimo";
        else if (Pronto(inc) < 1f) avviso = "hud.non_pronto";
        else if (giocatore.Mana < Costo(inc)) avviso = "hud.mana_insufficiente";
        if (avviso == null) return true;
        if (Time.time >= prossimoAvviso)
        {
            prossimoAvviso = Time.time + 1f;
            MessaggiSchermo.Mostra(Lingua.T(avviso), 1.5f);
            Suoni.Suona(Suono.Negato, transform.position + Vector3.up, 0.7f);
        }
        return false;
    }

    // Inizia la carica dell'incantesimo scelto: restituisce costo, carica e recupero. "daPasso": la carica parte
    // durante il Passo d'ombra, quindi questo incantesimo farà il danno moltiplicato.
    public DatiIncantesimo IniziaLancio(float moltiplicatoreDaPasso, out float costo, out float carica, out float recupero)
    {
        inCorso = Scelto;
        moltiplicatorePasso = moltiplicatoreDaPasso;
        costo = inCorso != null ? Costo(inCorso) : 0f;
        carica = inCorso != null ? Carica(inCorso) : 0.3f;
        recupero = inCorso != null ? Recupero(inCorso) : 0.3f;
        // L'attesa parte già adesso: così non si può rilanciare lo stesso incantesimo durante la carica.
        if (inCorso != null && !EEvocazione(inCorso)) prontoDa[inCorso] = Time.time + carica + Attesa(inCorso);
        return inCorso;
    }

    // Il bersaglio: quello agganciato, altrimenti il nemico vivo più vicino entro la portata.
    public Bersaglio ScegliBersaglio(float portata)
    {
        if (aggancio != null && aggancio.Agganciato && !aggancio.Attuale.Morto) return aggancio.Attuale;
        Bersaglio migliore = null;
        float minima = portata;
        foreach (Bersaglio b in FindObjectsByType<Bersaglio>(FindObjectsSortMode.None))
        {
            if (b == null || b.Morto || !b.isActiveAndEnabled) continue;
            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < minima) { minima = d; migliore = b; }
        }
        return migliore;
    }

    public float PortataScelta => inCorso != null ? Mathf.Max(inCorso.portata, 1f) : 20f;

    // La carica è finita: l'incantesimo parte.
    public void Lancia(Bersaglio obiettivo, Vector3 partenza, Vector3 direzione)
    {
        var inc = inCorso;
        inCorso = null;
        if (inc == null) return;
        float molt = moltiplicatorePasso;
        moltiplicatorePasso = 1f;

        switch (inc.effetto)
        {
            case DatiIncantesimo.Effetto.Proiettile:
                SferaMagica.LanciaIncantesimo(partenza, direzione, obiettivo, inc.velocita, transform, inc.colore, inc.grandezza, 3f,
                    (b, punto) => { if (b != null) ColpisciNemico(inc, b, punto, molt, true); });
                break;

            case DatiIncantesimo.Effetto.PallaDiFuoco:
                SferaMagica.LanciaIncantesimo(partenza, direzione, obiettivo, inc.velocita, transform, inc.colore, inc.grandezza,
                    inc.portata / Mathf.Max(1f, inc.velocita), (b, punto) => Esplosione(inc, punto, molt));
                break;

            case DatiIncantesimo.Effetto.Scia:
                ZonaMagica.CreaScia(this, inc, transform.position, transform.forward);
                break;

            case DatiIncantesimo.Effetto.Onda:
                Onda(inc, molt);
                break;

            case DatiIncantesimo.Effetto.Pozza:
                Pozza(inc, obiettivo, molt);
                break;

            case DatiIncantesimo.Effetto.PassoOmbra:
                giocatore.AttivaPassoOmbra(inc.durata, inc.velocitaInPiu, inc.moltiplicatoreSuccessivo,
                    inc.rallentamentoInterruzione, inc.durataRallentamentoInterruzione);
                break;

            case DatiIncantesimo.Effetto.Velo:
                ZonaMagica.CreaVelo(this, inc, transform.position);
                break;

            case DatiIncantesimo.Effetto.FuocoFatuo:
            case DatiIncantesimo.Effetto.Lupo:
            case DatiIncantesimo.Effetto.Bambola:
                Evoca(inc, partenza);
                break;
        }
        Suoni.Suona(Suono.SferaLancio, partenza, 0.9f);
        if (inc.effetto == DatiIncantesimo.Effetto.Proiettile || inc.effetto == DatiIncantesimo.Effetto.PallaDiFuoco)
            MondoRete.InviaSfera(partenza, direzione, obiettivo, inc.velocita);   // gli altri giocatori la vedono partire
    }

    // ---------- danno ----------

    // Un colpo diretto di un incantesimo su un nemico. "moltiplicatoreDaPasso" vale solo per i colpi diretti.
    public void ColpisciNemico(DatiIncantesimo inc, Bersaglio b, Vector3 da, float moltiplicatoreDaPasso, bool diretto, float base_ = -1f)
    {
        if (b == null || b.Morto) return;
        var molt = new List<float>();
        if (inc.moltiplicatoreIgnaro > 1f && b.TryGetComponent(out InseguimentoNemico vista) && vista.Ignaro) molt.Add(inc.moltiplicatoreIgnaro);
        if (diretto && moltiplicatoreDaPasso > 1f) molt.Add(moltiplicatoreDaPasso);
        float danno = Danno(inc, base_ >= 0f ? base_ : inc.danno) * Moltiplicatori.Combina(molt);
        if (danno <= 0f) return;
        float dannoFinale = CalcoloDanno.Calcola(danno, statistiche, b.Statistiche, out bool critico, Magia.Di(inc.scuola).ignoraArmatura);
        b.RiceviColpo(dannoFinale, da, critico);
        giocatore.IncantesimoASegno(dannoFinale);   // amuleto Cuore del lago nero: un po' di mana
        Suoni.Suona(Suono.ImpattoColpo, b.transform.position + Vector3.up, critico ? 1f : 0.8f, critico ? 0.75f : 1.1f);

        if (inc.rallentamento > 0f && inc.effetto == DatiIncantesimo.Effetto.Proiettile)
            EffettiNemico.Applica(b, EffettiNemico.Tipo.Rallenta, inc.rallentamento, DurataEffetto(inc, inc.durataRallentamento), da);
    }

    // Danno nel tempo (scia, gas): niente moltiplicatori, un colpo ogni mezzo secondo.
    public void DannoNelTempo(DatiIncantesimo inc, Bersaglio b, float dannoAlSecondo, float secondi, Vector3 da, bool evocazione = false)
    {
        if (b == null || b.Morto) return;
        float danno = (evocazione ? dannoAlSecondo : Danno(inc, dannoAlSecondo)) * secondi;
        if (danno <= 0f) return;
        float dannoFinale = CalcoloDanno.Calcola(danno, evocazione ? null : statistiche, b.Statistiche, out bool critico, Magia.Di(inc.scuola).ignoraArmatura);
        b.RiceviColpo(dannoFinale, da, critico);
    }

    void Esplosione(DatiIncantesimo inc, Vector3 punto, float molt)
    {
        ZonaMagica.Lampo(punto, inc.raggio, inc.colore);
        foreach (var b in NemiciVicini(punto, inc.raggio)) ColpisciNemico(inc, b, punto, molt, true);
    }

    // Onda: davanti al personaggio, fino a "Lunghezza" metri e larga "Raggio". I nemici più bassi dell'onda vengono
    // spinti indietro e storditi; quelli più alti solo rallentati, con il 20% di danno in meno per ogni metro in più.
    void Onda(DatiIncantesimo inc, float molt)
    {
        Vector3 avanti = transform.forward;
        avanti.y = 0f;
        avanti.Normalize();
        Vector3 centro = transform.position + avanti * (inc.lunghezza * 0.5f);
        ZonaMagica.CreaOnda(this, inc, transform.position, avanti);
        var giaColpiti = new HashSet<Bersaglio>();
        var meta = new Vector3(Mathf.Max(0.5f, inc.raggio) * 0.5f, 6f, inc.lunghezza * 0.5f);
        foreach (Collider c in Physics.OverlapBox(centro, meta, Quaternion.LookRotation(avanti), ~0, QueryTriggerInteraction.Ignore))
        {
            Bersaglio b = c.GetComponentInParent<Bersaglio>();
            if (b == null || b.Morto || !giaColpiti.Add(b)) continue;
            Collider corpo = b.GetComponent<Collider>();
            float altezza = corpo != null ? corpo.bounds.size.y : 2f;
            if (altezza <= inc.altezza)
            {
                ColpisciNemico(inc, b, transform.position, molt, true);
                EffettiNemico.Applica(b, EffettiNemico.Tipo.Spingi, inc.spinta, 0f, transform.position);
                EffettiNemico.Applica(b, EffettiNemico.Tipo.Stordisci, 0f, DurataEffetto(inc, inc.stordimento), transform.position);
            }
            else
            {
                float riduzione = Mathf.Clamp01(1f - 0.2f * (altezza - inc.altezza));
                ColpisciNemico(inc, b, transform.position, molt, true, inc.danno * riduzione);
                EffettiNemico.Applica(b, EffettiNemico.Tipo.Rallenta, inc.rallentamento, DurataEffetto(inc, inc.durataRallentamento), transform.position);
            }
        }
    }

    // Pozza nera: dove si mira (il nemico scelto, oppure qualche metro davanti). Chi è dentro resta bloccato,
    // poi rallenta.
    void Pozza(DatiIncantesimo inc, Bersaglio obiettivo, float molt)
    {
        Vector3 punto = obiettivo != null ? obiettivo.transform.position : transform.position + transform.forward * 6f;
        if (Physics.Raycast(punto + Vector3.up * 2f, Vector3.down, out RaycastHit terra, 10f, ~0, QueryTriggerInteraction.Ignore)) punto = terra.point;
        float blocco = DurataEffetto(inc, inc.blocco);
        ZonaMagica.CreaPozza(this, inc, punto, blocco);
        var presi = NemiciVicini(punto, inc.raggio);
        foreach (var b in presi)
        {
            ColpisciNemico(inc, b, punto, molt, true);
            EffettiNemico.Applica(b, EffettiNemico.Tipo.Blocca, 0f, blocco, punto);
        }
        if (inc.rallentamentoDopo > 0f) StartCoroutine(RallentaDopo(inc, presi, blocco, punto));
    }

    IEnumerator RallentaDopo(DatiIncantesimo inc, List<Bersaglio> presi, float attesa, Vector3 punto)
    {
        yield return new WaitForSeconds(attesa);
        foreach (var b in presi)
            if (b != null && !b.Morto) EffettiNemico.Applica(b, EffettiNemico.Tipo.Rallenta, inc.rallentamentoDopo, DurataEffetto(inc, inc.durataRallentamentoDopo), punto);
    }

    public static List<Bersaglio> NemiciVicini(Vector3 punto, float raggio)
    {
        var trovati = new List<Bersaglio>();
        foreach (Collider c in Physics.OverlapSphere(punto, Mathf.Max(0.1f, raggio), ~0, QueryTriggerInteraction.Ignore))
        {
            Bersaglio b = c.GetComponentInParent<Bersaglio>();
            if (b != null && !b.Morto && !trovati.Contains(b)) trovati.Add(b);
        }
        return trovati;
    }

    // ---------- evocazioni ----------

    void Evoca(DatiIncantesimo inc, Vector3 partenza)
    {
        if (!evocazioni.TryGetValue(inc, out var lista)) evocazioni[inc] = lista = new List<Evocazione>();
        lista.RemoveAll(e => e == null);
        if (lista.Count >= MassimoEvocazioni(inc)) return;

        Vector3 posto = transform.position + transform.forward * 1.5f;
        Evocazione nuova;
        switch (inc.effetto)
        {
            case DatiIncantesimo.Effetto.FuocoFatuo: nuova = FuocoFatuo.Crea(this, inc, posto); break;
            case DatiIncantesimo.Effetto.Lupo: nuova = SpiritoLupo.Crea(this, inc, posto); break;
            default: nuova = BambolaOssa.Crea(this, inc, posto); break;
        }
        lista.Add(nuova);
        // L'attesa parte quando questa evocazione finisce o muore.
        nuova.Finita += () =>
        {
            prontoDa[inc] = Time.time + Attesa(inc);
            if (evocazioni.TryGetValue(inc, out var l)) l.Remove(nuova);
        };
    }

    void OnDisable()
    {
        // Lo Stregone sparisce (cambio scena o classe): le sue evocazioni con lui.
        foreach (var lista in evocazioni.Values)
            foreach (var e in lista)
                if (e != null) e.Termina(false);
        evocazioni.Clear();
    }
}

// La regola dei moltiplicatori di danno (decisa da Lorenzo, vale per tutte le classi): se se ne sommano più di uno,
// quelli da ×2 in su perdono la parte dopo la virgola, poi si moltiplicano; quelli sotto ×2 restano come sono.
// Esempio: ×2 e ×2,5 → ×2 per ×2 = ×4. Da solo un ×2,5 resta ×2,5.
public static class Moltiplicatori
{
    public static float Combina(IList<float> valori)
    {
        int quanti = 0;
        foreach (float v in valori) if (!Mathf.Approximately(v, 1f)) quanti++;
        float totale = 1f;
        foreach (float v in valori)
            totale *= quanti > 1 && v >= 2f ? Mathf.Floor(v) : v;
        return totale;
    }
}
