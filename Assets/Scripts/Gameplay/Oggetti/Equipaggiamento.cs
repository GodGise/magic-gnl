using UnityEngine;

// Quello che il giocatore ha addosso: un'arma, uno scudo, un'armatura e un amuleto.
// A cosa serve: applica gli oggetti al personaggio. Arma e scudo cambiano i numeri del colpo e della parata
// in Giocatore Controllo; armatura, amuleto e critico dell'arma diventano Modificatori delle Statistiche;
// il peso di scudo e armatura rallenta e rende la schivata più cara; l'effetto degli amuleti arcani lo
// applica Giocatore Controllo.
// Regola delle due mani: equipaggiando lo spadone lo scudo si toglie; equipaggiando uno scudo mentre si
// ha lo spadone, si toglie lo spadone.
// L'inventario (lo farà Giuseppe) chiamerà Equipaggia(oggetto) e i metodi Togli...; l'evento Cambiato avvisa
// quando l'equipaggiamento cambia, per ridisegnare i menu.
// Come montarlo: sul Giocatore. Se manca lo aggiunge da solo Giocatore Controllo.
// Per provare un oggetto senza inventario: trascinarlo nella casella giusta qui nell'Inspector (anche
// durante il Play: viene applicato subito).
public class Equipaggiamento : MonoBehaviour
{
    [SerializeField] DatiArma arma;
    [SerializeField] DatiScudo scudo;
    [SerializeField] DatiArmatura armatura;
    [SerializeField] DatiAmuleto amuleto;

    public DatiArma Arma => arma;
    public DatiScudo Scudo => scudo;
    public DatiArmatura Armatura => armatura;
    public DatiAmuleto Amuleto => amuleto;
    public event System.Action Cambiato;

    GiocatoreControllo giocatore;
    Statistiche statistiche;
    Statistiche.Modificatore modArma, modArmatura, modAmuleto;

    void Awake()
    {
        giocatore = GetComponent<GiocatoreControllo>();
        statistiche = Statistiche.Di(this);
    }

    void Start() => Applica();

    // Durante il Play, cambiando una casella nell'Inspector l'oggetto si applica subito.
    void OnValidate()
    {
        if (Application.isPlaying && statistiche != null) Applica();
    }

    // Mette addosso un oggetto nella casella giusta (al posto di quello che c'era).
    public void Equipaggia(DatiOggetto oggetto)
    {
        switch (oggetto)
        {
            case DatiArma a:
                arma = a;
                if (a.dueMani) scudo = null;
                break;
            case DatiScudo s:
                scudo = s;
                if (arma != null && arma.dueMani) arma = null;
                break;
            case DatiArmatura b: armatura = b; break;
            case DatiAmuleto c: amuleto = c; break;
            default: return;
        }
        Applica();
    }

    public void TogliArma() { arma = null; Applica(); }
    public void TogliScudo() { scudo = null; Applica(); }
    public void TogliArmatura() { armatura = null; Applica(); }
    public void TogliAmuleto() { amuleto = null; Applica(); }

    void Applica()
    {
        // Con lo spadone lo scudo non conta (se è rimasto nella casella dall'Inspector).
        DatiScudo scudoUsato = arma != null && arma.dueMani ? null : scudo;
        if (scudo != null && scudoUsato == null)
            Debug.LogWarning("Equipaggiamento: " + arma.Nome + " è a due mani, lo scudo " + scudo.Nome + " non viene usato.");

        Sostituisci(ref modArma, arma != null ? arma.ModificatoreCritico() : null);
        Sostituisci(ref modArmatura, armatura != null ? armatura.Modificatore() : null);
        Sostituisci(ref modAmuleto, amuleto != null ? amuleto.Modificatore() : null);

        if (giocatore != null) giocatore.AggiornaEquipaggiamento(arma, scudoUsato, armatura, amuleto);
        Cambiato?.Invoke();
    }

    void Sostituisci(ref Statistiche.Modificatore vecchio, Statistiche.Modificatore nuovo)
    {
        statistiche.TogliModificatore(vecchio);
        statistiche.AggiungiModificatore(nuovo);
        vecchio = nuovo;
    }
}
