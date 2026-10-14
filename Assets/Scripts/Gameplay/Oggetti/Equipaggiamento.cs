using System.Collections.Generic;
using UnityEngine;

// Quello che il giocatore ha addosso: un'arma, uno scudo, un'armatura, un amuleto e, per il Ladro, un'arma
// a distanza (arco o balestra: i suoi numeri sono pronti, il tiro si farà più avanti).
// Lo Stregone (Docs/incantesimi-stregone.md) ha invece un bastone al posto dell'arma, un libro al posto dello
// scudo, la veste come armatura e le caselle degli incantesimi (4 di base, fino a 6 con certi libri).
// A cosa serve: applica gli oggetti al personaggio. Arma e scudo cambiano i numeri del colpo e della parata
// in Giocatore Controllo; armatura, amuleto, libro e critico dell'arma diventano Modificatori delle Statistiche;
// il peso di scudo, libro e armatura rallenta e rende la schivata più cara; l'effetto degli amuleti arcani lo
// applica Giocatore Controllo. I pro e contro sugli incantesimi (ModificatoriMagia) li somma Magia: MagiaStregone
// li legge da qui.
// Regola delle due mani: equipaggiando lo spadone lo scudo si toglie; equipaggiando uno scudo mentre si
// ha lo spadone, si toglie lo spadone. Lo stesso vale per il Bastone del lago e il libro.
// Incantesimi: al massimo 2 della stessa scuola nelle caselle (regola di Lorenzo). Se le caselle diminuiscono
// (si toglie un libro), gli incantesimi che non ci stanno più tornano nello zaino.
// L'inventario (InventarioGioco) chiama Equipaggia(oggetto), MettiIncantesimo e i metodi Togli...; l'evento
// Cambiato avvisa quando l'equipaggiamento cambia, per ridisegnare i menu.
// Come montarlo: sul Giocatore. Se manca lo aggiunge da solo Giocatore Controllo.
// Per provare un oggetto senza inventario: trascinarlo nella casella giusta qui nell'Inspector (anche
// durante il Play: viene applicato subito).
public class Equipaggiamento : MonoBehaviour
{
    public const int CaselleBase = 4, CaselleMassime = 6, MassimoPerScuola = 2;

    [SerializeField] DatiArma arma;
    [SerializeField] DatiScudo scudo;
    [SerializeField] DatiArmatura armatura;
    [SerializeField] DatiAmuleto amuleto;
    [SerializeField] DatiArmaDistanza armaDistanza;
    [Header("Stregone")]
    [SerializeField] DatiBastone bastone;
    [SerializeField] DatiLibro libro;
    [Tooltip("Le caselle degli incantesimi, in ordine (tasti 1, 2, 3...). Contano solo le prime 4 (più quelle del libro).")]
    [SerializeField] DatiIncantesimo[] incantesimi = new DatiIncantesimo[CaselleMassime];

    public DatiArma Arma => arma;
    public DatiScudo Scudo => scudo;
    public DatiArmatura Armatura => armatura;
    public DatiAmuleto Amuleto => amuleto;
    public DatiArmaDistanza ArmaDistanza => armaDistanza;
    public DatiBastone Bastone => bastone;
    public DatiLibro Libro => libro;
    // Il libro conta solo se il bastone non è a due mani.
    public DatiLibro LibroUsato => bastone != null && bastone.dueMani ? null : libro;
    public event System.Action Cambiato;

    // Pro e contro sugli incantesimi di bastone, libro, veste e amuleto, già sommati.
    public ModificatoriMagia Magia { get; private set; } = new ModificatoriMagia();
    public int NumeroCaselle => Mathf.Clamp(CaselleBase + Magia.caselleExtra, 1, CaselleMassime);
    public DatiIncantesimo Incantesimo(int casella) =>
        casella >= 0 && casella < NumeroCaselle && casella < incantesimi.Length ? incantesimi[casella] : null;

    GiocatoreControllo giocatore;
    Statistiche statistiche;
    Statistiche.Modificatore modArma, modArmatura, modAmuleto, modLibro, modBastone;

    void Awake()
    {
        giocatore = GetComponent<GiocatoreControllo>();
        statistiche = Statistiche.Di(this);
        if (incantesimi == null || incantesimi.Length != CaselleMassime) System.Array.Resize(ref incantesimi, CaselleMassime);
        // Gli oggetti addosso si vedono sulla figura (forme provvisorie, vedi AspettoEquipaggiamento).
        if (GetComponent<AspettoEquipaggiamento>() == null) gameObject.AddComponent<AspettoEquipaggiamento>();
    }

    void Start() => Applica();

    // Durante il Play, cambiando una casella nell'Inspector l'oggetto si applica subito.
    void OnValidate()
    {
        if (incantesimi == null || incantesimi.Length != CaselleMassime) System.Array.Resize(ref incantesimi, CaselleMassime);
        if (Application.isPlaying && statistiche != null) Applica();
    }

    // Mette addosso un oggetto nella casella giusta (al posto di quello che c'era).
    // Un incantesimo va nella prima casella libera (se non ce n'è, non fa niente: vedi MettiIncantesimo).
    public void Equipaggia(DatiOggetto oggetto)
    {
        switch (oggetto)
        {
            case DatiArma a:
                arma = a;
                if (a.dueMani) scudo = null;
                // Lo Stregone con un'arma di tutte le classi la tiene al posto del bastone (casella 0 dell'inventario).
                if (a.tutteLeClassi && SceltaPartita.Classe == ClasseGiocatore.Stregone) bastone = null;
                break;
            case DatiScudo s:
                scudo = s;
                if (arma != null && arma.dueMani) arma = null;
                break;
            case DatiArmatura b: armatura = b; break;
            case DatiAmuleto c: amuleto = c; break;
            case DatiArmaDistanza d: armaDistanza = d; break;
            case DatiBastone st:
                bastone = st;
                if (arma != null && arma.tutteLeClassi && SceltaPartita.Classe == ClasseGiocatore.Stregone) arma = null;
                if (st.dueMani) libro = null;
                break;
            case DatiLibro l:
                libro = l;
                if (bastone != null && bastone.dueMani) bastone = null;
                break;
            case DatiIncantesimo inc:
                int libera = CasellaLibera();
                if (libera >= 0) MettiIncantesimo(libera, inc);
                return;
            default: return;
        }
        Applica();
    }

    public void TogliArma() { arma = null; Applica(); }
    public void TogliScudo() { scudo = null; Applica(); }
    public void TogliArmatura() { armatura = null; Applica(); }
    public void TogliAmuleto() { amuleto = null; Applica(); }
    public void TogliArmaDistanza() { armaDistanza = null; Applica(); }
    public void TogliBastone() { bastone = null; Applica(); }
    public void TogliLibro() { libro = null; Applica(); }

    // ---------- incantesimi ----------

    public int CasellaLibera()
    {
        for (int i = 0; i < NumeroCaselle; i++) if (incantesimi[i] == null) return i;
        return -1;
    }

    public int CasellaDi(DatiIncantesimo inc)
    {
        for (int i = 0; i < NumeroCaselle; i++) if (incantesimi[i] == inc) return i;
        return -1;
    }

    // Vero se l'incantesimo si può mettere in quella casella senza superare 2 incantesimi della stessa scuola
    // (quello che c'è già nella casella viene sostituito, quindi non conta).
    public bool RispettaScuole(int casella, DatiIncantesimo inc)
    {
        if (inc == null) return true;
        int stessi = 0;
        for (int i = 0; i < NumeroCaselle; i++)
            if (i != casella && incantesimi[i] != null && incantesimi[i].scuola == inc.scuola) stessi++;
        return stessi < MassimoPerScuola;
    }

    // Mette un incantesimo in una casella. Restituisce quello che c'era (da rimettere nello zaino), oppure null.
    // Non controlla la regola delle scuole: la controlla l'inventario prima (RispettaScuole), per poter avvisare.
    public DatiIncantesimo MettiIncantesimo(int casella, DatiIncantesimo inc)
    {
        if (casella < 0 || casella >= NumeroCaselle) return null;
        var vecchio = incantesimi[casella];
        incantesimi[casella] = inc;
        Cambiato?.Invoke();
        return vecchio;
    }

    public DatiIncantesimo TogliIncantesimo(int casella) => MettiIncantesimo(casella, null);

    void Applica()
    {
        // Con lo spadone lo scudo non conta (se è rimasto nella casella dall'Inspector).
        DatiScudo scudoUsato = arma != null && arma.dueMani ? null : scudo;
        if (scudo != null && scudoUsato == null)
            Debug.LogWarning("Equipaggiamento: " + arma.Nome + " è a due mani, lo scudo " + scudo.Nome + " non viene usato.");
        DatiLibro libroUsato = LibroUsato;
        if (libro != null && libroUsato == null)
            Debug.LogWarning("Equipaggiamento: " + bastone.Nome + " è a due mani, il libro " + libro.Nome + " non viene usato.");

        Sostituisci(ref modArma, arma != null ? arma.ModificatoreCritico() : null);
        Sostituisci(ref modArmatura, armatura != null ? armatura.Modificatore() : null);
        Sostituisci(ref modAmuleto, amuleto != null ? amuleto.Modificatore() : null);
        Sostituisci(ref modLibro, libroUsato != null ? libroUsato.Modificatore() : null);
        Sostituisci(ref modBastone, bastone != null ? bastone.Modificatore() : null);

        Magia = ModificatoriMagia.Somma(new List<ModificatoriMagia>
        {
            bastone != null ? bastone.magia : null,
            libroUsato != null ? libroUsato.magia : null,
            armatura != null ? armatura.magia : null,
            amuleto != null ? amuleto.magia : null,
        });

        // Meno caselle di prima (libro tolto): gli incantesimi che non ci stanno tornano nello zaino.
        for (int i = NumeroCaselle; i < incantesimi.Length; i++)
        {
            if (incantesimi[i] == null) continue;
            if (Application.isPlaying) Zaino.Di(this).Aggiungi(incantesimi[i]);
            incantesimi[i] = null;
        }

        if (giocatore != null) giocatore.AggiornaEquipaggiamento(arma, scudoUsato, armatura, amuleto, libroUsato);
        Cambiato?.Invoke();
    }

    void Sostituisci(ref Statistiche.Modificatore vecchio, Statistiche.Modificatore nuovo)
    {
        statistiche.TogliModificatore(vecchio);
        statistiche.AggiungiModificatore(nuovo);
        vecchio = nuovo;
    }
}
