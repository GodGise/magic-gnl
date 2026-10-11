using UnityEngine;

// Un personaggio che segue il giocatore (Tobin che va con il papà al capanno).
// A cosa serve: compagni che camminano dietro al giocatore durante una quest, senza combattere.
// Come funziona: se il giocatore si allontana più di "Distanza", il personaggio gli va dietro (corre se è lontano);
// resta appoggiato al terreno e si gira dove cammina. Se resta troppo indietro (per esempio dopo un cambio di zona)
// ricompare vicino al giocatore. Non passa attraverso i muri in modo intelligente: per le prove va bene, i percorsi
// stretti si sistemano più avanti.
// Come montarlo: sul personaggio. Si lascia il componente SPENTO e lo si accende da un evento quando deve iniziare a
// seguire (nell'evento: trascina il personaggio e scegli SeguiGiocatore > enabled); spegnendolo si ferma dov'è.
public class SeguiGiocatore : MonoBehaviour
{
    [Tooltip("Distanza a cui resta dal giocatore, in metri.")]
    [SerializeField] float distanza = 2.2f;
    [SerializeField] float velocitaPasso = 3f;
    [SerializeField] float velocitaCorsa = 6.5f;
    [Tooltip("Oltre questa distanza ricompare vicino al giocatore.")]
    [SerializeField] float distanzaMassima = 30f;
    [Tooltip("Altezza da terra del centro del personaggio (metà della sua altezza).")]
    [SerializeField] float altezzaDaTerra = 0.6f;

    GiocatoreControllo giocatore;

    void Update()
    {
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null || Dialogo.InCorso) return;

        Vector3 pos = transform.position;
        Vector3 meta = giocatore.transform.position;
        Vector3 verso = meta - pos;
        verso.y = 0f;
        float lontano = verso.magnitude;

        if (lontano > distanzaMassima)
        {
            Vector3 dietro = meta - giocatore.transform.forward * distanza;
            transform.position = Appoggia(dietro);
            return;
        }
        if (lontano > distanza)
        {
            float velocita = lontano > distanza * 3f ? velocitaCorsa : velocitaPasso;
            Vector3 passo = verso.normalized * Mathf.Min(velocita * Time.deltaTime, lontano - distanza);
            transform.position = Appoggia(pos + passo);
        }
        if (lontano > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(verso.normalized), 10f * Time.deltaTime);
    }

    // Tiene il personaggio sul terreno (o sul pavimento della casa) guardando in basso da sopra la testa.
    Vector3 Appoggia(Vector3 punto)
    {
        var colpi = Physics.RaycastAll(punto + Vector3.up * 2.5f, Vector3.down, 6f, ~0, QueryTriggerInteraction.Ignore);
        float migliore = float.MaxValue;
        float y = punto.y;
        foreach (var c in colpi)
        {
            if (c.transform.IsChildOf(transform) || c.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            if (c.distance < migliore) { migliore = c.distance; y = c.point.y + altezzaDaTerra; }
        }
        punto.y = y;
        return punto;
    }
}
