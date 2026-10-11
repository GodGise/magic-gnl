using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea le armi improvvisate della gattabuia (Docs/gattabuia.md) e gli amuleti deboli del
// bottino del Carceriere (in Assets/Dati/Oggetti/Comuni/Amuleti). Le armi improvvisate possono usare tutte le
// classi senza distinzioni (casella "Tutte Le Classi" di DatiOggetto). Sono armi raccolte da terra: solo danno e
// numeri fissi, niente critico in più, niente colpo alle spalle, niente bonus.
// I file vanno in Assets/Dati/Oggetti/Comuni/Armi. Il menu crea solo quelli che mancano; "Aggiorna" rimette in tutti
// i valori scritti qui sotto (tenendo icona e modello).
// Lo Stregone le tiene al posto del bastone e combatte corpo a corpo finché non rimette un bastone.
// Come si usa: menu in alto "magic-gnl > Crea armi improvvisate".
public static class CreaOggettiComuni
{
    const string Radice = "Assets/Dati/Oggetti/Comuni/Armi";
    static bool riscrivi;

    [MenuItem("magic-gnl/Crea armi improvvisate")]
    static void MenuCrea()
    {
        riscrivi = false;
        Crea();
    }

    [MenuItem("magic-gnl/Aggiorna armi improvvisate (riscrive i valori)")]
    static void MenuAggiorna()
    {
        if (!EditorUtility.DisplayDialog("Armi improvvisate",
            "Rimettere nelle armi improvvisate i valori scritti in CreaOggettiComuni.cs? Le modifiche fatte a mano nell'Inspector andranno perse (icona e modello restano).",
            "Aggiorna", "Annulla")) return;
        riscrivi = true;
        Crea();
        riscrivi = false;
    }

    static void Crea()
    {
        int creati = 0;
        // Riferimento: la Spada della vecchia vita fa 25 di danno, costa 20 di resistenza e un colpo dura 0,75 s
        // (circa 33 di danno al secondo). Le armi improvvisate devono essere chiaramente peggiori delle armi vere.

        // Osso lungo: un femore raccolto fra i cadaveri. Corto e leggero, colpi rapidi ma deboli; si para male.
        // Circa 23 di danno al secondo, costa poca resistenza. Un po' di penetrazione perché è una botta, non un taglio.
        creati += Arma("osso-lungo", "Osso lungo", "osso_lungo", a =>
        {
            a.tipo = DatiArma.Tipo.Mazza;
            a.danno = 14f; a.costoAttacco = 13f;
            a.preparazione = 0.2f; a.colpoAttivo = 0.12f; a.recupero = 0.28f;
            a.portata = 1.5f; a.raggio = 1f; a.arco = 100f; a.affondo = 2.5f;
            a.penetrazioneArmatura = 0.1f;
            a.dannoAssorbitoSenzaScudo = 0.2f; a.costoParataSenzaScudo = 22f;
        });

        // Catenaccio: una catena con un lucchetto in fondo, strappata dal muro di una cella. Lenta e faticosa, ma
        // arriva lontano e spazza un arco largo (prende più nemici). Il lucchetto di ferro passa un po' l'armatura.
        // Con una catena non si para quasi niente. Circa 19 di danno al secondo, ma su più nemici.
        creati += Arma("catenaccio", "Catenaccio", "catenaccio", a =>
        {
            a.tipo = DatiArma.Tipo.Mazza;
            a.danno = 22f; a.costoAttacco = 22f;
            a.preparazione = 0.42f; a.colpoAttivo = 0.2f; a.recupero = 0.55f;
            a.portata = 2.4f; a.raggio = 1.3f; a.arco = 160f; a.affondo = 1.5f;
            a.penetrazioneArmatura = 0.2f;
            a.dannoAssorbitoSenzaScudo = 0.1f; a.costoParataSenzaScudo = 30f;
        });

        // Mannaia del macellaio: la lama larga degli orchi macellai (zona 3). La più forte delle armi improvvisate:
        // taglia bene da vicino, ma è corta e non passa l'armatura. Circa 26 di danno al secondo.
        creati += Arma("mannaia-del-macellaio", "Mannaia del macellaio", "mannaia_macellaio", a =>
        {
            a.tipo = DatiArma.Tipo.Ascia;
            a.danno = 23f; a.costoAttacco = 18f;
            a.preparazione = 0.3f; a.colpoAttivo = 0.15f; a.recupero = 0.42f;
            a.portata = 1.6f; a.raggio = 1f; a.arco = 110f; a.affondo = 2f;
            a.penetrazioneArmatura = 0f;
            a.dannoAssorbitoSenzaScudo = 0.25f; a.costoParataSenzaScudo = 22f;
        });

        // Pugnale arrugginito: una lama corta e mangiata dalla ruggine. Il più veloce e il meno faticoso, ma colpisce
        // pochissimo e bisogna stare addosso al nemico. Niente colpo alle spalle in più: non è un pugnale da Ladro.
        // Circa 22 di danno al secondo.
        creati += Arma("pugnale-arrugginito", "Pugnale arrugginito", "pugnale_arrugginito", a =>
        {
            a.tipo = DatiArma.Tipo.Pugnale;
            a.danno = 9f; a.costoAttacco = 9f;
            a.preparazione = 0.12f; a.colpoAttivo = 0.08f; a.recupero = 0.2f;
            a.portata = 1.1f; a.raggio = 0.8f; a.arco = 70f; a.affondo = 3f;
            a.penetrazioneArmatura = 0f;
            a.dannoAssorbitoSenzaScudo = 0.1f; a.costoParataSenzaScudo = 25f;
        });

        // Bottino del Carceriere (a caso, Docs/gattabuia.md): due armi di fortuna un po' migliori di quelle della gattabuia.
        // Mazza chiodata: un bastone pesante pieno di chiodi. Circa 27 di danno al secondo; i chiodi passano un po' l'armatura.
        creati += Arma("mazza-chiodata", "Mazza chiodata", "mazza_chiodata", a =>
        {
            a.tipo = DatiArma.Tipo.Mazza;
            a.danno = 24f; a.costoAttacco = 19f;
            a.preparazione = 0.32f; a.colpoAttivo = 0.15f; a.recupero = 0.43f;
            a.portata = 1.8f; a.raggio = 1.1f; a.arco = 120f; a.affondo = 2f;
            a.penetrazioneArmatura = 0.15f;
            a.dannoAssorbitoSenzaScudo = 0.25f; a.costoParataSenzaScudo = 22f;
        });

        // Mannaia affilata: come la mannaia del macellaio ma ripassata sulla pietra. Più danno, un po' più lenta della
        // mazza chiodata (0,98 s contro 0,90 s a colpo). Circa 27 di danno al secondo, niente penetrazione.
        creati += Arma("mannaia-affilata", "Mannaia affilata", "mannaia_affilata", a =>
        {
            a.tipo = DatiArma.Tipo.Ascia;
            a.danno = 26f; a.costoAttacco = 20f;
            a.preparazione = 0.35f; a.colpoAttivo = 0.16f; a.recupero = 0.47f;
            a.portata = 1.6f; a.raggio = 1f; a.arco = 110f; a.affondo = 2f;
            a.penetrazioneArmatura = 0f;
            a.dannoAssorbitoSenzaScudo = 0.25f; a.costoParataSenzaScudo = 22f;
        });

        // Amuleti deboli del bottino a caso del Carceriere: roba da poco ma utile all'inizio. Gli amuleti sono già in
        // comune fra le classi. Sono deboli, quindi per ora senza malus.
        creati += Amuleto("dente-d-orco", "Dente d'orco", "dente_orco", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.VitaPerUccisione; m.valore = 3f;
        });
        creati += Amuleto("lacci-di-cuoio", "Lacci di cuoio", "lacci_cuoio", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.RecuperoResistenza; m.valore = 10f;
        });
        creati += Amuleto("pietra-torbida-del-lago", "Pietra torbida del lago", "pietra_torbida", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.ManaPerUccisione; m.valore = 3f;
        });

        AssetDatabase.SaveAssets();
        Debug.Log(creati > 0 ? "Armi improvvisate: " + creati + " file in " + Radice + "." : "Armi improvvisate: c'erano già tutte.");
    }

    // Crea il file se manca (1) o, con "Aggiorna", gli rimette i valori di questo script (1); altrimenti niente (0).
    static int Arma(string file, string nome, string chiave, System.Action<DatiArma> imposta)
    {
        CreaCartelle(Radice);
        string percorso = Radice + "/" + file + ".asset";
        var esistente = AssetDatabase.LoadAssetAtPath<DatiArma>(percorso);
        if (esistente != null && !riscrivi) return 0;

        var arma = ScriptableObject.CreateInstance<DatiArma>();
        arma.nomeDiLavoro = nome;
        arma.chiaveNome = "oggetto." + chiave + ".nome";
        arma.chiaveDescrizione = "oggetto." + chiave + ".descrizione";
        arma.classe = ClasseGiocatore.Guerriero;   // non conta: la usano tutte le classi
        arma.tutteLeClassi = true;
        arma.probabilitaCritico = 0f;
        arma.moltiplicatoreCritico = 0f;
        arma.moltiplicatoreAlleSpalle = 1f;
        imposta(arma);

        if (esistente == null)
        {
            AssetDatabase.CreateAsset(arma, percorso);
            return 1;
        }
        arma.icona = esistente.icona;
        arma.modello = esistente.modello;
        arma.materialeModello = esistente.materialeModello;
        arma.rotazioneModello = esistente.rotazioneModello;
        arma.posizioneModello = esistente.posizioneModello;
        arma.scalaModello = esistente.scalaModello;
        EditorUtility.CopySerialized(arma, esistente);
        esistente.name = file;
        EditorUtility.SetDirty(esistente);
        Object.DestroyImmediate(arma);
        return 1;
    }

    // Come Arma, per gli amuleti comuni (Assets/Dati/Oggetti/Comuni/Amuleti).
    static int Amuleto(string file, string nome, string chiave, System.Action<DatiAmuleto> imposta)
    {
        const string cartella = "Assets/Dati/Oggetti/Comuni/Amuleti";
        CreaCartelle(cartella);
        string percorso = cartella + "/" + file + ".asset";
        var esistente = AssetDatabase.LoadAssetAtPath<DatiAmuleto>(percorso);
        if (esistente != null && !riscrivi) return 0;

        var amuleto = ScriptableObject.CreateInstance<DatiAmuleto>();
        amuleto.nomeDiLavoro = nome;
        amuleto.chiaveNome = "oggetto." + chiave + ".nome";
        amuleto.chiaveDescrizione = "oggetto." + chiave + ".descrizione";
        amuleto.classe = ClasseGiocatore.Guerriero;   // non conta: gli amuleti sono di tutte le classi
        imposta(amuleto);

        if (esistente == null)
        {
            AssetDatabase.CreateAsset(amuleto, percorso);
            return 1;
        }
        amuleto.icona = esistente.icona;
        amuleto.modello = esistente.modello;
        EditorUtility.CopySerialized(amuleto, esistente);
        esistente.name = file;
        EditorUtility.SetDirty(esistente);
        Object.DestroyImmediate(amuleto);
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
