using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea i file degli oggetti dello Stregone (uccide a distanza con la magia, è fragile:
// poca armatura, più mana; schiva, non para). Armi magiche (bastoni, verga, bastone del lago a due mani),
// libri (nella casella dello Scudo), vesti (leggera, media, pesante) e amuleti (magici e arcani, ognuno con
// un bonus e un malus). Numeri di partenza da Docs/oggetti-stregone.md, regola del mana in Docs/mana.md.
// A cosa serve: ogni oggetto è un file in Assets/Dati/Oggetti/Stregone/ (Armi, Libri, Vesti, Amuleti).
// Il menu "Crea" crea solo i file che MANCANO; il menu "Aggiorna" rimette in tutti i valori scritti qui sotto
// (tenendo icona e modello già collegati).
// Come si usa: menu in alto "magic-gnl > Crea oggetti dello Stregone", poi selezionare il Giocatore e trascinare
// gli oggetti nelle caselle di Equipaggiamento (il libro va nella casella Libro). Con un bastone o una verga
// equipaggiati l'attacco lancia la sfera con i numeri dell'arma e costa mana.
// Le evocazioni non ci sono ancora: libri e amuleti che le allungano hanno già il loro numero.
// Per aggiungere un oggetto nuovo: una riga nel metodo Crea() qui sotto, con i suoi numeri.
public static class CreaOggettiStregone
{
    const string Radice = "Assets/Dati/Oggetti/Stregone";
    static bool riscrivi;

    [MenuItem("magic-gnl/Crea oggetti dello Stregone")]
    static void MenuCrea()
    {
        riscrivi = false;
        Crea();
    }

    [MenuItem("magic-gnl/Aggiorna oggetti dello Stregone (riscrive i valori)")]
    static void MenuAggiorna()
    {
        if (!EditorUtility.DisplayDialog("Oggetti dello Stregone",
            "Rimettere in tutti gli oggetti i valori scritti in CreaOggettiStregone.cs? Le modifiche fatte a mano nell'Inspector andranno perse (icona e modello restano).",
            "Aggiorna", "Annulla")) return;
        riscrivi = true;
        Crea();
        riscrivi = false;
    }

    static void Crea()
    {
        int creati = 0;

        // Riferimenti: giocatore con vita 100, mana 100, critico 10% da ×1,75; orco sgherro con vita 160,
        // armatura 25. Lo Stregone deve uccidere l'orco in circa lo stesso tempo del Guerriero (4,8 s con la spada).
        // Mana: si ricarica da solo dopo una breve pausa (Giocatore Controllo); il mana pieno basta per circa
        // 20 secondi di fuoco continuo con il bastone di partenza.

        // ---------- Armi magiche (ogni lancio costa mana, non resistenza) ----------

        // Bastone: equilibrato, la base dello Stregone.
        creati += Arma("bastone-della-vecchia-vita", "Bastone della vecchia vita", "bastone_vecchia_vita", a =>
        {
            a.tipo = DatiArma.Tipo.Bastone;
            a.danno = 22f; a.costoMana = 3f; a.costoAttacco = 0f;
            a.preparazione = 0.3f; a.colpoAttivo = 0.1f; a.recupero = 0.3f;
            a.portata = 22f; a.velocitaIncantesimo = 16f;
            a.penetrazioneArmatura = 0.15f;
            a.dannoAssorbitoSenzaScudo = 0.1f; a.costoParataSenzaScudo = 30f;
        });
        creati += Arma("bastone-di-quercia-nera", "Bastone di quercia nera", "bastone_quercia_nera", a =>
        {
            a.tipo = DatiArma.Tipo.Bastone;
            a.danno = 26f; a.costoMana = 3.5f; a.costoAttacco = 0f;
            a.preparazione = 0.32f; a.colpoAttivo = 0.1f; a.recupero = 0.32f;
            a.portata = 22f; a.velocitaIncantesimo = 16f;
            a.penetrazioneArmatura = 0.15f; a.probabilitaCritico = 5f;
            a.dannoAssorbitoSenzaScudo = 0.1f; a.costoParataSenzaScudo = 30f;
        });
        creati += Arma("bastone-d-ossidiana", "Bastone d'ossidiana", "bastone_ossidiana", a =>
        {
            a.tipo = DatiArma.Tipo.Bastone;
            a.danno = 30f; a.costoMana = 4f; a.costoAttacco = 0f;
            a.preparazione = 0.35f; a.colpoAttivo = 0.1f; a.recupero = 0.35f;
            a.portata = 24f; a.velocitaIncantesimo = 16f;
            a.penetrazioneArmatura = 0.25f; a.probabilitaCritico = 5f;
            a.dannoAssorbitoSenzaScudo = 0.1f; a.costoParataSenzaScudo = 30f;
        });

        // Verga: leggera e velocissima, poco danno a colpo.
        creati += Arma("verga-d-osso", "Verga d'osso", "verga_osso", a =>
        {
            a.tipo = DatiArma.Tipo.Verga;
            a.danno = 15f; a.costoMana = 2f; a.costoAttacco = 0f;
            a.preparazione = 0.15f; a.colpoAttivo = 0.1f; a.recupero = 0.2f;
            a.portata = 18f; a.velocitaIncantesimo = 20f;
            a.penetrazioneArmatura = 0.1f; a.probabilitaCritico = 5f;
            a.dannoAssorbitoSenzaScudo = 0.1f; a.costoParataSenzaScudo = 30f;
        });

        // Bastone del lago: a due mani (niente libro), lento, portata e danno altissimi.
        creati += Arma("bastone-del-lago", "Bastone del lago", "bastone_lago", a =>
        {
            a.tipo = DatiArma.Tipo.Bastone; a.dueMani = true;
            a.danno = 48f; a.costoMana = 8f; a.costoAttacco = 0f;
            a.preparazione = 0.55f; a.colpoAttivo = 0.1f; a.recupero = 0.65f;
            a.portata = 30f; a.velocitaIncantesimo = 13f;
            a.penetrazioneArmatura = 0.35f; a.probabilitaCritico = 5f;
            a.dannoAssorbitoSenzaScudo = 0.15f; a.costoParataSenzaScudo = 35f;
        });

        // ---------- Libri (nella casella dello Scudo) ----------

        creati += Libro("libro-della-vecchia-vita", "Libro della vecchia vita", "libro_vecchia_vita", l =>
        {
            l.manaMassimo = 20f;
        });
        creati += Libro("libro-dei-sussurri", "Libro dei sussurri", "libro_sussurri", l =>
        {
            l.manaMassimo = 30f; l.recuperoMana = 10f;
        });
        creati += Libro("libro-delle-braci", "Libro delle braci", "libro_braci", l =>
        {
            l.manaMassimo = 20f; l.potenzaIncantesimi = 15f;
        });
        creati += Libro("libro-dell-evocatore", "Libro dell'evocatore", "libro_evocatore", l =>
        {
            l.manaMassimo = 25f; l.durataEvocazioni = 40f;
        });
        creati += Libro("libro-del-lago-nero", "Libro del lago nero", "libro_lago_nero", l =>
        {
            l.manaMassimo = 50f; l.recuperoMana = 20f; l.potenzaIncantesimi = 10f;
            l.costoSchivataExtra = 4f; l.moltiplicatoreVelocita = 0.99f;
        });

        // ---------- Vesti (pochissima armatura, più mana) ----------

        creati += Veste("tunica-stracciata", "Tunica stracciata", "tunica_stracciata", b =>
        {
            b.peso = DatiArmatura.Peso.Leggera;
            b.armatura = 3f; b.manaMassimo = 0f; b.costoSchivataExtra = 0f; b.moltiplicatoreVelocita = 1f;
        });
        creati += Veste("veste-dell-evocatore", "Veste dell'evocatore", "veste_evocatore", b =>
        {
            b.peso = DatiArmatura.Peso.Media;
            b.armatura = 6f; b.manaMassimo = 15f; b.costoSchivataExtra = 0f; b.moltiplicatoreVelocita = 1f;
        });
        creati += Veste("manto-di-cenere", "Manto di cenere", "manto_cenere", b =>
        {
            b.peso = DatiArmatura.Peso.Pesante;
            b.armatura = 10f; b.manaMassimo = 25f; b.costoSchivataExtra = 2f; b.moltiplicatoreVelocita = 0.99f;
        });

        // ---------- Amuleti (ognuno con un bonus e un malus) ----------

        // Magici: bonus semplici alle statistiche.
        creati += Amuleto("osso-inciso", "Osso inciso", "osso_inciso", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.manaMassimo = 15f;
            m.malus.vitaMassimaPercento = -7f;
        });
        creati += Amuleto("cristallo-opaco", "Cristallo opaco", "cristallo_opaco", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.recuperoMana = 15f;
            m.malus.velocitaAttacco = -7.5f; // lanci il 7,5% più lenti
        });
        creati += Amuleto("cenere-benedetta", "Cenere benedetta", "cenere_benedetta", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.potenzaIncantesimi = 12f;
            m.malus.manaMassimoPercento = -10f;
        });

        // Arcani: effetti speciali.
        creati += Amuleto("sigillo-del-focolare", "Sigillo del focolare", "sigillo_focolare", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.ManaPerUccisione; m.valore = 8f;
            m.malus.bonusDanno = -5f;
        });
        creati += Amuleto("occhio-del-lago", "Occhio del lago", "occhio_lago", m =>
        {
            // Le evocazioni non ci sono ancora: il numero è pronto per quando si faranno.
            m.tipo = DatiAmuleto.Tipo.Arcano; m.bonus.durataEvocazioni = 50f;
            m.malus.vitaMassimaPercento = -15f;
        });
        creati += Amuleto("cuore-del-lago-nero", "Cuore del lago nero", "cuore_lago_nero", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.RubaMana; m.valore = 5f;
            m.malus.vitaMassimaPercento = -20f;
        });

        AssetDatabase.SaveAssets();
        Debug.Log(creati > 0
            ? "Oggetti dello Stregone: " + (riscrivi ? "aggiornati " : "creati ") + creati + " file in " + Radice + "."
            : "Oggetti dello Stregone: c'erano già tutti, nessun file cambiato.");
    }

    static int Arma(string file, string nome, string chiave, System.Action<DatiArma> imposta) =>
        Oggetto("Armi", file, nome, chiave, imposta);

    static int Libro(string file, string nome, string chiave, System.Action<DatiLibro> imposta) =>
        Oggetto("Libri", file, nome, chiave, imposta);

    static int Veste(string file, string nome, string chiave, System.Action<DatiArmatura> imposta) =>
        Oggetto("Vesti", file, nome, chiave, imposta);

    static int Amuleto(string file, string nome, string chiave, System.Action<DatiAmuleto> imposta) =>
        Oggetto("Amuleti", file, nome, chiave, imposta);

    // Crea il file se manca (restituisce 1); con "Aggiorna" rimette i valori in quello che c'è già, tenendo
    // icona e modello (restituisce 1); altrimenti 0.
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
        oggetto.classe = ClasseGiocatore.Stregone;
        imposta(oggetto);

        if (esistente == null)
        {
            AssetDatabase.CreateAsset(oggetto, percorso);
            return 1;
        }

        // Aggiorna: valori nuovi, ma icona e modello restano quelli già collegati.
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
