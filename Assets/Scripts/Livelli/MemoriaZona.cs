using System.Collections.Generic;
using UnityEngine;

// Ricorda com'era una zona in una scena a parte (per esempio la cripta) quando la si lascia, e la rimette così
// rientrando: leve tirate, porte aperte, bauli aperti (senza ridare il contenuto), oggetti raccolti, nemici uccisi.
// Senza, uscendo e rientrando la scena si caricherebbe da capo (e il baule ridarebbe il medaglione).
// Come funziona: usa lo stato che ogni oggetto condiviso sa già dare per il co-op (IOggettoCondiviso: Leva, Porta,
// Baule, OggettoRaccoglibile...) e la morte dei Bersagli. La memoria dura finché si gioca (il salvataggio vero è
// ancora da fare).
// Come montarlo: su un oggetto vuoto della scena della zona (lo mette da solo il comando che crea la zona).
// Lo chiama CaricamentoZona uscendo (Salva); rientrando si rimette da solo.
public class MemoriaZona : MonoBehaviour
{
    // per ogni scena: stato degli oggetti condivisi (per nome completo) e nemici morti
    static readonly Dictionary<string, Dictionary<string, int>> stati = new Dictionary<string, Dictionary<string, int>>();
    static readonly Dictionary<string, HashSet<string>> morti = new Dictionary<string, HashSet<string>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Azzera() { stati.Clear(); morti.Clear(); }

    string Scena => gameObject.scene.name;

    // Si aspetta un fotogramma: leve, porte e bauli creano il loro aspetto nel loro Start.
    System.Collections.IEnumerator Start()
    {
        yield return null;
        if (stati.TryGetValue(Scena, out var salvati))
            foreach (var o in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (o is IOggettoCondiviso condiviso && o.gameObject.scene == gameObject.scene
                    && salvati.TryGetValue(Nome(o.transform), out int stato) && stato != condiviso.StatoRete)
                    condiviso.StatoDaRete(stato);
        if (morti.TryGetValue(Scena, out var uccisi))
            foreach (var b in FindObjectsByType<Bersaglio>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (b.gameObject.scene == gameObject.scene && uccisi.Contains(Nome(b.transform))) b.gameObject.SetActive(false);
    }

    public void Salva()
    {
        var salvati = new Dictionary<string, int>();
        foreach (var o in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (o is IOggettoCondiviso condiviso && o.gameObject.scene == gameObject.scene)
                salvati[Nome(o.transform)] = condiviso.StatoRete;
        stati[Scena] = salvati;
        if (!morti.TryGetValue(Scena, out var uccisi)) morti[Scena] = uccisi = new HashSet<string>();
        foreach (var b in FindObjectsByType<Bersaglio>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b.gameObject.scene == gameObject.scene && (b.Morto || !b.gameObject.activeSelf)) uccisi.Add(Nome(b.transform));
    }

    static string Nome(Transform t)
    {
        var testo = new System.Text.StringBuilder(t.name);
        for (var p = t.parent; p != null; p = p.parent) testo.Insert(0, p.name + "/");
        return testo.ToString();
    }
}
