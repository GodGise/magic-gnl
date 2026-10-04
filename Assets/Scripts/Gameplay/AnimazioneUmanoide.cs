using UnityEngine;

// Animazione provvisoria della figura a blocchi creata da AspettoUmanoide.
// A cosa serve: dà vita a giocatore e nemici senza animazioni vere (arriveranno con i modelli di Nazar).
//   - Camminata: gambe e braccia oscillano (braccio opposto alla gamba) e il corpo saltella appena;
//     più si va veloci, più il passo è ampio. Da fermi la figura torna dritta.
//   - Attacco del giocatore: tre colpi diversi in combo (orizzontale da destra, rovescio da sinistra,
//     dall'alto). Ogni colpo ha tre momenti: carica, colpo, ritorno, legati ai tempi dell'attacco.
//   - Schivata: uno scatto, con il corpo basso e inclinato nella direzione in cui si schiva.
//   - Parata: braccia alzate davanti. Colpito: barcolla all'indietro. Morto: cade a terra.
//   - Nemici: durante il preavviso rosso caricano il colpo, poi colpiscono; ogni attacco usa
//     uno dei tre movimenti, a turno.
// Come montarlo: non serve montarlo. Lo aggiunge da solo AspettoUmanoide quando crea la figura.
// I numeri si possono regolare dall'Inspector durante il Play (sul giocatore o sul nemico).
public class AnimazioneUmanoide : MonoBehaviour
{
    [Header("Camminata")]
    [Tooltip("Ampiezza massima dell'oscillazione delle gambe, in gradi.")]
    [SerializeField] float angoloGambe = 35f;
    [Tooltip("Ampiezza massima dell'oscillazione delle braccia, in gradi.")]
    [SerializeField] float angoloBraccia = 30f;
    [Tooltip("Metri percorsi a ogni passo: più è piccolo, più le gambe vanno veloci.")]
    [SerializeField] float lunghezzaPasso = 0.9f;
    [Tooltip("Velocità (metri al secondo) a cui il passo raggiunge l'ampiezza massima.")]
    [SerializeField] float velocitaPienaAmpiezza = 5f;
    [Tooltip("Di quanto si alza il corpo a ogni passo, in metri.")]
    [SerializeField] float saltelloCorpo = 0.05f;
    [Tooltip("Quanto in fretta l'oscillazione parte e si ferma.")]
    [SerializeField] float prontezza = 4f;

    [Header("Azioni")]
    [Tooltip("Quanto in fretta la figura raggiunge ogni posa: più è alto, più i movimenti sono secchi.")]
    [SerializeField] float velocitaPose = 22f;
    [Tooltip("Quanto si inclina il corpo durante la schivata, in gradi.")]
    [SerializeField] float inclinazioneSchivata = 28f;
    [Tooltip("Per quanti secondi il nemico resta nella posa del colpo dopo aver colpito.")]
    [SerializeField] float durataColpoNemico = 0.4f;

    // Oltre questa velocità non è camminare ma uno spostamento di colpo (per esempio la rinascita).
    const float VelocitaTeletrasporto = 30f;

    // ---------- Pose ----------
    // Una posa dice come ruotare ogni parte (angoli in gradi, attorno a X, Y e Z) e quanto abbassare il corpo.
    // X negativo sulle braccia o sulle gambe = in avanti; Z positivo sul braccio destro = verso l'esterno.
    struct Posa
    {
        public Vector3 braccioDestro, braccioSinistro, gambaDestra, gambaSinistra;
        public Vector3 corpo;      // X = inclinazione avanti/indietro, Y = torsione, Z = piega di lato
        public float abbassamento; // metri

        public static Posa Mescola(Posa a, Posa b, float t)
        {
            return new Posa
            {
                braccioDestro = Vector3.Lerp(a.braccioDestro, b.braccioDestro, t),
                braccioSinistro = Vector3.Lerp(a.braccioSinistro, b.braccioSinistro, t),
                gambaDestra = Vector3.Lerp(a.gambaDestra, b.gambaDestra, t),
                gambaSinistra = Vector3.Lerp(a.gambaSinistra, b.gambaSinistra, t),
                corpo = Vector3.Lerp(a.corpo, b.corpo, t),
                abbassamento = Mathf.Lerp(a.abbassamento, b.abbassamento, t)
            };
        }
    }

    static readonly Posa Neutra = new Posa();

    // I tre colpi della combo: per ognuno la posa di carica e quella del colpo.
    static readonly Posa[] Cariche =
    {
        // 1. Orizzontale da destra: braccio alzato in fuori a destra, busto girato a destra.
        new Posa { braccioDestro = new Vector3(-85f, 0f, 65f), braccioSinistro = new Vector3(-30f, 0f, -15f), corpo = new Vector3(0f, 30f, 0f), gambaDestra = new Vector3(15f, 0f, 0f), gambaSinistra = new Vector3(-10f, 0f, 0f), abbassamento = 0.05f },
        // 2. Rovescio da sinistra: braccio portato davanti al petto verso sinistra, busto girato a sinistra.
        new Posa { braccioDestro = new Vector3(-80f, 0f, -55f), braccioSinistro = new Vector3(20f, 0f, -20f), corpo = new Vector3(0f, -30f, 0f), gambaDestra = new Vector3(-10f, 0f, 0f), gambaSinistra = new Vector3(15f, 0f, 0f), abbassamento = 0.05f },
        // 3. Dall'alto: braccio alzato sopra la testa, corpo inclinato all'indietro.
        new Posa { braccioDestro = new Vector3(-170f, 0f, 10f), braccioSinistro = new Vector3(-150f, 0f, -10f), corpo = new Vector3(-12f, 0f, 0f), gambaDestra = new Vector3(10f, 0f, 0f), gambaSinistra = new Vector3(-15f, 0f, 0f), abbassamento = 0f }
    };

    static readonly Posa[] Colpi =
    {
        // 1. Il braccio spazza da destra a sinistra, il busto ruota, la gamba sinistra va avanti.
        new Posa { braccioDestro = new Vector3(-85f, 0f, -45f), braccioSinistro = new Vector3(25f, 0f, -20f), corpo = new Vector3(8f, -35f, 0f), gambaDestra = new Vector3(25f, 0f, 0f), gambaSinistra = new Vector3(-30f, 0f, 0f), abbassamento = 0.12f },
        // 2. Il braccio spazza da sinistra a destra, il busto ruota dall'altra parte.
        new Posa { braccioDestro = new Vector3(-85f, 0f, 75f), braccioSinistro = new Vector3(-35f, 0f, -25f), corpo = new Vector3(8f, 35f, 0f), gambaDestra = new Vector3(-30f, 0f, 0f), gambaSinistra = new Vector3(25f, 0f, 0f), abbassamento = 0.12f },
        // 3. Il colpo scende dall'alto fino davanti ai piedi, con un affondo in avanti.
        new Posa { braccioDestro = new Vector3(-25f, 0f, 0f), braccioSinistro = new Vector3(-20f, 0f, 0f), corpo = new Vector3(22f, 0f, 0f), gambaDestra = new Vector3(30f, 0f, 0f), gambaSinistra = new Vector3(-35f, 0f, 0f), abbassamento = 0.2f }
    };

    static readonly Posa PosaParata = new Posa
    {
        braccioDestro = new Vector3(-80f, 0f, -30f), braccioSinistro = new Vector3(-80f, 0f, 30f),
        gambaDestra = new Vector3(15f, 0f, 0f), gambaSinistra = new Vector3(-15f, 0f, 0f),
        corpo = new Vector3(6f, 0f, 0f), abbassamento = 0.08f
    };

    static readonly Posa PosaBarcollo = new Posa
    {
        braccioDestro = new Vector3(-25f, 0f, 35f), braccioSinistro = new Vector3(-25f, 0f, -35f),
        gambaDestra = new Vector3(15f, 0f, 0f), gambaSinistra = new Vector3(-5f, 0f, 0f),
        corpo = new Vector3(-15f, 0f, 0f), abbassamento = 0.05f
    };

    static readonly Posa PosaCaduto = new Posa
    {
        braccioDestro = new Vector3(-20f, 0f, 60f), braccioSinistro = new Vector3(-20f, 0f, -60f),
        corpo = new Vector3(-85f, 0f, 0f), abbassamento = 0.85f
    };

    // ---------- Stato ----------

    Transform figura, gambaSinistra, gambaDestra, braccioSinistro, braccioDestro;
    GiocatoreControllo giocatore;
    Bersaglio nemico;
    Vector3 posizioneFigura;
    Vector3 ultimaPosizione;
    float fase;
    float ampiezza;

    // Per i nemici: il colpo parte quando finisce il preavviso, e dura qualche istante.
    bool nemicoCaricava;
    float colpoNemicoFino;
    int movimentoNemico;

    // Chiamato da AspettoUmanoide: la figura e i quattro perni (anche e spalle) da far muovere.
    public void Imposta(Transform figura, Transform gambaSinistra, Transform gambaDestra, Transform braccioSinistro, Transform braccioDestro)
    {
        this.figura = figura;
        this.gambaSinistra = gambaSinistra;
        this.gambaDestra = gambaDestra;
        this.braccioSinistro = braccioSinistro;
        this.braccioDestro = braccioDestro;
        posizioneFigura = figura.localPosition;
        ultimaPosizione = transform.position;
        giocatore = GetComponent<GiocatoreControllo>();
        nemico = GetComponent<Bersaglio>();
    }

    void LateUpdate()
    {
        if (figura == null) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // 1. Camminata, in base a quanto si è spostato in orizzontale dall'ultimo fotogramma.
        Vector3 spostamento = transform.position - ultimaPosizione;
        spostamento.y = 0f;
        ultimaPosizione = transform.position;
        float distanza = spostamento.magnitude;
        if (distanza / dt > VelocitaTeletrasporto) distanza = 0f;

        float obiettivo = Mathf.Clamp01(distanza / dt / velocitaPienaAmpiezza);
        ampiezza = Mathf.MoveTowards(ampiezza, obiettivo, dt * prontezza);
        fase += distanza / lunghezzaPasso * Mathf.PI; // un ciclo completo ogni due passi
        float onda = Mathf.Sin(fase);
        float gamba = onda * angoloGambe * ampiezza;
        float braccio = onda * angoloBraccia * ampiezza;
        float saltello = Mathf.Abs(Mathf.Cos(fase)) * saltelloCorpo * ampiezza;

        // 2. Azione in corso (attacco, schivata...): quanto pesa sulla camminata, da 0 a 1.
        Posa azione = PosaAzione(out float peso);

        // 3. Mescola camminata e azione, poi avvicina ogni parte alla sua posa in modo morbido.
        float k = 1f - Mathf.Exp(-velocitaPose * dt);
        Avvicina(gambaSinistra, Quaternion.Euler(gamba, 0f, 0f), azione.gambaSinistra, peso, k);
        Avvicina(gambaDestra, Quaternion.Euler(-gamba, 0f, 0f), azione.gambaDestra, peso, k);
        Avvicina(braccioSinistro, Quaternion.Euler(-braccio, 0f, 0f), azione.braccioSinistro, peso, k);
        Avvicina(braccioDestro, Quaternion.Euler(braccio, 0f, 0f), azione.braccioDestro, peso, k);

        Quaternion rotazioneCorpo = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(azione.corpo), peso);
        figura.localRotation = Quaternion.Slerp(figura.localRotation, rotazioneCorpo, k);
        Vector3 posizioneCorpo = posizioneFigura + Vector3.up * (saltello * (1f - peso) - azione.abbassamento * peso);
        figura.localPosition = Vector3.Lerp(figura.localPosition, posizioneCorpo, k);
    }

    static void Avvicina(Transform parte, Quaternion camminata, Vector3 angoliAzione, float peso, float k)
    {
        Quaternion obiettivo = Quaternion.Slerp(camminata, Quaternion.Euler(angoliAzione), peso);
        parte.localRotation = Quaternion.Slerp(parte.localRotation, obiettivo, k);
    }

    // Sceglie la posa dell'azione in corso. peso = 0 vuol dire solo camminata.
    Posa PosaAzione(out float peso)
    {
        peso = 0f;
        if (giocatore != null) return PosaGiocatore(out peso);
        if (nemico != null) return PosaNemico(out peso);
        return Neutra;
    }

    Posa PosaGiocatore(out float peso)
    {
        peso = 1f;
        float t = giocatore.TempoNelloStato;
        switch (giocatore.StatoAttuale)
        {
            case GiocatoreControllo.Stato.Attacco:
                return PosaColpo(giocatore.ColpoCombo, t, giocatore.DurataPreparazioneAttacco, giocatore.DurataColpoAttivo, giocatore.DurataRecuperoAttacco);

            case GiocatoreControllo.Stato.Schivata:
                // Entra subito nello scatto, lo tiene e negli ultimi istanti torna dritto.
                float s = Mathf.Clamp01(t / Mathf.Max(0.01f, giocatore.DurataSchivata));
                peso = s < 0.15f ? s / 0.15f : (s > 0.7f ? (1f - s) / 0.3f : 1f);
                return PosaSchivata(transform.InverseTransformDirection(giocatore.DirezioneSchivata));

            case GiocatoreControllo.Stato.Parata:
                return PosaParata;

            case GiocatoreControllo.Stato.Stordito:
                return PosaBarcollo;

            case GiocatoreControllo.Stato.Morto:
                return PosaCaduto;
        }
        peso = 0f;
        return Neutra;
    }

    Posa PosaNemico(out float peso)
    {
        peso = 1f;
        int indice = Mathf.Max(0, nemico.NumeroAttacco - 1) % 3;

        // Durante il preavviso rosso: carica il colpo piano piano.
        if (nemico.StaAttaccando)
        {
            nemicoCaricava = true;
            movimentoNemico = indice;
            float carica = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(nemico.TempoAttacco / Mathf.Max(0.01f, nemico.DurataPreavviso)));
            return Posa.Mescola(Neutra, Cariche[indice], carica);
        }

        // Il preavviso è appena finito: parte il colpo.
        if (nemicoCaricava)
        {
            nemicoCaricava = false;
            colpoNemicoFino = Time.time + durataColpoNemico;
        }

        if (Time.time < colpoNemicoFino)
        {
            // Primo terzo: colpo secco. Poi torna piano in posizione.
            float s = 1f - (colpoNemicoFino - Time.time) / durataColpoNemico;
            if (s < 0.3f) return Posa.Mescola(Cariche[movimentoNemico], Colpi[movimentoNemico], s / 0.3f);
            return Posa.Mescola(Colpi[movimentoNemico], Neutra, Mathf.SmoothStep(0f, 1f, (s - 0.3f) / 0.7f));
        }

        peso = 0f;
        return Neutra;
    }

    // Un colpo della combo: carica durante la preparazione, colpo durante la fase attiva, ritorno nel recupero.
    static Posa PosaColpo(int indice, float t, float preparazione, float attivo, float recupero)
    {
        Posa carica = Cariche[indice % 3];
        Posa colpo = Colpi[indice % 3];

        if (t < preparazione) return Posa.Mescola(Neutra, carica, Mathf.SmoothStep(0f, 1f, t / Mathf.Max(0.01f, preparazione)));
        t -= preparazione;
        if (t < attivo) return Posa.Mescola(carica, colpo, t / Mathf.Max(0.01f, attivo));
        t -= attivo;
        return Posa.Mescola(colpo, Neutra, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / Mathf.Max(0.01f, recupero))));
    }

    // Scatto della schivata: corpo basso e inclinato verso la direzione dello scatto (vista dal personaggio),
    // braccia tirate indietro, una gamba avanti e una dietro.
    Posa PosaSchivata(Vector3 direzioneLocale)
    {
        direzioneLocale.y = 0f;
        if (direzioneLocale.sqrMagnitude < 0.0001f) direzioneLocale = Vector3.back;
        direzioneLocale.Normalize();

        return new Posa
        {
            corpo = new Vector3(direzioneLocale.z * inclinazioneSchivata, 0f, -direzioneLocale.x * inclinazioneSchivata),
            braccioDestro = new Vector3(45f, 0f, 20f),
            braccioSinistro = new Vector3(45f, 0f, -20f),
            gambaDestra = new Vector3(30f, 0f, 0f),
            gambaSinistra = new Vector3(-35f, 0f, 0f),
            abbassamento = 0.18f
        };
    }
}
