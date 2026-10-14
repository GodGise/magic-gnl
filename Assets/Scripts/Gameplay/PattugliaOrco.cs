using UnityEngine;

// Un orco di pattuglia (la gattabuia): cammina avanti e indietro fra alcuni punti, si ferma un po' a ogni punto e si
// guarda intorno. Quando vede (o sente) il giocatore lo insegue come gli altri; se lo perde torna al giro.
// Come montarlo: su un orco (Bersaglio + InseguimentoNemico); in "Punti" gli oggetti vuoti del giro, in ordine.
// Lo mette da solo il comando "magic-gnl > Gattabuia: crea la scena della gattabuia".
[RequireComponent(typeof(InseguimentoNemico))]
public class PattugliaOrco : MonoBehaviour
{
    [SerializeField] Transform[] punti;
    [Tooltip("Secondi fermo a ogni punto prima di andare al successivo.")]
    [SerializeField, Min(0f)] float sosta = 3f;

    InseguimentoNemico vista;
    Bersaglio bersaglio;
    int prossimo;
    float partenza;
    bool inViaggio;

    void Awake()
    {
        vista = GetComponent<InseguimentoNemico>();
        bersaglio = GetComponent<Bersaglio>();
        partenza = Time.time + sosta;
    }

    void Update()
    {
        if (punti == null || punti.Length == 0 || bersaglio.Morto || Rete.Ospite) return;
        if (!vista.AlPosto) { inViaggio = false; return; }   // in cammino, o insegue qualcuno
        if (!inViaggio) { inViaggio = true; partenza = Time.time + sosta; }
        if (Time.time < partenza) return;
        var p = punti[prossimo];
        prossimo = (prossimo + 1) % punti.Length;
        if (p == null) return;
        Vector3 avanti = punti[prossimo] != null ? punti[prossimo].position - p.position : transform.forward;
        vista.CambiaPosto(p.position + Vector3.up * 1.1f, avanti);
        vista.VaiAlPosto(false);
        inViaggio = false;
    }
}
