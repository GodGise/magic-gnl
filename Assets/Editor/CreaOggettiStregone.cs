using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea i file degli oggetti dello Stregone, con i numeri decisi da Lorenzo il 9 ottobre
// (Docs/incantesimi-stregone.md): 12 incantesimi in 4 scuole (Brace, Lago Nero, Ombra, Evocazione), 6 bastoni,
// 5 libri, 3 vesti e 6 amuleti. Bastoni, libri, vesti e amuleti non fanno danno: cambiano gli incantesimi con un pro
// e un contro (sezione "Magia" di ogni oggetto, vedi ModificatoriMagia).
// A cosa serve: ogni oggetto è un file in Assets/Dati/Oggetti/Stregone/ (Incantesimi, Bastoni, Libri, Vesti, Amuleti).
// Il menu "Crea" crea solo i file che MANCANO; il menu "Aggiorna" rimette in tutti i valori scritti qui sotto
// (tenendo icona e modello già collegati).
// Come si usa: menu in alto "magic-gnl > Crea oggetti dello Stregone". Per provarli: scegliere lo Stregone (menu
// "magic-gnl > Prova: gioca come Stregone"), poi in Play riempire lo zaino e aprire l'inventario con Tab.
// Per aggiungere un oggetto nuovo: una riga nel metodo Crea() qui sotto, con i suoi numeri.
public static class CreaOggettiStregone
{
    const string Radice = "Assets/Dati/Oggetti/Stregone";
    static bool riscrivi;

    static readonly Color Fuoco = new Color(1f, 0.55f, 0.2f);
    static readonly Color FuocoForte = new Color(1f, 0.35f, 0.1f);
    static readonly Color Ghiaccio = new Color(0.55f, 0.85f, 1f);
    static readonly Color AcquaNera = new Color(0.08f, 0.1f, 0.18f);
    static readonly Color Ombra = new Color(0.45f, 0.25f, 0.7f);
    static readonly Color Nebbia = new Color(0.75f, 0.78f, 0.82f);
    static readonly Color Spirito = new Color(0.55f, 0.8f, 1f);

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

    internal static void Crea()
    {
        int creati = 0;
        // Riferimenti: Stregone con vita 100 e mana 100; il mana si ricarica 2 s dopo l'ultimo incantesimo, 100 punti
        // in 17 s. Orco sgherro: vita 160, armatura 25.

        // ---------- Incantesimi: Brace (tanto danno, anche ad area) ----------

        creati += Incantesimo("scintilla", "Scintilla", "scintilla", i =>
        {
            i.scuola = ScuolaMagia.Brace; i.effetto = DatiIncantesimo.Effetto.Proiettile;
            i.costoMana = 5f; i.danno = 21f; i.carica = 0.3f; i.recupero = 0.3f; i.attesa = 0.5f;
            i.velocita = 20f; i.portata = 22f; i.grandezza = 0.8f; i.colore = Fuoco;
        });
        creati += Incantesimo("palla-di-fuoco", "Palla di fuoco", "palla_fuoco", i =>
        {
            i.scuola = ScuolaMagia.Brace; i.effetto = DatiIncantesimo.Effetto.PallaDiFuoco;
            i.costoMana = 18f; i.danno = 32f; i.carica = 0.7f; i.recupero = 0.5f; i.attesa = 1.3f;
            i.raggio = 3f; i.velocita = 11f; i.portata = 22f; i.grandezza = 1.8f; i.colore = FuocoForte;
        });
        creati += Incantesimo("scia-di-brace", "Scia di brace", "scia_brace", i =>
        {
            i.scuola = ScuolaMagia.Brace; i.effetto = DatiIncantesimo.Effetto.Scia;
            i.costoMana = 22f; i.dannoAlSecondo = 11f; i.carica = 0.7f; i.recupero = 0.6f; i.attesa = 10f;
            i.lunghezza = 6f; i.raggio = 1.5f; i.durata = 6f; i.colore = Fuoco;
        });

        // ---------- Incantesimi: Lago Nero (controllo) ----------

        creati += Incantesimo("scheggia-di-ghiaccio", "Scheggia di ghiaccio", "scheggia_ghiaccio", i =>
        {
            i.scuola = ScuolaMagia.LagoNero; i.effetto = DatiIncantesimo.Effetto.Proiettile;
            i.costoMana = 7f; i.danno = 15f; i.carica = 0.3f; i.recupero = 0.3f; i.attesa = 1.5f;
            i.rallentamento = 35f; i.durataRallentamento = 1.6f;
            i.velocita = 24f; i.portata = 22f; i.grandezza = 0.8f; i.colore = Ghiaccio;
        });
        creati += Incantesimo("onda-del-lago", "Onda del lago", "onda_lago", i =>
        {
            i.scuola = ScuolaMagia.LagoNero; i.effetto = DatiIncantesimo.Effetto.Onda;
            i.costoMana = 20f; i.danno = 20f; i.carica = 0.5f; i.recupero = 0.6f; i.attesa = 10f;
            i.altezza = 4f; i.lunghezza = 6f; i.raggio = 4f; i.spinta = 5f; i.stordimento = 1.5f;
            i.rallentamento = 30f; i.durataRallentamento = 2f; i.colore = Ghiaccio;
        });
        creati += Incantesimo("pozza-nera", "Pozza nera", "pozza_nera", i =>
        {
            i.scuola = ScuolaMagia.LagoNero; i.effetto = DatiIncantesimo.Effetto.Pozza;
            i.costoMana = 27f; i.danno = 3f; i.carica = 0.3f; i.recupero = 0.5f; i.attesa = 20f;
            i.raggio = 4f; i.portata = 18f; i.blocco = 3.5f; i.rallentamentoDopo = 20f; i.durataRallentamentoDopo = 3f;
            i.colore = AcquaNera;
        });

        // ---------- Incantesimi: Ombra (colpire senza farsi vedere, scappare) ----------

        creati += Incantesimo("dardo-d-ombra", "Dardo d'ombra", "dardo_ombra", i =>
        {
            i.scuola = ScuolaMagia.Ombra; i.effetto = DatiIncantesimo.Effetto.Proiettile;
            i.costoMana = 8f; i.danno = 20f; i.carica = 0.2f; i.recupero = 0.3f; i.attesa = 1f;
            i.moltiplicatoreIgnaro = 2f; i.velocita = 30f; i.portata = 24f; i.grandezza = 0.6f; i.colore = Ombra;
        });
        creati += Incantesimo("passo-d-ombra", "Passo d'ombra", "passo_ombra", i =>
        {
            i.scuola = ScuolaMagia.Ombra; i.effetto = DatiIncantesimo.Effetto.PassoOmbra;
            i.costoMana = 40f; i.carica = 0.1f; i.recupero = 0.2f; i.attesa = 30f;
            i.durata = 4f; i.velocitaInPiu = 40f; i.moltiplicatoreSuccessivo = 2.5f;
            i.rallentamentoInterruzione = 50f; i.durataRallentamentoInterruzione = 2f; i.colore = Ombra;
        });
        creati += Incantesimo("velo-di-nebbia", "Velo di nebbia", "velo_nebbia", i =>
        {
            i.scuola = ScuolaMagia.Ombra; i.effetto = DatiIncantesimo.Effetto.Velo;
            i.costoMana = 22f; i.carica = 0.4f; i.recupero = 0.4f; i.attesa = 20f;
            i.raggio = 5f; i.durata = 6f; i.colore = Nebbia;
        });

        // ---------- Incantesimi: Evocazione (aiutanti; l'attesa parte quando l'evocazione finisce) ----------

        creati += Incantesimo("fuoco-fatuo", "Fuoco fatuo", "fuoco_fatuo", i =>
        {
            i.scuola = ScuolaMagia.Evocazione; i.effetto = DatiIncantesimo.Effetto.FuocoFatuo;
            i.costoMana = 15f; i.carica = 0.7f; i.recupero = 0.4f; i.attesa = 7f;
            i.durata = 20f; i.dannoEvocazione = 6f; i.intervalloColpi = 1.5f; i.massimoInsieme = 3; i.colore = new Color(0.6f, 0.9f, 1f);
        });
        creati += Incantesimo("spirito-del-lupo", "Spirito del lupo", "spirito_lupo", i =>
        {
            i.scuola = ScuolaMagia.Evocazione; i.effetto = DatiIncantesimo.Effetto.Lupo;
            i.costoMana = 35f; i.carica = 2f; i.recupero = 0.6f; i.attesa = 60f;
            i.durata = 45f; i.vitaEvocazione = 60f; i.dannoEvocazione = 14f; i.intervalloColpi = 1.2f;
            i.vitaPerUccisione = 10f; i.lunghezzaScia = 1f; i.dannoAlSecondo = 5f; i.massimoInsieme = 1; i.colore = Spirito;
        });
        creati += Incantesimo("bambola-di-ossa", "Bambola di ossa", "bambola_ossa", i =>
        {
            i.scuola = ScuolaMagia.Evocazione; i.effetto = DatiIncantesimo.Effetto.Bambola;
            i.costoMana = 40f; i.carica = 0.5f; i.recupero = 0.5f; i.attesa = 60f;
            i.vitaEvocazione = 150f; i.raggioAttrazione = 8f; i.tempoTrasformazione = 20f;
            i.raggio = 1.5f; i.rallentamento = 35f; i.durataRallentamento = 3f; i.dannoAlSecondo = 6f;
            i.vitaScheletro = 45f; i.dannoScheletro = 11f; i.intervalloScheletro = 1.6f; i.durataScheletro = 30f;
            i.massimoInsieme = 1; i.colore = new Color(0.86f, 0.82f, 0.7f);
        });

        // ---------- Bastoni (non fanno danno: pro e contro sugli incantesimi) ----------

        creati += Bastone("bastone-della-vecchia-vita", "Bastone della vecchia vita", "bastone_vecchia_vita", b =>
        {
            b.magia.caricaSecondi = 0.5f;   // di partenza: nessun pro, ogni incantesimo si carica 0,5 s in più
        });
        creati += Bastone("bastone-di-quercia-nera", "Bastone di quercia nera", "bastone_quercia_nera", b =>
        {
            b.magia.brace.dannoPercento = 15f;
            b.magia.lagoNero.durataEffettiPercento = -25f;
        });
        creati += Bastone("bastone-del-lago", "Bastone del lago", "bastone_lago", b =>
        {
            b.dueMani = true;   // niente libro
            b.magia.lagoNero.durataEffettiPercento = 30f;
            b.magia.lagoNero.dannoPercento = 15f;
            b.magia.costoPercento = 17f;
        });
        creati += Bastone("bastone-d-ossidiana", "Bastone d'ossidiana", "bastone_ossidiana", b =>
        {
            b.magia.ombra.ignoraArmatura = 0.25f;
            b.magia.costoPercento = 15f;
        });
        creati += Bastone("bastone-del-cimitero", "Bastone del cimitero", "bastone_cimitero", b =>
        {
            b.magia.vitaEvocazioniPercento = 25f;
            b.magia.durataEvocazioniPercento = 25f;
            b.magia.dannoPercento = -20f;   // incantesimi che fanno danno (non le evocazioni)
        });
        creati += Bastone("verga-d-osso", "Verga d'osso", "verga_osso", b =>
        {
            b.tipo = DatiBastone.Tipo.Verga;
            b.magia.costoPercento = -15f;
            b.magia.dannoPercento = -5f;
            b.magia.caricaPercento = 10f;
            b.magia.recuperoPercento = 10f;
        });

        // ---------- Libri (nella casella dello Scudo) ----------

        creati += Libro("libro-della-vecchia-vita", "Libro della vecchia vita", "libro_vecchia_vita", l =>
        {
            l.manaMassimo = 30f;
        });
        creati += Libro("libro-dei-sussurri", "Libro dei sussurri", "libro_sussurri", l =>
        {
            l.magia.caselleExtra = 2;
            l.recuperoMana = -25f;
        });
        creati += Libro("libro-delle-braci", "Libro delle braci", "libro_braci", l =>
        {
            l.magia.attesaPercento = -15f;
            l.manaMassimo = -25f;
        });
        creati += Libro("libro-dell-evocatore", "Libro dell'evocatore", "libro_evocatore", l =>
        {
            l.magia.durataEvocazioniPercento = 30f;
            l.magia.attesaPercento = 10f;
            l.recuperoMana = -7f;
        });
        creati += Libro("libro-del-lago-nero", "Libro del lago nero", "libro_lago_nero", l =>
        {
            l.magia.caselleExtra = 1;
            l.manaMassimo = 20f;
            l.costoSchivataExtra = 10f;
            l.magia.costoPercento = 8f;
        });

        // ---------- Vesti (pochissima armatura) ----------

        creati += Veste("tunica-stracciata", "Tunica stracciata", "tunica_stracciata", v =>
        {
            v.peso = DatiArmatura.Peso.Leggera; v.armatura = 3f;
            v.costoSchivataExtra = -5f;              // ogni schivata costa 5 di resistenza in meno
            v.bonus.recuperoMana = 5f;
            v.manaPersoPerDannoPercento = 10f;       // ogni danno ricevuto toglie il 10% del mana massimo
        });
        creati += Veste("veste-dell-evocatore", "Veste dell'evocatore", "veste_evocatore", v =>
        {
            v.peso = DatiArmatura.Peso.Media; v.armatura = 6f;
            v.bonus.manaMassimoPercento = 10f;
            v.magia.caricaEvocazioniPercento = -15f;
            v.moltiplicatoreVelocita = 0.97f;
            v.magia.dannoPercento = -5f;
        });
        creati += Veste("manto-di-cenere", "Manto di cenere", "manto_cenere", v =>
        {
            v.peso = DatiArmatura.Peso.Pesante; v.armatura = 10f;
            v.manaMassimo = 20f;
            v.magia.dannoPercento = 10f;
            v.moltiplicatoreVelocita = 0.95f;
            v.manaPerSchivata = 5f;                  // ogni schivata costa anche 5 di mana
        });

        // ---------- Amuleti (ognuno con un bonus e un malus) ----------

        creati += Amuleto("osso-inciso", "Osso inciso", "osso_inciso", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.manaMassimo = 15f;
            m.malus.vitaMassimaPercento = -7f;
        });
        creati += Amuleto("cristallo-opaco", "Cristallo opaco", "cristallo_opaco", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.bonus.recuperoMana = 10f;
            m.magia.caricaPercento = 10f;            // malus: carica il 10% più lunga
        });
        creati += Amuleto("cenere-benedetta", "Cenere benedetta", "cenere_benedetta", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Magico; m.magia.dannoPercento = 12f;
            m.malus.manaMassimoPercento = -10f;
        });
        creati += Amuleto("sigillo-del-focolare", "Sigillo del focolare", "sigillo_focolare", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.ManaPerUccisione; m.valore = 8f;
            m.malus.bonusDanno = -5f;
        });
        creati += Amuleto("occhio-del-lago", "Occhio del lago", "occhio_lago", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.magia.doppiaEvocazione = true;
            m.malus.vitaMassimaPercento = -25f;
            m.magia.durataEvocazioniPercento = -30f;
        });
        creati += Amuleto("cuore-del-lago-nero", "Cuore del lago nero", "cuore_lago_nero", m =>
        {
            m.tipo = DatiAmuleto.Tipo.Arcano; m.effetto = DatiAmuleto.Effetto.RubaMana; m.valore = 5f;
            m.bonus.vitaPerUccisione = 6f;
            m.malus.vitaMassimaPercento = -30f;
            m.malus.recuperoMana = -10f;
        });

        AssetDatabase.SaveAssets();
        Debug.Log(creati > 0
            ? "Oggetti dello Stregone: " + (riscrivi ? "aggiornati " : "creati ") + creati + " file in " + Radice + "."
            : "Oggetti dello Stregone: c'erano già tutti, nessun file cambiato.");
    }

    static int Incantesimo(string file, string nome, string chiave, System.Action<DatiIncantesimo> imposta) =>
        Oggetto("Incantesimi", file, nome, chiave, imposta);

    static int Bastone(string file, string nome, string chiave, System.Action<DatiBastone> imposta) =>
        Oggetto("Bastoni", file, nome, chiave, imposta);

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
