using UnityEngine;

// Base comune di tutti gli oggetti del gioco che si possono equipaggiare: armi, armature, amuleti.
// A cosa serve: tiene quello che hanno tutti gli oggetti (nome, descrizione, icona, modello 3D, classe),
// così l'inventario di Giuseppe può mostrarli tutti allo stesso modo.
// Come si usa: non si usa da sola. Si crea un'arma, un'armatura o un amuleto (vedi DatiArma, DatiArmatura,
// DatiAmuleto) dal pannello Project: tasto destro > Create > magic-gnl > Oggetti.
// Il nome e la descrizione sono CHIAVI di Lingua (per esempio "oggetto.spada_scudo.nome"): il testo vero,
// nelle 8 lingue, va aggiunto alla tabella di Lingua.cs. Finché manca, si vede il "nome di lavoro".
public abstract class DatiOggetto : ScriptableObject
{
    [Header("Identità")]
    [Tooltip("Nome per noi, in italiano, usato finché non c'è la traduzione in Lingua.cs.")]
    public string nomeDiLavoro = "";
    [Tooltip("Chiave del nome in Lingua.cs, per esempio oggetto.spada_scudo.nome")]
    public string chiaveNome = "";
    [Tooltip("Chiave della descrizione in Lingua.cs, per esempio oggetto.spada_scudo.descrizione")]
    public string chiaveDescrizione = "";
    [Tooltip("Classe che può usare l'oggetto.")]
    public ClasseGiocatore classe = ClasseGiocatore.Guerriero;

    [Header("Aspetto (da collegare quando Nazar avrà fatto il modello)")]
    [Tooltip("Immagine dell'oggetto per l'inventario.")]
    public Sprite icona;
    [Tooltip("Modello 3D dell'oggetto (prefab con l'FBX di Nazar). Vuoto = si usa la forma provvisoria.")]
    public GameObject modello;

    // Nome da mostrare al giocatore, nella lingua scelta.
    public string Nome
    {
        get
        {
            if (string.IsNullOrEmpty(chiaveNome)) return nomeDiLavoro;
            string tradotto = Lingua.T(chiaveNome);
            return tradotto == chiaveNome && !string.IsNullOrEmpty(nomeDiLavoro) ? nomeDiLavoro : tradotto;
        }
    }

    public string Descrizione => string.IsNullOrEmpty(chiaveDescrizione) ? "" : Lingua.T(chiaveDescrizione);
}
