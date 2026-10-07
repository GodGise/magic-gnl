using Unity.Netcode;
using UnityEngine;

// La "sagoma di rete" di un giocatore: quello che gli altri giocatori vedono di lui.
// A cosa serve: il giocatore vero (GiocatoreControllo) resta locale e non cambia; questo oggetto di rete ne copia
// posizione e direzione e le spedisce agli altri. Sul PC del proprietario è invisibile (c'è già il personaggio vero);
// sugli altri PC è una capsula colorata che si muove in modo fluido.
// Come montarlo: non si monta a mano. Sta nel prefab Resources/Rete/GiocatoreRete (creato dall'editor, vedi
// Assets/Editor/CreaPrefabRete.cs) e lo crea ReteCoop per ogni giocatore che entra.
// Nelle fasi successive (combattimento, nemici) questa sagoma potrà portare anche vita, animazione e arma.
public class GiocatoreRete : NetworkBehaviour
{
    // Il proprietario scrive, tutti leggono.
    readonly NetworkVariable<Vector3> posizione = new NetworkVariable<Vector3>(
        Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<float> direzione = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Tooltip("Quanto in fretta la sagoma raggiunge la posizione ricevuta. Più è alto, meno ritardo (ma più scatti).")]
    [SerializeField] float morbidezza = 15f;

    static readonly Color[] colori =
    {
        new Color(0.85f, 0.35f, 0.3f), new Color(0.3f, 0.6f, 0.9f), new Color(0.4f, 0.8f, 0.4f),
    };

    // Quanti giocatori ci sono in partita (le sagome create), per il pannello di ReteCoop.
    public static int Presenti { get; private set; }

    GiocatoreControllo locale;
    GameObject aspetto;
    bool primoAggiornamento = true;

    public override void OnNetworkSpawn()
    {
        Presenti++;
        if (IsOwner)
        {
            transform.position = TrovaLocale() != null ? locale.transform.position : Vector3.zero;
            return;
        }
        CreaAspetto();
        transform.position = posizione.Value;
    }

    public override void OnNetworkDespawn()
    {
        Presenti = Mathf.Max(0, Presenti - 1);
        if (aspetto != null) Destroy(aspetto);
    }

    GiocatoreControllo TrovaLocale()
    {
        if (locale == null) locale = FindFirstObjectByType<GiocatoreControllo>();
        return locale;
    }

    void CreaAspetto()
    {
        // Capsula provvisoria (alta 1,8 m) senza collisioni: da sostituire con il modello vero del personaggio.
        aspetto = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        aspetto.name = "SagomaGiocatore" + OwnerClientId;
        var collisore = aspetto.GetComponent<Collider>();
        if (collisore != null) Destroy(collisore);
        aspetto.transform.SetParent(transform, false);
        aspetto.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        var disegno = aspetto.GetComponent<Renderer>();
        if (disegno != null)
        {
            disegno.material = new Material(disegno.sharedMaterial);
            disegno.material.color = colori[(int)(OwnerClientId % (ulong)colori.Length)];
        }
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (IsOwner)
        {
            // Copia il personaggio vero. Se la scena cambia, lo cerca di nuovo.
            if (TrovaLocale() == null) return;
            transform.SetPositionAndRotation(locale.transform.position, Quaternion.Euler(0f, locale.transform.eulerAngles.y, 0f));
            posizione.Value = transform.position;
            direzione.Value = transform.eulerAngles.y;
            return;
        }

        // Appena creata la sagoma, o se l'altro si teletrasporta (più di 20 m), salta subito nel punto giusto.
        if (primoAggiornamento || (transform.position - posizione.Value).sqrMagnitude > 400f)
        {
            transform.SetPositionAndRotation(posizione.Value, Quaternion.Euler(0f, direzione.Value, 0f));
            primoAggiornamento = false;
            return;
        }
        float t = 1f - Mathf.Exp(-morbidezza * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, posizione.Value, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, direzione.Value, 0f), t);
    }
}
