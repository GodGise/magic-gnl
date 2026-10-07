using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// La figura di rete di un giocatore: quello che gli altri vedono di lui in co-op.
// A cosa serve: il giocatore vero (GiocatoreControllo) resta sul suo PC e non cambia. Questo oggetto di rete ne copia
// posizione, direzione e azioni (attacco, combo, schivata, parata, colpito, morto, esecuzione, arma, fodero,
// invisibilità) e le spedisce agli altri. Sul PC del proprietario non si vede (c'è già il personaggio vero);
// sugli altri PC è una figura umana a blocchi (AspettoUmanoide), animata come il giocatore vero, con il nome sopra.
// Sul PC dell'host la figura è anche un bersaglio per i nemici (IObiettivoNemico): quando un nemico la colpisce,
// il colpo viene mandato al PC del proprietario, che decide se l'ha parato o schivato e quanto danno prende.
// Come montarlo: non si monta a mano. Sta nel prefab Resources/Rete/GiocatoreRete (creato da Assets/Editor/CreaPrefabRete.cs)
// e lo crea ReteCoop per ogni giocatore che entra.
public class GiocatoreRete : NetworkBehaviour, IObiettivoNemico, IPersonaggioAnimato
{
    // ---------- dati condivisi: il proprietario scrive, tutti leggono ----------
    readonly NetworkVariable<Vector3> posizione = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<float> direzione = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // Stato, colpo della combo, arma, fodero, invisibilità e un contatore delle azioni, tutto in un numero (vedi Impacchetta).
    readonly NetworkVariable<int> azione = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<Vector3> direzioneSchivata = new NetworkVariable<Vector3>(Vector3.back, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // Tempi delle azioni (cambiano con l'arma): preparazione, colpo, recupero, schivata.
    readonly NetworkVariable<Vector4> tempiAttacco = new NetworkVariable<Vector4>(new Vector4(0.25f, 0.15f, 0.35f, 0.45f), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // Preparazione e recupero dell'incantesimo, momento del taglio e durata dell'esecuzione.
    readonly NetworkVariable<Vector4> tempiAltro = new NetworkVariable<Vector4>(new Vector4(0.3f, 0.45f, 0.7f, 1.4f), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<float> strettoia = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<float> furtivita = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // Numero del giocatore (1, 2, 3): lo sceglie l'host quando lo crea.
    readonly NetworkVariable<byte> numero = new NetworkVariable<byte>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Tooltip("Quanto in fretta la figura raggiunge la posizione ricevuta. Più è alto, meno ritardo (ma più scatti).")]
    [SerializeField] float morbidezza = 15f;

    // Colore del corpo di ogni giocatore (1, 2, 3), scuri come il giocatore ma distinguibili.
    static readonly Color[] colori =
    {
        new Color(0.22f, 0.2f, 0.24f), new Color(0.12f, 0.2f, 0.32f), new Color(0.3f, 0.14f, 0.12f),
    };

    public int Numero => numero.Value;
    // Solo sull'host, prima della creazione in rete (vedi ReteCoop): il valore va in rete appena nasce.
    public void ImpostaNumero(int n) => numeroIniziale = (byte)Mathf.Clamp(n, 1, 255);
    byte numeroIniziale = 1;
    // Distanza di lato con cui parte ogni giocatore, per non comparire uno dentro l'altro.
    public static readonly float[] SpostamentoPartenza = { 0f, 1.6f, -1.6f };

    GiocatoreControllo locale;
    GameObject figura;
    TextMesh etichetta;
    bool primoAggiornamento = true;
    bool bastoneAggiunto;
    bool armaBastonePrima;
    bool invisibilePrima;
    readonly List<Renderer> partiFigura = new List<Renderer>();
    readonly List<Color> coloriFigura = new List<Color>();

    // Il proprietario: per capire quando un'azione ricomincia (per esempio il secondo colpo della combo).
    GiocatoreControllo.Stato statoPrima;
    float tempoPrima;
    int contatoreAzioni;

    // Chi riceve: il tempo passato dall'inizio dell'azione, contato qui.
    int ultimoContatore = -1;
    float tempoLocale;
    float inizioGestoFodero = -10f;
    bool foderoPrima;

    // ---------- nascita e fine ----------

    public override void OnNetworkSpawn()
    {
        if (IsServer) numero.Value = numeroIniziale;
        if (IsOwner)
        {
            if (TrovaLocale() != null)
            {
                // Chi non ospita parte un po' di lato rispetto all'host.
                if (!IsServer) locale.SpostaPartenza(locale.transform.right * SpostamentoPartenza[(Numero - 1) % SpostamentoPartenza.Length]);
                transform.position = locale.transform.position;
            }
            return;
        }
        CreaFigura();
        transform.SetPositionAndRotation(posizione.Value, Quaternion.Euler(0f, direzione.Value, 0f));
        // Sul PC dell'host i nemici possono prendere di mira anche questa figura.
        if (IsServer) ObiettiviNemici.Iscrivi(this);
    }

    public override void OnNetworkDespawn()
    {
        ObiettiviNemici.Togli(this);
        if (figura != null) Destroy(figura);
    }

    GiocatoreControllo TrovaLocale()
    {
        if (locale == null) locale = ObiettiviNemici.Locale != null ? ObiettiviNemici.Locale : FindFirstObjectByType<GiocatoreControllo>();
        return locale;
    }

    void CreaFigura()
    {
        // Una capsula come quella del giocatore (alta 2 m, centro all'altezza dell'anca), poi la figura a blocchi sopra.
        figura = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        figura.name = "Figura giocatore " + Numero;
        Destroy(figura.GetComponent<Collider>());
        figura.transform.SetParent(transform, false);
        var disegno = figura.GetComponent<MeshRenderer>();
        disegno.sharedMaterial = new Material(disegno.sharedMaterial) { color = colori[(Numero - 1 + colori.Length) % colori.Length] };
        AspettoUmanoide.Prepara(figura, new Color(0.05f, 0.05f, 0.06f), AspettoUmanoide.Arma.Spada);

        // Il nome sopra la testa (Giocatore 1, 2, 3).
        var testo = new GameObject("Nome");
        testo.transform.SetParent(transform, false);
        testo.transform.localPosition = Vector3.up * 1.35f;
        etichetta = testo.AddComponent<TextMesh>();
        Font carattere = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        etichetta.font = carattere;
        testo.GetComponent<MeshRenderer>().sharedMaterial = carattere.material;
        etichetta.fontSize = 48;
        etichetta.characterSize = 0.03f;
        etichetta.anchor = TextAnchor.MiddleCenter;
        etichetta.color = new Color(0.85f, 0.75f, 0.55f);
        AggiornaEtichetta();
        Lingua.Cambiata += AggiornaEtichetta;
    }

    public override void OnDestroy()
    {
        Lingua.Cambiata -= AggiornaEtichetta;
        ObiettiviNemici.Togli(this);
        base.OnDestroy();
    }

    void AggiornaEtichetta()
    {
        if (etichetta != null) etichetta.text = Lingua.T("rete.giocatore") + " " + Numero;
    }

    // ---------- ogni fotogramma ----------

    void Update()
    {
        if (!IsSpawned) return;
        if (IsOwner) { CopiaGiocatore(); return; }
        SeguiGiocatore();
    }

    // Proprietario: copia il personaggio vero nelle variabili di rete (Netcode le spedisce solo quando cambiano).
    void CopiaGiocatore()
    {
        if (TrovaLocale() == null) return;
        Transform t = locale.transform;
        transform.SetPositionAndRotation(t.position, Quaternion.Euler(0f, t.eulerAngles.y, 0f));
        posizione.Value = t.position;
        direzione.Value = t.eulerAngles.y;

        // Un'azione nuova: stato diverso, oppure lo stesso stato ricominciato da capo.
        if (locale.StatoAttuale != statoPrima || locale.TempoNelloStato < tempoPrima) contatoreAzioni = (contatoreAzioni + 1) & 0xFFFF;
        statoPrima = locale.StatoAttuale;
        tempoPrima = locale.TempoNelloStato;

        azione.Value = Impacchetta(locale.StatoAttuale, locale.ColpoCombo, locale.AttaccoMagico,
            locale.Arma == GiocatoreControllo.ArmaImpugnata.Bastone, locale.HaBastone, locale.ArmaNelFodero, locale.Invisibile, contatoreAzioni);
        direzioneSchivata.Value = locale.DirezioneSchivata;
        tempiAttacco.Value = new Vector4(locale.DurataPreparazioneAttacco, locale.DurataColpoAttivo, locale.DurataRecuperoAttacco, locale.DurataSchivata);
        tempiAltro.Value = new Vector4(locale.DurataPreparazioneIncantesimo, locale.DurataRecuperoIncantesimo, locale.MomentoTaglio, locale.DurataEsecuzione);
        strettoia.Value = Mathf.Round(locale.Strettoia * 50f) / 50f;   // arrotondata: non serve spedirla a ogni minimo cambio
        furtivita.Value = locale.Furtivita;
    }

    // Chi guarda: muove la figura verso la posizione ricevuta e aggiorna arma, fodero, ombra e tempi delle azioni.
    void SeguiGiocatore()
    {
        int a = azione.Value;
        int contatore = (a >> 16) & 0xFFFF;
        if (contatore != ultimoContatore) { ultimoContatore = contatore; tempoLocale = 0f; }
        else tempoLocale += Time.deltaTime;

        bool fodero = (a & (1 << 9)) != 0;
        if (fodero != foderoPrima) { foderoPrima = fodero; inizioGestoFodero = Time.time; }

        if (figura != null)
        {
            bool haBastone = (a & (1 << 8)) != 0;
            bool bastone = (a & (1 << 7)) != 0;
            if (haBastone && !bastoneAggiunto) { AspettoUmanoide.AggiungiBastone(figura); bastoneAggiunto = true; }
            if (bastone != armaBastonePrima)
            {
                armaBastonePrima = bastone;
                AspettoUmanoide.MostraArma(figura, bastone ? AspettoUmanoide.Arma.Bastone : AspettoUmanoide.Arma.Spada);
            }
            AggiornaOmbra((a & (1 << 10)) != 0);
        }

        if (primoAggiornamento || (transform.position - posizione.Value).sqrMagnitude > 400f)
        {
            // Appena creata, o se l'altro si è spostato di colpo (rinascita al checkpoint): salta subito lì.
            transform.SetPositionAndRotation(posizione.Value, Quaternion.Euler(0f, direzione.Value, 0f));
            primoAggiornamento = false;
        }
        else
        {
            float t = 1f - Mathf.Exp(-morbidezza * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, posizione.Value, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, direzione.Value, 0f), t);
        }

        // Il nome guarda sempre verso la camera.
        if (etichetta != null && Camera.main != null)
            etichetta.transform.rotation = Quaternion.LookRotation(etichetta.transform.position - Camera.main.transform.position);
    }

    // Svanito nell'ombra: anche gli altri lo vedono scuro, come lo vede lui.
    void AggiornaOmbra(bool invisibile)
    {
        if (invisibile == invisibilePrima) return;
        invisibilePrima = invisibile;
        if (invisibile)
        {
            partiFigura.Clear();
            coloriFigura.Clear();
            foreach (Renderer parte in figura.GetComponentsInChildren<Renderer>())
            {
                if (!parte.enabled) continue;
                partiFigura.Add(parte);
                coloriFigura.Add(parte.material.color);
                parte.material.color = new Color(0.04f, 0.04f, 0.08f);
            }
            return;
        }
        for (int i = 0; i < partiFigura.Count; i++)
            if (partiFigura[i] != null) partiFigura[i].material.color = coloriFigura[i];
        partiFigura.Clear();
        coloriFigura.Clear();
    }

    static int Impacchetta(GiocatoreControllo.Stato stato, int combo, bool magico, bool bastoneInMano, bool haBastone, bool fodero, bool invisibile, int contatore)
    {
        int a = (int)stato & 0xF;
        a |= (combo & 0x3) << 4;
        if (magico) a |= 1 << 6;
        if (bastoneInMano) a |= 1 << 7;
        if (haBastone) a |= 1 << 8;
        if (fodero) a |= 1 << 9;
        if (invisibile) a |= 1 << 10;
        a |= (contatore & 0xFFFF) << 16;
        return a;
    }

    // ---------- IPersonaggioAnimato: quello che legge AnimazioneUmanoide per muovere la figura ----------

    public GiocatoreControllo.Stato StatoAttuale => (GiocatoreControllo.Stato)(azione.Value & 0xF);
    public float TempoNelloStato => tempoLocale;
    public int ColpoCombo => (azione.Value >> 4) & 0x3;
    public bool AttaccoMagico => (azione.Value & (1 << 6)) != 0;
    public float DurataPreparazioneAttacco => tempiAttacco.Value.x;
    public float DurataColpoAttivo => tempiAttacco.Value.y;
    public float DurataRecuperoAttacco => tempiAttacco.Value.z;
    public float DurataSchivata => tempiAttacco.Value.w;
    public Vector3 DirezioneSchivata => direzioneSchivata.Value;
    public float DurataPreparazioneIncantesimo => tempiAltro.Value.x;
    public float DurataRecuperoIncantesimo => tempiAltro.Value.y;
    public float MomentoTaglio => tempiAltro.Value.z;
    public float DurataEsecuzione => tempiAltro.Value.w;
    public float Strettoia => strettoia.Value;
    public bool ArmaNelFodero => (azione.Value & (1 << 9)) != 0;
    public float TempoGestoFodero => Time.time - inizioGestoFodero;
    public float DurataGestoFodero => 0.45f;

    // ---------- IObiettivoNemico: i nemici dell'host attaccano anche questa figura ----------

    public Transform Corpo => transform;
    public bool Abbattuto => StatoAttuale == GiocatoreControllo.Stato.Morto;
    public bool Invisibile => (azione.Value & (1 << 10)) != 0;
    public float Furtivita => furtivita.Value;

    public void ColpitoDaNemico(Bersaglio nemico)
    {
        if (nemico != null && IsSpawned && IsServer) ColpitoRpc(nemico.NumeroRete);
    }

    // Sul PC del giocatore colpito: il suo personaggio vero riceve il colpo (con parata, schivata e armatura).
    [Rpc(SendTo.Owner)]
    void ColpitoRpc(int numeroNemico)
    {
        Bersaglio nemico = RegistroNemici.Trova(numeroNemico);
        if (nemico != null && TrovaLocale() != null) locale.ColpitoDaNemico(nemico);
    }

    // Il premio dell'uccisione arriva con MondoRete.InviaSconfitto; qui non serve.
    public void NemicoSconfitto() { }
}
