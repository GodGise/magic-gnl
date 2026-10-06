using UnityEngine;

// Quello che il giocatore ha addosso: un'arma, un'armatura e un amuleto.
// A cosa serve: applica gli oggetti al personaggio. L'arma cambia i numeri del colpo e della parata in
// Giocatore Controllo; armatura, amuleto e critico dell'arma diventano Modificatori delle Statistiche.
// L'inventario (lo farà Giuseppe) chiamerà Equipaggia(oggetto) e Togli(...); l'evento Cambiato avvisa
// quando l'equipaggiamento cambia, per ridisegnare i menu.
// Come montarlo: sul Giocatore. Se manca lo aggiunge da solo Giocatore Controllo.
// Per provare un oggetto senza inventario: trascinarlo nella casella giusta qui nell'Inspector (anche
// durante il Play: viene applicato subito).
public class Equipaggiamento : MonoBehaviour
{
    [SerializeField] DatiArma arma;
    [SerializeField] DatiArmatura armatura;
    [SerializeField] DatiAmuleto amuleto;

    public DatiArma Arma => arma;
    public DatiArmatura Armatura => armatura;
    public DatiAmuleto Amuleto => amuleto;
    public event System.Action Cambiato;

    GiocatoreControllo giocatore;
    Statistiche statistiche;
    Statistiche.Modificatore modArma, modArmatura, modAmuleto;
    DatiArma armaApplicata;
    DatiArmatura armaturaApplicata;
    DatiAmuleto amuletoApplicato;

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
            case DatiArma a: arma = a; break;
            case DatiArmatura b: armatura = b; break;
            case DatiAmuleto c: amuleto = c; break;
            default: return;
        }
        Applica();
    }

    public void TogliArma() { arma = null; Applica(); }
    public void TogliArmatura() { armatura = null; Applica(); }
    public void TogliAmuleto() { amuleto = null; Applica(); }

    void Applica()
    {
        if (arma != armaApplicata)
        {
            statistiche.TogliModificatore(modArma);
            modArma = arma != null ? arma.ModificatoreCritico() : null;
            statistiche.AggiungiModificatore(modArma);
            if (giocatore != null) giocatore.ImpostaArma(arma);
            armaApplicata = arma;
        }
        if (armatura != armaturaApplicata)
        {
            statistiche.TogliModificatore(modArmatura);
            modArmatura = armatura != null ? armatura.Modificatore() : null;
            statistiche.AggiungiModificatore(modArmatura);
            if (giocatore != null) giocatore.ImpostaArmatura(armatura);
            armaturaApplicata = armatura;
        }
        if (amuleto != amuletoApplicato)
        {
            statistiche.TogliModificatore(modAmuleto);
            modAmuleto = amuleto != null ? amuleto.Modificatore() : null;
            statistiche.AggiungiModificatore(modAmuleto);
            amuletoApplicato = amuleto;
        }
        Cambiato?.Invoke();
    }
}
