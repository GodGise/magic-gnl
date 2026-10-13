using UnityEngine;

// Spegne all'avvio alcuni componenti per le classi non spuntate, lasciando acceso il resto dell'oggetto.
// A cosa serve: Edric serve a tutte le classi (quest 5, la leggenda del lago), ma il suo dialogo e i regali della
// quest 2 (lo studio delle magie) sono solo per lo Stregone. Con questo componente Edric si vede per tutti, mentre
// "Parla" della quest 2 e i regali restano spenti per Guerriero e Ladro. Un componente spento qui si può riaccendere
// dopo da un evento (per esempio Interlocutore > QuestCompiuta per i ringraziamenti).
// Diverso da SoloPerClasse, che spegne tutto l'oggetto.
// Come montarlo: sul personaggio; in "Componenti" i componenti da spegnere, poi le spunte delle classi che li tengono.
[DefaultExecutionOrder(-50)]
public class ComponentiPerClasse : MonoBehaviour
{
    [SerializeField] Behaviour[] componenti;
    [SerializeField] bool guerriero;
    [SerializeField] bool ladro;
    [SerializeField] bool stregone = true;

    void Awake()
    {
        if (SoloPerClasse.Vale(guerriero, ladro, stregone) || componenti == null) return;
        foreach (var c in componenti)
            if (c != null) c.enabled = false;
    }
}
