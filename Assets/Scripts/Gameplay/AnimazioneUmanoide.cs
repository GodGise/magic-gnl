using UnityEngine;

// Animazione provvisoria della figura a blocchi creata da AspettoUmanoide.
// A cosa serve: dà vita a giocatore e nemici senza animazioni vere (arriveranno con i modelli di Nazar).
//   - Camminata: gambe e braccia oscillano (braccio opposto alla gamba) e il corpo saltella appena;
//     più si va veloci, più il passo è ampio. Da fermi la figura torna dritta.
//   - Attacco del giocatore: tre colpi diversi in combo (orizzontale da destra, rovescio da sinistra,
//     dall'alto). Ogni colpo ha tre momenti: carica, colpo, ritorno, legati ai tempi dell'attacco.
//   - Schivata: uno scatto, con il corpo basso e inclinato nella direzione in cui si schiva.
//   - Parata: braccia alzate davanti. Colpito: barcolla all'indietro. Morto: cade a terra.
//   - Esecuzione furtiva: il giocatore afferra il nemico da dietro con il braccio sinistro e con la spada gli
//     taglia la gola con un fendente orizzontale; il nemico si inarca all'indietro con le braccia aperte.
//   - Nemici: durante il preavviso rosso caricano il colpo, poi colpiscono; ogni attacco usa
//     uno dei tre movimenti, a turno.
//   - Strettoie (solo giocatore, vedi PassaggioStretto): più lo spazio si stringe, più la figura si gira
//     di fianco, abbassa le braccia vicino al corpo e avanza a passetti laterali corti. Quando entra nella
//     strettoia porta il braccio dietro la spalla e mette l'arma nel fodero sulla schiena; quando esce la riprende.
// Co-op: anima allo stesso modo anche la figura degli altri giocatori (GiocatoreRete), con i loro stati ricevuti in rete.
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

    [Header("Strettoie")]
    [Tooltip("Di quanto si gira di fianco la figura nella strettoia piena, in gradi.")]
    [SerializeField] float rotazioneDiFianco = 80f;
    [Tooltip("Lunghezza dei passetti laterali nella strettoia piena, in metri.")]
    [SerializeField] float passoInStrettoia = 0.35f;
    [Tooltip("Quanto si allargano le gambe a ogni passetto laterale, in gradi.")]
    [SerializeField] float aperturaPassetti = 22f;

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

    // Lancio con il bastone: lo alza in alto dietro la testa, poi lo spinge in avanti verso il nemico.
    static readonly Posa CaricaIncantesimo = new Posa
    {
        braccioDestro = new Vector3(-150f, 0f, 15f), braccioSinistro = new Vector3(-60f, 0f, -20f),
        gambaDestra = new Vector3(12f, 0f, 0f), gambaSinistra = new Vector3(-10f, 0f, 0f),
        corpo = new Vector3(-8f, 15f, 0f), abbassamento = 0.03f
    };

    static readonly Posa ColpoIncantesimo = new Posa
    {
        braccioDestro = new Vector3(-75f, 0f, 0f), braccioSinistro = new Vector3(-40f, 0f, -15f),
        gambaDestra = new Vector3(20f, 0f, 0f), gambaSinistra = new Vector3(-25f, 0f, 0f),
        corpo = new Vector3(12f, -5f, 0f), abbassamento = 0.1f
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

    // Di fianco fra due pareti: braccia strette al corpo, busto appena chinato.
    // (La rotazione di fianco e i passetti si aggiungono a parte, perché dipendono dai numeri dell'Inspector.)
    static readonly Posa PosaStretta = new Posa
    {
        braccioDestro = new Vector3(-20f, 0f, -6f), braccioSinistro = new Vector3(-20f, 0f, 6f),
        corpo = new Vector3(4f, 0f, 0f), abbassamento = 0.06f
    };

    // Il braccio destro va dietro la spalla destra, dove sta il fodero.
    static readonly Vector3 BraccioAlFodero = new Vector3(-165f, 0f, -20f);

    static readonly Posa PosaCaduto = new Posa
    {
        braccioDestro = new Vector3(-20f, 0f, 60f), braccioSinistro = new Vector3(-20f, 0f, -60f),
        corpo = new Vector3(-85f, 0f, 0f), abbassamento = 0.85f
    };

    // Esecuzione furtiva, giocatore: presa (braccio sinistro attorno al nemico, spada portata a destra
    // all'altezza della gola), poi il taglio da destra a sinistra con il busto che ruota.
    static readonly Posa PosaPresa = new Posa
    {
        braccioSinistro = new Vector3(-95f, 0f, 20f), braccioDestro = new Vector3(-95f, 0f, 60f),
        gambaDestra = new Vector3(15f, 0f, 0f), gambaSinistra = new Vector3(-12f, 0f, 0f),
        corpo = new Vector3(10f, 20f, 0f), abbassamento = 0.06f
    };

    static readonly Posa PosaTaglio = new Posa
    {
        braccioSinistro = new Vector3(-80f, 0f, 10f), braccioDestro = new Vector3(-95f, 0f, -55f),
        gambaDestra = new Vector3(18f, 0f, 0f), gambaSinistra = new Vector3(-15f, 0f, 0f),
        corpo = new Vector3(12f, -25f, 0f), abbassamento = 0.1f
    };

    // Esecuzione furtiva, nemico: preso alle spalle, si inarca all'indietro con le braccia aperte.
    static readonly Posa PosaGiustiziato = new Posa
    {
        braccioDestro = new Vector3(-35f, 0f, 40f), braccioSinistro = new Vector3(-35f, 0f, -40f),
        gambaDestra = new Vector3(-8f, 0f, 0f), gambaSinistra = new Vector3(8f, 0f, 0f),
        corpo = new Vector3(-22f, 0f, 0f), abbassamento = 0.12f
    };

    // ---------- Stato ----------

    Transform figura, gambaSinistra, gambaDestra, braccioSinistro, braccioDestro;
    IPersonaggioAnimato giocatore;   // GiocatoreControllo, o GiocatoreRete per gli altri giocatori in co-op
    Bersaglio nemico;
    InseguimentoNemico inseguimento;   // solo per i nemici con la vista: serve a sapere se sta subendo un'esecuzione
    Vector3 posizioneFigura;
    Vector3 ultimaPosizione;
    float fase;
    float ampiezza;

    // Per i nemici: il colpo parte quando finisce il preavviso, e dura qualche istante.
    bool nemicoCaricava;
    float colpoNemicoFino;
    int movimentoNemico;

    // Fodero sulla schiena (solo giocatore): dove va l'arma nelle strettoie.
    Transform mano, fodero, armaNelFodero;

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
        giocatore = GetComponentInParent<IPersonaggioAnimato>();
        nemico = GetComponent<Bersaglio>();

        if (giocatore != null)
        {
            mano = TrovaFiglio(transform, "Mano destra");
            // Fodero dietro la spalla destra: l'impugnatura in alto, la lama giù in diagonale dietro la schiena.
            fodero = new GameObject("Fodero").transform;
            fodero.SetParent(figura, false);
            fodero.localPosition = new Vector3(0.2f, 0.62f, -0.2f);
            fodero.localRotation = Quaternion.Euler(0f, 0f, -25f);
        }
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

        // Strettoia: 0 = camminata normale, 1 = tutto di fianco a passetti (cresce piano, vedi PassaggioStretto).
        float stretto = giocatore != null ? giocatore.Strettoia : 0f;

        // Nella strettoia si va piano: l'ampiezza piena si raggiunge già a passo lento.
        float velocitaPiena = Mathf.Lerp(velocitaPienaAmpiezza, 1.5f, stretto);
        float obiettivo = Mathf.Clamp01(distanza / dt / velocitaPiena);
        ampiezza = Mathf.MoveTowards(ampiezza, obiettivo, dt * prontezza);
        fase += distanza / Mathf.Lerp(lunghezzaPasso, passoInStrettoia, stretto) * Mathf.PI; // un ciclo ogni due passi
        float onda = Mathf.Sin(fase);
        float gamba = onda * angoloGambe * ampiezza;
        float braccio = onda * angoloBraccia * ampiezza;
        float saltello = Mathf.Abs(Mathf.Cos(fase)) * saltelloCorpo * ampiezza;

        // Camminata normale, poi mescolata con quella di fianco quanto è stretto il passaggio.
        // Di fianco si va verso la sinistra della figura: la gamba sinistra si apre, poi la destra la raggiunge.
        Quaternion camminaGambaS = Quaternion.Euler(gamba, 0f, 0f);
        Quaternion camminaGambaD = Quaternion.Euler(-gamba, 0f, 0f);
        Quaternion camminaBraccioS = Quaternion.Euler(-braccio, 0f, 0f);
        Quaternion camminaBraccioD = Quaternion.Euler(braccio, 0f, 0f);
        if (stretto > 0f)
        {
            float apertura = aperturaPassetti * ampiezza;
            camminaGambaS = Quaternion.Slerp(camminaGambaS, Quaternion.Euler(0f, 0f, -apertura * Mathf.Max(0f, onda)), stretto);
            camminaGambaD = Quaternion.Slerp(camminaGambaD, Quaternion.Euler(0f, 0f, -apertura * 0.6f * Mathf.Max(0f, -onda)), stretto);
            camminaBraccioS = Quaternion.Slerp(camminaBraccioS, Quaternion.Euler(PosaStretta.braccioSinistro), stretto);
            camminaBraccioD = Quaternion.Slerp(camminaBraccioD, Quaternion.Euler(PosaStretta.braccioDestro), stretto);
        }

        // 2. Azione in corso (attacco, schivata...): quanto pesa sulla camminata, da 0 a 1.
        Posa azione = PosaAzione(out float peso);

        // Gesto del fodero: il braccio destro va dietro la spalla e torna, a metà gesto l'arma cambia posto.
        float gestoFodero = AggiornaFodero();
        if (gestoFodero > 0f) camminaBraccioD = Quaternion.Slerp(camminaBraccioD, Quaternion.Euler(BraccioAlFodero), gestoFodero);

        // 3. Mescola camminata e azione, poi avvicina ogni parte alla sua posa in modo morbido.
        float k = 1f - Mathf.Exp(-velocitaPose * dt);
        Avvicina(gambaSinistra, camminaGambaS, azione.gambaSinistra, peso, k);
        Avvicina(gambaDestra, camminaGambaD, azione.gambaDestra, peso, k);
        Avvicina(braccioSinistro, camminaBraccioS, azione.braccioSinistro, peso, k);
        Avvicina(braccioDestro, camminaBraccioD, azione.braccioDestro, peso, k);

        // Corpo: di fianco quanto è stretto il passaggio, poi l'eventuale azione sopra.
        Quaternion corpoCammina = Quaternion.Slerp(Quaternion.identity,
            Quaternion.Euler(PosaStretta.corpo.x, rotazioneDiFianco, PosaStretta.corpo.z), stretto);
        Quaternion rotazioneCorpo = Quaternion.Slerp(corpoCammina, Quaternion.Euler(azione.corpo), peso);
        figura.localRotation = Quaternion.Slerp(figura.localRotation, rotazioneCorpo, k);
        float abbassamentoCammina = PosaStretta.abbassamento * stretto - saltello;
        Vector3 posizioneCorpo = posizioneFigura + Vector3.up * (-abbassamentoCammina * (1f - peso) - azione.abbassamento * peso);
        figura.localPosition = Vector3.Lerp(figura.localPosition, posizioneCorpo, k);
    }

    // Sposta l'arma fra mano e fodero seguendo GiocatoreControllo. Restituisce quanto il braccio
    // deve andare verso il fodero (0 = niente gesto, 1 = mano dietro la spalla).
    float AggiornaFodero()
    {
        if (giocatore == null || mano == null || fodero == null) return 0f;

        float g = giocatore.TempoGestoFodero / Mathf.Max(0.01f, giocatore.DurataGestoFodero);
        bool gestoInCorso = g < 1f;

        // L'arma cambia posto a metà gesto (quando la mano è alla spalla), oppure subito se non c'è un gesto (rinascita).
        if (!gestoInCorso || g >= 0.5f)
        {
            if (giocatore.ArmaNelFodero && armaNelFodero == null) MettiNelFodero();
            else if (!giocatore.ArmaNelFodero && armaNelFodero != null) RiprendiArma();
        }
        return gestoInCorso ? Mathf.Sin(g * Mathf.PI) : 0f;
    }

    void MettiNelFodero()
    {
        foreach (Transform arma in mano)
        {
            if (!arma.gameObject.activeSelf) continue;
            armaNelFodero = arma;
            arma.SetParent(fodero, false);
            arma.localPosition = Vector3.zero;
            arma.localRotation = Quaternion.identity;
            return;
        }
    }

    void RiprendiArma()
    {
        armaNelFodero.SetParent(mano, false);
        armaNelFodero.localPosition = Vector3.zero;
        armaNelFodero.localRotation = Quaternion.identity;
        armaNelFodero = null;
    }

    static Transform TrovaFiglio(Transform da, string nome)
    {
        foreach (Transform figlio in da)
        {
            if (figlio.name == nome) return figlio;
            Transform trovato = TrovaFiglio(figlio, nome);
            if (trovato != null) return trovato;
        }
        return null;
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
                if (giocatore.AttaccoMagico)
                    return PosaTempi(CaricaIncantesimo, ColpoIncantesimo, t, giocatore.DurataPreparazioneIncantesimo, 0.1f, giocatore.DurataRecuperoIncantesimo);
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

            case GiocatoreControllo.Stato.Esecuzione:
                return PosaEsecuzione(t, giocatore.MomentoTaglio, giocatore.DurataEsecuzione);
        }
        peso = 0f;
        return Neutra;
    }

    Posa PosaNemico(out float peso)
    {
        peso = 1f;
        if (inseguimento == null) inseguimento = GetComponent<InseguimentoNemico>();
        if (inseguimento != null && inseguimento.InEsecuzione) return PosaGiustiziato;

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

    // Esecuzione: si porta nella presa, la tiene fino al taglio, taglia in un attimo, poi torna dritto.
    static Posa PosaEsecuzione(float t, float momentoTaglio, float durata)
    {
        float presa = Mathf.Max(0.05f, momentoTaglio * 0.45f);
        const float durataTaglio = 0.12f;
        if (t < presa) return Posa.Mescola(Neutra, PosaPresa, Mathf.SmoothStep(0f, 1f, t / presa));
        if (t < momentoTaglio) return PosaPresa;
        if (t < momentoTaglio + durataTaglio) return Posa.Mescola(PosaPresa, PosaTaglio, (t - momentoTaglio) / durataTaglio);
        float ritorno = Mathf.Max(0.05f, durata - momentoTaglio - durataTaglio);
        return Posa.Mescola(PosaTaglio, Neutra, Mathf.SmoothStep(0f, 1f, (t - momentoTaglio - durataTaglio) / ritorno));
    }

    // Un colpo della combo: carica durante la preparazione, colpo durante la fase attiva, ritorno nel recupero.
    static Posa PosaColpo(int indice, float t, float preparazione, float attivo, float recupero)
    {
        return PosaTempi(Cariche[indice % 3], Colpi[indice % 3], t, preparazione, attivo, recupero);
    }

    // Carica, colpo e ritorno in posizione, in base al tempo passato dall'inizio dell'azione.
    static Posa PosaTempi(Posa carica, Posa colpo, float t, float preparazione, float attivo, float recupero)
    {
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
