using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea i file degli oggetti del Guerriero già decisi (armi, armature, amuleti).
// A cosa serve: ogni oggetto è un file in Assets/Dati/Oggetti/Guerriero/ (Armi, Armature, Amuleti), con i suoi
// numeri nell'Inspector. Questo menu crea solo quelli che MANCANO: gli oggetti già creati e magari ritoccati
// a mano nell'Inspector non vengono toccati.
// Come si usa: menu in alto "magic-gnl > Crea oggetti del Guerriero". Poi, per provarne uno, selezionare il
// Giocatore e trascinare l'oggetto nella casella giusta del componente Equipaggiamento.
// Per aggiungere un oggetto nuovo: una riga nel metodo Crea() qui sotto, con i suoi numeri.
public static class CreaOggettiGuerriero
{
    const string Radice = "Assets/Dati/Oggetti/Guerriero";

    [MenuItem("magic-gnl/Crea oggetti del Guerriero")]
    static void Crea()
    {
        int creati = 0;

        // ---------- Armi ----------

        // Spada e scudo: l'arma del Guerriero, ritrovata nel bottino degli orchi durante la fuga.
        // Equilibrata: sono gli stessi numeri usati finora nel prototipo, il punto di riferimento per le altre armi.
        creati += Arma("spada-e-scudo", "Spada e scudo", "spada_scudo", a =>
        {
            a.tipo = DatiArma.Tipo.SpadaEScudo;
            a.danno = 25f; a.costoAttacco = 20f;
            a.preparazione = 0.25f; a.colpoAttivo = 0.15f; a.recupero = 0.35f;
            a.portata = 1.8f; a.raggio = 1.3f; a.arco = 120f; a.affondo = 3f;
            a.probabilitaCritico = 0f; a.moltiplicatoreCritico = 0f;
            a.dannoAssorbitoInParata = 0.9f; a.costoColpoParato = 20f;
        });

        AssetDatabase.SaveAssets();
        Debug.Log(creati > 0
            ? "Oggetti del Guerriero: creati " + creati + " file nuovi in " + Radice + "."
            : "Oggetti del Guerriero: c'erano già tutti, nessun file cambiato.");
    }

    static int Arma(string file, string nome, string chiave, System.Action<DatiArma> imposta) =>
        Oggetto("Armi", file, nome, chiave, imposta);

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
        oggetto.classe = ClasseGiocatore.Guerriero;
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
