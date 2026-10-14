using System.Collections.Generic;
using UnityEngine;

// Un personaggio che segue il giocatore (Tobin che va con il papà al capanno).
// A cosa serve: compagni che camminano dietro al giocatore durante una quest, senza combattere.
// Come funziona: se il giocatore si allontana più di "Distanza", il personaggio gli va dietro (corre se è lontano);
// resta appoggiato al terreno e si gira dove cammina. Se fra lui e il giocatore c'è un muro, rifà la strada del
// giocatore (le sue "orme", lasciate ogni mezzo metro): così passa dalle porte invece di attraversare i muri.
// Se resta troppo indietro (per esempio dopo un cambio di zona) ricompare vicino al giocatore.
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

    const float PassoOrme = 0.5f;
    const int OrmeMassime = 80;

    GiocatoreControllo giocatore;
    readonly List<Vector3> orme = new List<Vector3>();

    void OnEnable() => orme.Clear();

    void Update()
    {
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null || Dialogo.InCorso) return;

        Vector3 pos = transform.position;
        Vector3 meta = giocatore.transform.position;
        LasciaOrma(meta);

        Vector3 verso = meta - pos;
        verso.y = 0f;
        float lontano = verso.magnitude;

        if (lontano > distanzaMassima)
        {
            transform.position = Appoggia(meta - giocatore.transform.forward * distanza);
            orme.Clear();
            return;
        }
        if (lontano <= distanza) return;

        // Strada: dritto verso il giocatore se lo vede, altrimenti la prima orma ancora da raggiungere.
        Vector3 dove = meta;
        if (!Vede(pos, meta))
        {
            while (orme.Count > 1 && Piatto(orme[0] - pos).magnitude < 0.35f) orme.RemoveAt(0);
            if (orme.Count > 0) dove = orme[0];
        }
        else orme.Clear();

        Vector3 direzione = Piatto(dove - pos);
        float corsa = lontano > distanza * 3f ? velocitaCorsa : velocitaPasso;
        float passo = Mathf.Min(corsa * Time.deltaTime, dove == meta ? lontano - distanza : direzione.magnitude);
        if (direzione.sqrMagnitude > 0.0001f)
        {
            transform.position = Appoggia(pos + direzione.normalized * passo);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direzione.normalized), 10f * Time.deltaTime);
        }
    }

    void LasciaOrma(Vector3 piedi)
    {
        if (orme.Count > 0 && Piatto(piedi - orme[orme.Count - 1]).magnitude < PassoOrme) return;
        orme.Add(piedi);
        if (orme.Count > OrmeMassime) orme.RemoveAt(0);
    }

    // Vero se fra il personaggio e il giocatore non c'è niente di solido (all'altezza del petto).
    bool Vede(Vector3 da, Vector3 a)
    {
        Vector3 inizio = da + Vector3.up * 0.2f, fine = a + Vector3.up * 0.5f;
        foreach (var colpo in Physics.RaycastAll(inizio, fine - inizio, Vector3.Distance(inizio, fine), ~0, QueryTriggerInteraction.Ignore))
        {
            if (colpo.transform.IsChildOf(transform) || colpo.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            return false;
        }
        return true;
    }

    static Vector3 Piatto(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // Tiene il personaggio sul terreno (o sul pavimento della casa) guardando in basso da poco sopra la testa
    // (non più in alto, se no sotto una tettoia finirebbe sul tetto).
    Vector3 Appoggia(Vector3 punto)
    {
        Vector3 inizio = new Vector3(punto.x, transform.position.y + altezzaDaTerra + 0.6f, punto.z);
        var colpi = Physics.RaycastAll(inizio, Vector3.down, altezzaDaTerra * 2f + 2.5f, ~0, QueryTriggerInteraction.Ignore);
        float migliore = float.MaxValue;
        float y = transform.position.y;
        foreach (var c in colpi)
        {
            if (c.transform.IsChildOf(transform) || c.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            if (c.distance < migliore) { migliore = c.distance; y = c.point.y + altezzaDaTerra; }
        }
        punto.y = y;
        return punto;
    }
}
