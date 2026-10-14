using UnityEngine;

// Lo scontro con l'orco enorme della piazza, scritto per essere perso (decisione di Lorenzo, 11 ottobre).
// Regole: è uno scontro normale, ma dopo "Secondi Prima Dell'Invulnerabilità" (3 minuti) dall'inizio l'orco non
// prende più danni né effetti degli incantesimi (Bersaglio.Invulnerabile): i colpi rimbalzano. Non muore mai: se la
// sua vita scende sotto "Vita Minima" diventa invulnerabile subito. Quando il giocatore muore, RazziaVillaggio porta
// al risveglio nella gattabuia.
// Lo scontro comincia la prima volta che l'orco insegue il giocatore o viene colpito.
// Come montarlo: non serve, lo aggiunge RazziaVillaggio all'orco enorme quando atterra in piazza. Per cambiare i
// numeri si può mettere a mano sul prefab dell'orco enorme.
[RequireComponent(typeof(Bersaglio))]
public class ScontroOrcoEnorme : MonoBehaviour
{
    [SerializeField, Min(0f)] float secondiPrimaDellInvulnerabilita = 180f;
    [Tooltip("Sotto questa parte della vita (0,25 = un quarto) diventa invulnerabile subito: non muore mai.")]
    [SerializeField, Range(0f, 1f)] float vitaMinima = 0.25f;

    Bersaglio bersaglio;
    InseguimentoNemico vista;
    float inizio = -1f;

    public bool Iniziato => inizio >= 0f;

    void Awake()
    {
        bersaglio = GetComponent<Bersaglio>();
        vista = GetComponent<InseguimentoNemico>();
        bersaglio.VitaNonSotto = 1f;   // anche un colpo fortissimo lo lascia vivo: questo scontro si perde
    }

    void Update()
    {
        if (bersaglio.Morto || bersaglio.Invulnerabile) return;
        if (inizio < 0f && ((vista != null && vista.StaInseguendo) || bersaglio.UltimoColpo > 0f)) inizio = Time.time;
        bool tempo = inizio >= 0f && Time.time - inizio >= secondiPrimaDellInvulnerabilita;
        bool quasiMorto = bersaglio.Vita <= bersaglio.VitaMassima * vitaMinima;
        if (!tempo && !quasiMorto) return;
        bersaglio.Invulnerabile = true;
        MessaggiSchermo.Mostra(Lingua.T("msg.razzia.invulnerabile"), 4f);
        Suoni.Suona(Suono.UrloOrco, transform.position + Vector3.up * 2f, 1f, 0.7f);
    }
}
