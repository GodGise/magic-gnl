using System.Collections.Generic;
using UnityEngine;

// Chi può essere inseguito e attaccato dai nemici: il giocatore di questo PC (GiocatoreControllo) e, in co-op,
// gli altri giocatori collegati (GiocatoreRete, solo sul PC che ospita la partita, dove i nemici "pensano").
// A cosa serve: Bersaglio e InseguimentoNemico non cercano più "il" giocatore, ma il più vicino fra quelli
// di questo elenco. Da soli c'è un solo giocatore e tutto funziona come prima.
// Come montarlo: non si monta. GiocatoreControllo e GiocatoreRete si iscrivono da soli.
public interface IObiettivoNemico
{
    Transform Corpo { get; }
    bool Abbattuto { get; }        // morto: i nemici lo lasciano stare
    bool Invisibile { get; }       // svanito nell'ombra (amuleto Ultimo respiro)
    float Furtivita { get; }       // accorcia la vista dei nemici (vedi Statistiche)
    // Il colpo di un nemico arriva: il danno lo calcola chi lo riceve, con la sua armatura (vedi CalcoloDanno).
    void ColpitoDaNemico(Bersaglio nemico);
    // Un nemico colpito da lui è morto (con il bastone ridà mana, con certi amuleti ridà vita).
    void NemicoSconfitto();
}

// Un obiettivo che attira i nemici vicini anche se non lo vedono (la Bambola di ossa dello Stregone).
public interface IEscaNemici : IObiettivoNemico
{
    float RaggioAttrazione { get; }
}

public static class ObiettiviNemici
{
    static readonly List<IObiettivoNemico> tutti = new List<IObiettivoNemico>();

    public static IReadOnlyList<IObiettivoNemico> Tutti => tutti;

    // Il giocatore di questo PC (quello comandato da tastiera e pad).
    public static GiocatoreControllo Locale { get; private set; }

    public static void Iscrivi(IObiettivoNemico chi)
    {
        if (chi != null && !tutti.Contains(chi)) tutti.Add(chi);
        if (chi is GiocatoreControllo g) Locale = g;
    }

    public static void Togli(IObiettivoNemico chi)
    {
        tutti.Remove(chi);
        if (ReferenceEquals(chi, Locale)) Locale = null;
    }

    // Vero se il nemico può prenderlo di mira adesso.
    public static bool Valido(IObiettivoNemico chi) =>
        Esiste(chi) && chi.Corpo.gameObject.activeInHierarchy && !chi.Abbattuto && !chi.Invisibile;

    // Un componente distrutto non è "null" per C# ma lo è per Unity: qui si controllano tutti e due.
    public static bool Esiste(IObiettivoNemico chi) => chi != null && !(chi is Object u && u == null);

    // L'esca (Bambola di ossa) più vicina che attira chi sta in "da", se c'è. A pari distanza ne sceglie una sola.
    public static IObiettivoNemico Esca(Vector3 da)
    {
        IObiettivoNemico migliore = null;
        float minima = float.MaxValue;
        for (int i = tutti.Count - 1; i >= 0; i--)
        {
            if (!(tutti[i] is IEscaNemici esca) || !Valido(esca)) continue;
            float d = (esca.Corpo.position - da).sqrMagnitude;
            if (d > esca.RaggioAttrazione * esca.RaggioAttrazione || d >= minima) continue;
            minima = d;
            migliore = esca;
        }
        return migliore;
    }

    // Il più vicino a "da" fra quelli validi (o null se non ce n'è nessuno). Un'esca vicina viene prima di tutti.
    public static IObiettivoNemico PiuVicino(Vector3 da)
    {
        var esca = Esca(da);
        if (esca != null) return esca;
        IObiettivoNemico migliore = null;
        float minima = float.MaxValue;
        for (int i = tutti.Count - 1; i >= 0; i--)
        {
            var chi = tutti[i];
            if (!Esiste(chi)) { tutti.RemoveAt(i); continue; }
            if (!Valido(chi)) continue;
            float d = (chi.Corpo.position - da).sqrMagnitude;
            if (d < minima) { minima = d; migliore = chi; }
        }
        return migliore;
    }

    // Vero se il collider appartiene a un giocatore (il nemico non lo considera un muro).
    public static bool EGiocatore(Collider c) => c != null && c.GetComponentInParent<GiocatoreControllo>() != null;
}

// Quello che serve ad AnimazioneUmanoide per muovere la figura di un giocatore: lo danno GiocatoreControllo
// (il giocatore di questo PC) e GiocatoreRete (la figura degli altri giocatori in co-op).
public interface IPersonaggioAnimato
{
    GiocatoreControllo.Stato StatoAttuale { get; }
    float TempoNelloStato { get; }
    int ColpoCombo { get; }
    float DurataPreparazioneAttacco { get; }
    float DurataColpoAttivo { get; }
    float DurataRecuperoAttacco { get; }
    float DurataSchivata { get; }
    Vector3 DirezioneSchivata { get; }
    bool AttaccoMagico { get; }
    float DurataPreparazioneIncantesimo { get; }
    float DurataRecuperoIncantesimo { get; }
    float MomentoTaglio { get; }
    float DurataEsecuzione { get; }
    float Strettoia { get; }
    bool ArmaNelFodero { get; }
    float TempoGestoFodero { get; }
    float DurataGestoFodero { get; }
}
