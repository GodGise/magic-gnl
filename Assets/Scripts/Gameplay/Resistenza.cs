using UnityEngine;

// Resistenza (stamina) del personaggio.
// A cosa serve: schivata, attacco, sprint e colpi parati consumano resistenza; si ricarica da sola
// dopo una breve pausa, ma non mentre si tiene la parata.
// Come montarlo: aggiungilo allo stesso oggetto del GiocatoreControllo (lo richiede da solo).
public class Resistenza : MonoBehaviour
{
    [SerializeField] float massimo = 100f;
    [SerializeField] float recuperoAlSecondo = 30f;
    [Tooltip("Secondi di attesa dopo l'ultima spesa prima che la resistenza ricominci a salire.")]
    [SerializeField] float ritardoRecupero = 0.8f;

    public float Attuale { get; private set; }
    public float Massimo => massimo;

    // Come in Elden Ring: si può agire finché la barra non è vuota, anche se l'azione la manda a zero.
    public bool HaResistenza => Attuale > 0.01f;

    // Ricarica più veloce o più lenta (1 = normale, 1,3 = +30%): lo cambiano gli amuleti arcani.
    public float MoltiplicatoreRecupero { get; set; } = 1f;

    // Messo a true dal giocatore mentre tiene la parata.
    public bool InPausaRecupero { get; set; }

    float prossimoRecupero;

    void Awake()
    {
        Attuale = massimo;
    }

    void Update()
    {
        if (InPausaRecupero || Time.time < prossimoRecupero) return;
        Attuale = Mathf.Min(massimo, Attuale + recuperoAlSecondo * MoltiplicatoreRecupero * Time.deltaTime);
    }

    public void Spendi(float costo)
    {
        Attuale = Mathf.Max(0f, Attuale - costo);
        prossimoRecupero = Time.time + ritardoRecupero;
    }

    public void Ripristina()
    {
        Attuale = massimo;
    }
}
