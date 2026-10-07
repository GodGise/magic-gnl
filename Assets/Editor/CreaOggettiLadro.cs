using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea i file degli oggetti del Ladro (lo stile "assassino": uccisioni a distanza o di
// soppiatto). Armi a distanza (arco corto, arco lungo, balestra), armi corte (pugnale, stiletto, doppi pugnali),
// armature (leggera, cuoio, ombra) e amuleti (magici e arcani, ognuno con un bonus e un malus).
// A cosa serve: ogni oggetto è un file in Assets/Dati/Oggetti/Ladro/ (ArmiDistanza, ArmiCorte, Armature, Amuleti).
// Il menu "Crea" crea solo i file che MANCANO; il menu "Aggiorna" rimette in tutti i valori scritti qui sotto
// (tenendo icona e modello già collegati).
// Come si usa: menu in alto "magic-gnl > Crea oggetti del Ladro", poi selezionare il Giocatore e trascinare gli
// oggetti nelle caselle di Equipaggiamento. Le armi a distanza sono pronte ma il tiro con l'arco non c'è ancora.
// Per aggiungere un oggetto nuovo: una riga nel metodo Crea() qui sotto, con i suoi numeri.
public static class CreaOggettiLadro
{
    const string Radice = "Assets/Dati/Oggetti/Ladro";
    static bool riscrivi;

    [MenuItem("magic-gnl/Crea oggetti del Ladro")]
    static void MenuCrea()
    {
        riscrivi = false;
        Crea();
    }

    [MenuItem("magic-gnl/Aggiorna oggetti del Ladro (riscrive i valori)")]
    static void MenuAggiorna()
    {
        if (!EditorUtility.DisplayDialog("Oggetti del Ladro",
            "Rimettere in tutti gli oggetti i valori scritti in CreaOggettiLadro.cs? Le modifiche fatte a mano nell'Inspector andranno perse (icona e modello restano).",
            "Aggiorna", "Annulla")) return;
        riscrivi = true;
        Crea();
        riscrivi = false;
    }

    static void Crea()
    {
        int creati = 0;

        // Riferimenti: giocatore con vita 100, critico 10% da ×1,75; orco sgherro con vita 160, armatura 25,
        // vista 18 m. Il Ladro fa poco danno di fronte e moltissimo alle spalle o da lontano senza essere visto.

        // ---------- Armi corte ----------

        // Pugnale: veloce, danno basso, colpo alle spalle forte. La base del Ladro.
        creati += Arma("pugnale-da-scuoiare", "Pugnale da scuoiare", "pugnale_scuoiare", a =>
        {
            a.tipo = DatiArma.Tipo.Pugnale;
            a.danno = 16f; a.costoAttacco = 12f;
            a.preparazione = 0.15f; a.colpoAttivo = 0.12f; a.recupero = 0.22f;
            a.portata = 1.4f; a.raggio = 1.0f; a.arco = 90f; a.affondo = 3.5f;
            a.probabilitaCritico = 10f; a.moltiplicatoreAlleSpalle = 2.5f;
            a.dannoAssorbitoSenzaScudo = 0.15f; a.costoParataSenzaScudo = 20f;
        });
        creati += Arma("pugnale-ricurvo-degli-orchi", "Pugnale ricurvo degli orchi", "pugnale_ricurvo", a =>
        {
            a.tipo = DatiArma.Tipo.Pugnale;
            a.danno = 22f; a.costoAttacco = 14f;
            a.preparazione = 0.18f; a.colpoAttivo = 0.12f; a.recupero = 0.27f;
            a.portata = 1.5f; a.raggio = 1.1f; a.arco = 100f; a.affondo = 3.5f;
            a.probabilitaCritico = 5f; a.moltiplicatoreAlleSpalle = 2f;
            a.dannoAssorbitoSenzaScudo = 0.25f; a.costoParataSenzaScudo = 25f;
        });

        // Stiletto: punta sottile, buca le armature, colpo alle spalle devastante.
        creati += Arma("stiletto-del-tagliagole", "Stiletto del tagliagole", "stiletto_tagliagole", a =>
        {
            a.tipo = DatiArma.Tipo.Stiletto;
            a.danno = 20f; a.costoAttacco = 13f;
            a.preparazione = 0.17f; a.colpoAttivo = 0.1f; a.recupero = 0.25f;
            a.portata = 1.4f; a.raggio = 0.9f; a.arco = 60f; a.affondo = 4f;
            a.penetrazioneArmatura = 0.5f;
            a.probabilitaCritico = 15f; a.moltiplicatoreCritico = 0.25f; a.moltiplicatoreAlleSpalle = 3f;
            a.dannoAssorbitoSenzaScudo = 0.15f; a.costoParataSenzaScudo = 28f;
        });
        creati += Arma("stiletto-d-ombra", "Stiletto d'ombra", "stiletto_ombra", a =>
        {
            a.tipo = DatiArma.Tipo.Stiletto;
            a.danno = 24f; a.costoAttacco = 14f;
            a.preparazione = 0.16f; a.colpoAttivo = 0.1f; a.recupero = 0.24f;
            a.portata = 1.4f; a.raggio = 0.9f; a.arco = 60f; a.affondo = 4f;
            a.penetrazioneArmatura = 0.3f;
            a.probabilitaCritico = 20f; a.moltiplicatoreCritico = 0.25f; a.moltiplicatoreAlleSpalle = 3f;
            a.dannoAssorbitoSenzaScudo = 0.15f; a.costoParataSenzaScudo = 28f;
        });

        // Doppi pugnali: una lama per mano (niente scudo), colpi rapidissimi e deboli.
        creati += Arma("pugnali-gemelli", "Pugnali gemelli", "pugnali_gemelli", a =>
        {
            a.tipo = DatiArma.Tipo.DoppiPugnali; a.dueMani = true;
            a.danno = 12f; a.costoAttacco = 10f;
            a.preparazione = 0.1f; a.colpoAttivo = 0.1f; a.recupero = 0.18f;
            a.portata = 1.4f; a.raggio = 1.1f; a.arco = 120f; a.affondo = 3.5f;
            a.probabilitaCritico = 10f; a.moltiplicatoreAlleSpalle = 2f;
            a.dannoAssorbitoSenzaScudo = 0.1f; a.costoParataSenzaScudo = 30f;
        });

        // ---------- Armi a distanza (il tiro si farà più avanti) ----------

        creati += ArmaDistanza("arco-corto-da-caccia", "Arco corto da caccia", "arco_caccia", d =>
        {
            d.tipo = DatiArmaDistanza.Tipo.ArcoCorto;
            d.danno = 18f; d.costoTiro = 12f; d.carica = 0.4f; d.ricarica = 0.35f;
            d.portata = 25f; d.velocitaFreccia = 35f;
            d.moltiplicatoreNonVisto = 1.5f;
        });
        creati += ArmaDistanza("arco-d-osso-degli-orchi", "Arco d'osso degli orchi", "arco_osso", d =>
        {
            d.tipo = DatiArmaDistanza.Tipo.ArcoCorto;
            d.danno = 22f; d.costoTiro = 14f; d.carica = 0.45f; d.ricarica = 0.4f;
            d.portata = 28f; d.velocitaFreccia = 38f;
            d.probabilitaCritico = 5f; d.moltiplicatoreNonVisto = 1.5f;
        });
        creati += ArmaDistanza("arco-lungo-di-tasso", "Arco lungo di tasso", "arco_tasso", d =>
        {
            d.tipo = DatiArmaDistanza.Tipo.ArcoLungo;
            d.danno = 32f; d.costoTiro = 20f; d.carica = 0.9f; d.ricarica = 0.6f;
            d.portata = 45f; d.velocitaFreccia = 50f;
            d.probabilitaCritico = 5f; d.moltiplicatoreNonVisto = 2f;
        });
        creati += ArmaDistanza("balestra-da-posta", "Balestra da posta", "balestra_posta", d =>
        {
            d.tipo = DatiArmaDistanza.Tipo.Balestra;
            d.danno = 45f; d.costoTiro = 18f; d.carica = 0.3f; d.ricarica = 1.6f;
            d.portata = 35f; d.velocitaFreccia = 60f; d.penetrazioneArmatura = 0.5f;
            d.moltiplicatoreNonVisto = 2f;
        });
        creati += ArmaDistanza("balestra-del-carceriere", "Balestra del carceriere", "balestra_carceriere", d =>
        {
            d.tipo = DatiArmaDistanza.Tipo.Balestra;
            d.danno = 60f; d.costoTiro = 25f; d.carica = 0.4f; d.ricarica = 2.2f;
            d.portata = 40f; d.velocitaFreccia = 65f; d.penetrazioneArmatura = 0.7f;
            d.moltiplicatoreNonVisto = 1.5f;
        });

        // ---------- Armature (poca protezione, tanta furtività) ----------

        creati += Armatura("giubba-scura", "Giubba scura", "giubba_scura", b =>
        {
            b.peso = DatiArmatura.Peso.Leggera;
            b.armatura = 5f; b.furtivita = 10f; b.costoSchivataExtra = 0f; b.moltiplicatoreVelocita = 1f;
        });
        creati += Armatura("corpetto-di-cuoio-rinforzato", "Corpetto di cuoio rinforzato", "corpetto_cuoio", b =>
        {
            b.peso = DatiArmatura.Peso.Cuoio;
            b.armatura = 15f; b.furtivita = 5f; b.costoSchivataExtra = 2f; b.moltiplicatoreVelocita = 0.99f;
        });
        creati += Armatura("manto-dell-ombra", "Manto dell'ombra", "manto_ombra", b =>
        {
            b.peso = DatiArmatura.Peso.Ombra;
            b.armatura = 3f; b.furtivita = 30f; b.costoSchivataExtra = 0f; b.moltiplicatoreVelocita = 1f;
        });

        // ---------- Amuleti (ognuno con un bonus e un malus) ----------

        // Magici: bonus semplici alle statistiche.
        creati += Amuleto("piuma-di-civetta", "Piuma di civetta", "piuma_civetta", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.furtivita = 15f;
            m.malus.bonusDanno = -5f;
        });
        creati += Amuleto("dente-di-vipera", "Dente di vipera", "dente_vipera", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.probabilitaCritico = 12f;
            m.malus.armaturaPercento = -10f;
        });
        creati += Amuleto("laccio-del-borsaiolo", "Laccio del borsaiolo", "laccio_borsaiolo", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.velocitaAttacco = 10f;
            m.malus.vitaMassimaPercento = -10f;
        });

        // Arcani: effetti speciali.
        creati += Amuleto("ultimo-respiro", "Ultimo respiro", "ultimo_respiro", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.VitaPerUccisione; m.valore = 8f;
            m.malus.bonusDanno = -5f;
        });
        creati += Amuleto("fiato-del-predatore", "Fiato del predatore", "fiato_predatore", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.RecuperoResistenza; m.valore = 40f;
            m.malus.armaturaPercento = -15f;
        });
        creati += Amuleto("goccia-di-sangue-nero", "Goccia di sangue nero", "goccia_sangue_nero", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.RubaVita; m.valore = 8f;
            m.malus.vitaMassimaPercento = -10f;
        });

        AssetDatabase.SaveAssets();
        Debug.Log(creati > 0
            ? "Oggetti del Ladro: " + (riscrivi ? "aggiornati " : "creati ") + creati + " file in " + Radice + "."
            : "Oggetti del Ladro: c'erano già tutti, nessun file cambiato.");
    }

    static int Arma(string file, string nome, string chiave, System.Action<DatiArma> imposta) =>
        Oggetto("ArmiCorte", file, nome, chiave, imposta);

    static int ArmaDistanza(string file, string nome, string chiave, System.Action<DatiArmaDistanza> imposta) =>
        Oggetto("ArmiDistanza", file, nome, chiave, imposta);

    static int Armatura(string file, string nome, string chiave, System.Action<DatiArmatura> imposta) =>
        Oggetto("Armature", file, nome, chiave, imposta);

    static int Amuleto(string file, string nome, string chiave, System.Action<DatiAmuleto> imposta) =>
        Oggetto("Amuleti", file, nome, chiave, imposta);

    // Crea il file se manca; restituisce 1 se l'ha creato, 0 se c'era già.
    static int Oggetto<T>(string cartella, string file, string nome, string chiave, System.Action<T> imposta) where T : DatiOggetto
    {
        string percorsoCartella = Radice + "/" + cartella;
        CreaCartelle(percorsoCartella);
        string percorso = percorsoCartella + "/" + file + ".asset";
        if (AssetDatabase.LoadAssetAtPath<T>(percorso) != null) return 0;

        T oggetto = ScriptableObject.CreateInstance<T>();
        oggetto.nomeDiLavoro = nome;
        oggetto.chiaveNome = "oggetto." + chiave + ".nome";
        oggetto.chiaveDescrizione = "oggetto." + chiave + ".descrizione";
        oggetto.classe = ClasseGiocatore.Ladro;
        imposta(oggetto);
        AssetDatabase.CreateAsset(oggetto, percorso);
        return 1;
    }

    static void CreaCartelle(string percorso)
    {
        string[] parti = percorso.Split('/');
        string attuale = parti[0];
        for (int i = 1; i < parti.Length; i++)
        {
            string prossima = attuale + "/" + parti[i];
            if (!AssetDatabase.IsValidFolder(prossima)) AssetDatabase.CreateFolder(attuale, parti[i]);
            attuale = prossima;
        }
    }
}
