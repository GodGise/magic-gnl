using System.Collections.Generic;
using UnityEngine;

// Inventario minimo del giocatore: per ora tiene solo le chiavi raccolte.
// A cosa serve: una Chiave raccolta finisce qui, e una Serratura controlla qui se il giocatore
// ha la chiave giusta. Ogni chiave ha un nome-codice (per esempio "chiesa") uguale a quello della serratura.
// È un inizio semplice: un inventario vero (oggetti, pozioni, schermata) è da decidere con il team.
// Come montarlo: non serve montarlo. Chiave e Serratura lo aggiungono da sole al giocatore se manca.
public class Inventario : MonoBehaviour
{
    readonly HashSet<string> chiavi = new HashSet<string>();

    public void AggiungiChiave(string codice) => chiavi.Add(codice);
    public bool HaChiave(string codice) => chiavi.Contains(codice);

    // Trova l'inventario del giocatore, aggiungendolo se non c'è ancora.
    public static Inventario Di(GiocatoreControllo giocatore)
    {
        if (giocatore == null) return null;
        Inventario inventario = giocatore.GetComponent<Inventario>();
        return inventario != null ? inventario : giocatore.gameObject.AddComponent<Inventario>();
    }
}
