using System.Collections.Generic;
using UnityEngine;

// Suono dei passi del giocatore, diverso in base al pavimento.
// A cosa serve: mentre il giocatore cammina o corre, a ogni passo suona un passo sull'erba, sulla
// terra del sentiero, sulla pietra o sul legno, a seconda di cosa ha sotto i piedi. Correndo i passi
// sono più fitti e un po' più forti. Non suona durante la schivata (ha il suo suono) né da morto.
// Come capisce il pavimento, in quest'ordine:
//   1. una zona con Superficie Sonora (trigger) in cui si trova;
//   2. un pavimento piatto senza collider sotto i piedi (le lastre del sentiero, il pavimento della cripta),
//      riconosciuto dal materiale;
//   3. il collider sotto i piedi: Superficie Sonora se ce l'ha, altrimenti il suo materiale.
// Come montarlo: non serve montarlo, GiocatoreControllo lo aggiunge da solo al giocatore.
public class PassiSonori : MonoBehaviour
{
    [Tooltip("Metri tra un passo e l'altro (uguale al passo dell'animazione).")]
    [SerializeField] float lunghezzaPasso = 0.9f;
    [SerializeField] float volume = 0.55f;
    [Tooltip("Sopra questa velocità (metri al secondo) i passi suonano più forti, come di corsa.")]
    [SerializeField] float velocitaCorsa = 6f;

    const float VelocitaTeletrasporto = 30f;

    GiocatoreControllo giocatore;
    CharacterController controller;
    Vector3 ultimaPosizione;
    float percorso;

    // Pavimenti piatti senza collider, con il loro tipo (cercati una volta all'inizio).
    readonly List<Renderer> pavimenti = new List<Renderer>();
    readonly List<SuperficieSonora.Tipo> tipiPavimenti = new List<SuperficieSonora.Tipo>();

    void Start()
    {
        giocatore = GetComponent<GiocatoreControllo>();
        controller = GetComponent<CharacterController>();
        ultimaPosizione = transform.position;

        foreach (MeshRenderer r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if (r.GetComponent<Collider>() != null || r.bounds.size.y > 0.25f) continue;
            SuperficieSonora.Tipo tipo = SuperficieSonora.DaMateriale(r.sharedMaterial, out bool trovato);
            if (!trovato) continue;
            pavimenti.Add(r);
            tipiPavimenti.Add(tipo);
        }
    }

    void Update()
    {
        Vector3 spostamento = transform.position - ultimaPosizione;
        spostamento.y = 0f;
        ultimaPosizione = transform.position;

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float distanza = spostamento.magnitude;
        if (distanza / dt > VelocitaTeletrasporto) distanza = 0f;

        bool aTerra = controller == null || controller.isGrounded;
        bool silenzio = giocatore != null &&
            (giocatore.StatoAttuale == GiocatoreControllo.Stato.Schivata || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto);
        if (!aTerra || silenzio || distanza <= 0f) return;

        percorso += distanza;
        if (percorso < lunghezzaPasso) return;
        percorso -= lunghezzaPasso;

        float velocita = distanza / dt;
        float forza = velocita > velocitaCorsa ? 1.25f : 1f;
        Vector3 piedi = transform.position + Vector3.down * 0.9f;
        Suoni.Suona(SuperficieSonora.SuonoPasso(TipoSottoIPiedi(piedi)), piedi, volume * forza);
    }

    SuperficieSonora.Tipo TipoSottoIPiedi(Vector3 piedi)
    {
        // 1. Zone con Superficie Sonora.
        foreach (Collider c in Physics.OverlapSphere(piedi, 0.3f, ~0, QueryTriggerInteraction.Collide))
        {
            if (!c.isTrigger) continue;
            SuperficieSonora zona = c.GetComponent<SuperficieSonora>();
            if (zona != null) return zona.tipo;
        }

        // 2. Pavimenti piatti senza collider proprio sotto i piedi.
        for (int i = 0; i < pavimenti.Count; i++)
        {
            if (pavimenti[i] == null) continue;
            Bounds b = pavimenti[i].bounds;
            if (piedi.x >= b.min.x && piedi.x <= b.max.x && piedi.z >= b.min.z && piedi.z <= b.max.z
                && piedi.y >= b.min.y - 0.4f && piedi.y <= b.max.y + 0.4f)
                return tipiPavimenti[i];
        }

        // 3. Il collider sotto i piedi.
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit colpo, 2f, ~0, QueryTriggerInteraction.Ignore))
        {
            SuperficieSonora superficie = colpo.collider.GetComponentInParent<SuperficieSonora>();
            if (superficie != null) return superficie.tipo;
            Renderer aspetto = colpo.collider.GetComponent<Renderer>();
            if (aspetto != null) return SuperficieSonora.DaMateriale(aspetto.sharedMaterial, out bool _);
        }
        return SuperficieSonora.Tipo.Erba;
    }
}
