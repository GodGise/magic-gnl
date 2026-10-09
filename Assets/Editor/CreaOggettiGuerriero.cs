using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea i file degli oggetti del Guerriero già decisi (armi, scudi, armature, amuleti).
// A cosa serve: ogni oggetto è un file in Assets/Dati/Oggetti/Guerriero/ (Armi, Scudi, Armature, Amuleti), con i suoi
// numeri nell'Inspector. Questo menu crea solo quelli che MANCANO: gli oggetti già creati e magari ritoccati
// a mano nell'Inspector non vengono toccati.
// Il menu "magic-gnl > Aggiorna oggetti del Guerriero (riscrive i valori)" invece rimette in TUTTI i file i valori
// scritti qui sotto (tenendo icona e modello già collegati): serve dopo aver cambiato i numeri in questo script.
// Come si usa: menu in alto "magic-gnl > Crea oggetti del Guerriero". Poi, per provarne uno, selezionare il
// Giocatore e trascinare l'oggetto nella casella giusta del componente Equipaggiamento.
// Per aggiungere un oggetto nuovo: una riga nel metodo Crea() qui sotto, con i suoi numeri.
public static class CreaOggettiGuerriero
{
    const string Radice = "Assets/Dati/Oggetti/Guerriero";

    static bool riscrivi;

    [MenuItem("magic-gnl/Crea oggetti del Guerriero")]
    static void MenuCrea()
    {
        riscrivi = false;
        Crea();
    }

    [MenuItem("magic-gnl/Aggiorna oggetti del Guerriero (riscrive i valori)")]
    static void MenuAggiorna()
    {
        if (!EditorUtility.DisplayDialog("Oggetti del Guerriero",
            "Rimettere in tutti gli oggetti i valori scritti in CreaOggettiGuerriero.cs? Le modifiche fatte a mano nell'Inspector andranno perse (icona e modello restano).",
            "Aggiorna", "Annulla")) return;
        riscrivi = true;
        Crea();
        riscrivi = false;
    }

    static void Crea()
    {
        int creati = 0;

        // Riferimento per i numeri: la Spada della vecchia vita (danno 25) è la base. L'orco sgherro ha
        // 160 di vita e 25 di armatura; il giocatore 100 di vita.

        // ---------- Armi ----------

        // Spada: equilibrata, veloce. La base di confronto per tutte le altre.
        creati += Arma("spada-della-vecchia-vita", "Spada della vecchia vita", "spada_vecchia_vita", a =>
        {
            a.tipo = DatiArma.Tipo.Spada;
            a.danno = 25f; a.costoAttacco = 20f;
            a.preparazione = 0.25f; a.colpoAttivo = 0.15f; a.recupero = 0.35f;
            a.portata = 1.8f; a.raggio = 1.3f; a.arco = 120f; a.affondo = 3f;
            a.dannoAssorbitoSenzaScudo = 0.4f; a.costoParataSenzaScudo = 25f;
        });
        creati += Arma("spada-del-capitano", "Spada del capitano", "spada_capitano", a =>
        {
            a.tipo = DatiArma.Tipo.Spada;
            a.danno = 30f; a.costoAttacco = 20f;
            a.preparazione = 0.24f; a.colpoAttivo = 0.15f; a.recupero = 0.33f;
            a.portata = 1.9f; a.raggio = 1.3f; a.arco = 120f; a.affondo = 3f;
            a.probabilitaCritico = 5f;
            a.dannoAssorbitoSenzaScudo = 0.3f; a.costoParataSenzaScudo = 20f;
        });

        // Ascia: più lenta, colpo largo, critici che fanno male.
        creati += Arma("ascia-da-legna", "Ascia da legna", "ascia_legna", a =>
        {
            a.tipo = DatiArma.Tipo.Ascia;
            a.danno = 34f; a.costoAttacco = 24f;
            a.preparazione = 0.38f; a.colpoAttivo = 0.15f; a.recupero = 0.48f;
            a.portata = 1.8f; a.raggio = 1.4f; a.arco = 150f; a.affondo = 2.5f;
            // Critico totale ×1,7: il giocatore parte da ×1,75, quindi l'ascia toglie 0,05.
            a.probabilitaCritico = 5f; a.moltiplicatoreCritico = -0.05f;
            a.dannoAssorbitoSenzaScudo = 0.25f; a.costoParataSenzaScudo = 30f;
        });
        creati += Arma("ascia-del-boia", "Ascia del boia", "ascia_boia", a =>
        {
            a.tipo = DatiArma.Tipo.Ascia;
            a.danno = 38f; a.costoAttacco = 30f;
            a.preparazione = 0.5f; a.colpoAttivo = 0.15f; a.recupero = 0.52f;
            a.portata = 1.9f; a.raggio = 1.4f; a.arco = 150f; a.affondo = 2.5f;
            // Critico totale ×2: il giocatore parte da ×1,75, quindi l'ascia aggiunge 0,25.
            a.probabilitaCritico = 10f; a.moltiplicatoreCritico = 0.25f;
            a.dannoAssorbitoSenzaScudo = 0.35f; a.costoParataSenzaScudo = 30f;
        });

        // Mazza: lenta e corta, ma ignora gran parte dell'armatura: la scelta contro i nemici corazzati.
        creati += Arma("mazza-ferrata", "Mazza ferrata", "mazza_ferrata", a =>
        {
            a.tipo = DatiArma.Tipo.Mazza;
            a.danno = 28f; a.costoAttacco = 25f;
            a.preparazione = 0.36f; a.colpoAttivo = 0.15f; a.recupero = 0.45f;
            a.portata = 1.6f; a.raggio = 1.2f; a.arco = 100f; a.affondo = 2.5f;
            a.penetrazioneArmatura = 0.45f;
            a.dannoAssorbitoSenzaScudo = 0.3f; a.costoParataSenzaScudo = 26f;
        });
        creati += Arma("martello-di-ossa", "Martello di ossa", "martello_ossa", a =>
        {
            a.tipo = DatiArma.Tipo.Mazza;
            a.danno = 34f; a.costoAttacco = 28f;
            a.preparazione = 0.42f; a.colpoAttivo = 0.18f; a.recupero = 0.5f;
            a.portata = 1.7f; a.raggio = 1.25f; a.arco = 100f; a.affondo = 2.5f;
            a.penetrazioneArmatura = 0.45f;
            a.probabilitaCritico = 1.5f;
            a.dannoAssorbitoSenzaScudo = 0.3f; a.costoParataSenzaScudo = 28f;
        });

        // Spadone: a due mani (niente scudo), lentissimo, portata e danno altissimi, para male con la lama.
        creati += Arma("spadone-del-cavaliere", "Spadone del cavaliere", "spadone_cavaliere", a =>
        {
            a.tipo = DatiArma.Tipo.Spadone; a.dueMani = true;
            a.danno = 46f; a.costoAttacco = 32f;
            a.preparazione = 0.5f; a.colpoAttivo = 0.2f; a.recupero = 0.6f;
            a.portata = 2.4f; a.raggio = 1.6f; a.arco = 160f; a.affondo = 2f;
            a.probabilitaCritico = 5f;
            a.dannoAssorbitoSenzaScudo = 0.3f; a.costoParataSenzaScudo = 35f;
        });

        // ---------- Scudi ----------

        creati += Scudo("scudo-della-vecchia-vita", "Scudo della vecchia vita", "scudo_vecchia_vita", s =>
        {
            s.taglia = DatiScudo.Taglia.Medio;
            s.dannoAssorbito = 0.6f; s.costoColpoParato = 25f; s.arcoParata = 120f;
            s.costoSchivataExtra = 5f; s.moltiplicatoreVelocita = 0.97f;
        });
        creati += Scudo("pavese-di-quercia", "Pavese di quercia", "pavese_quercia", s =>
        {
            s.taglia = DatiScudo.Taglia.Grande;
            s.dannoAssorbito = 0.85f; s.costoColpoParato = 22f; s.arcoParata = 160f;
            s.costoSchivataExtra = 15f; s.moltiplicatoreVelocita = 0.9f;
        });
        creati += Scudo("brocchiere-di-ferro", "Brocchiere di ferro", "brocchiere_ferro", s =>
        {
            s.taglia = DatiScudo.Taglia.Piccolo;
            s.dannoAssorbito = 0.3f; s.costoColpoParato = 25f; s.arcoParata = 100f;
            s.finestraParataPerfetta = 0.15f; s.sbilanciamento = 0.8f; s.moltiplicatoreDannoSbilanciato = 1.5f;
            s.costoSchivataExtra = 3f; s.moltiplicatoreVelocita = 0.98f;
        });

        // ---------- Armature ----------

        creati += Armatura("giubba-di-cuoio", "Giubba di cuoio imbottito", "giubba_cuoio", b =>
        {
            b.peso = DatiArmatura.Peso.Leggera;
            b.armatura = 10f; b.costoSchivataExtra = 3.5f; b.moltiplicatoreVelocita = 0.98f;
        });
        creati += Armatura("cotta-di-maglia-rattoppata", "Cotta di maglia rattoppata", "cotta_maglia", b =>
        {
            b.peso = DatiArmatura.Peso.Media;
            b.armatura = 25f; b.costoSchivataExtra = 6.5f; b.moltiplicatoreVelocita = 0.95f;
        });
        creati += Armatura("corazza-di-piastre-annerite", "Corazza di piastre annerite", "corazza_piastre", b =>
        {
            b.peso = DatiArmatura.Peso.Pesante;
            b.armatura = 45f; b.costoSchivataExtra = 12f; b.moltiplicatoreVelocita = 0.88f;
        });

        // ---------- Amuleti ----------

        // Ogni amuleto ha un bonus e un malus (scelta di Lorenzo).
        // Magici: bonus semplici alle statistiche.
        creati += Amuleto("zanna-di-lupo", "Zanna di lupo", "zanna_lupo", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.bonusDanno = 10f;
            m.malus.resistenzaMassimaPercento = -10f; // resistenza massima -10%
        });
        creati += Amuleto("occhio-di-corvo", "Occhio di corvo", "occhio_corvo", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.probabilitaCritico = 10f;
            m.malus.vitaMassimaPercento = -7f; // vita massima -7%
        });
        creati += Amuleto("pietra-del-focolare", "Pietra del focolare", "pietra_focolare", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.armatura = 15f;
            m.malus.velocitaAttacco = -7.5f; // attacchi il 7,5% più lenti
        });

        // Arcani: effetti speciali.
        creati += Amuleto("cuore-di-brace", "Cuore di brace", "cuore_brace", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.VitaPerUccisione; m.valore = 12f;
            m.malus.bonusDanno = -5f; // danno -5%
        });
        creati += Amuleto("respiro-del-lago", "Respiro del lago", "respiro_lago", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.RecuperoResistenza; m.valore = 30f;
            m.malus.armatura = -8f; m.malus.vitaMassimaPercento = -7f; // -8 armatura e vita massima -7%
        });
        creati += Amuleto("sangue-antico", "Sangue antico", "sangue_antico", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.RubaVita; m.valore = 7f;
            m.malus.vitaMassimaPercento = -15f; // vita massima -15%
        });

        AssetDatabase.SaveAssets();
        Debug.Log(creati > 0
            ? "Oggetti del Guerriero: " + (riscrivi ? "aggiornati " : "creati ") + creati + " file in " + Radice + "."
            : "Oggetti del Guerriero: c'erano già tutti, nessun file cambiato.");
    }

    static int Arma(string file, string nome, string chiave, System.Action<DatiArma> imposta) =>
        Oggetto("Armi", file, nome, chiave, imposta);

    static int Scudo(string file, string nome, string chiave, System.Action<DatiScudo> imposta) =>
        Oggetto("Scudi", file, nome, chiave, imposta);

    static int Armatura(string file, string nome, string chiave, System.Action<DatiArmatura> imposta) =>
        Oggetto("Armature", file, nome, chiave, imposta);

    static int Amuleto(string file, string nome, string chiave, System.Action<DatiAmuleto> imposta) =>
        Oggetto("Amuleti", file, nome, chiave, imposta);

    // Crea il file se manca (restituisce 1) o, con "Aggiorna", gli rimette i valori di questo script (1);
    // altrimenti lo lascia com'è (0).
    static int Oggetto<T>(string cartella, string file, string nome, string chiave, System.Action<T> imposta) where T : DatiOggetto
    {
        string percorsoCartella = Radice + "/" + cartella;
        CreaCartelle(percorsoCartella);
        string percorso = percorsoCartella + "/" + file + ".asset";
        T esistente = AssetDatabase.LoadAssetAtPath<T>(percorso);
        if (esistente != null && !riscrivi) return 0;

        T oggetto = ScriptableObject.CreateInstance<T>();
        oggetto.nomeDiLavoro = nome;
        oggetto.chiaveNome = "oggetto." + chiave + ".nome";
        oggetto.chiaveDescrizione = "oggetto." + chiave + ".descrizione";
        oggetto.classe = ClasseGiocatore.Guerriero;
        imposta(oggetto);

        if (esistente == null)
        {
            AssetDatabase.CreateAsset(oggetto, percorso);
            return 1;
        }

        // Stesso file (stesso .meta, così i collegamenti restano), valori nuovi; icona e modello restano.
        oggetto.icona = esistente.icona;
        oggetto.modello = esistente.modello;
        EditorUtility.CopySerialized(oggetto, esistente);
        esistente.name = file;
        EditorUtility.SetDirty(esistente);
        Object.DestroyImmediate(oggetto);
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
