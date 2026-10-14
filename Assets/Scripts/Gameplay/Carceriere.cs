using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Il Carceriere, il miniboss della gattabuia (Docs/gattabuia.md). PRIMA VERSIONE "in grigio": usa gli attacchi
// normali di Bersaglio (un colpo con preavviso); le combo, la piroetta e la catena che afferra sono da fare.
// Cosa fa già:
//   - fase 2: sotto "Fase Due Sotto" della vita (48%, cioè 300 su 620 da soli) si ferma 3 secondi a spezzare le
//     catene (in quel momento si può colpire), poi attacca più spesso e fa più danno;
//   - bottino alla sconfitta (regola di Lorenzo, 11 ottobre): sempre la chiave del portone e la Catena del
//     carceriere, più 3 oggetti a caso fra i 5 della lista "Bottino A Caso"; finiscono a terra intorno al corpo;
//   - "Alla Sconfitta": cosa succede quando muore (per esempio l'obiettivo "Apri il portone").
// Come montarlo: su un orco grosso con Bersaglio (Boss acceso) e InseguimentoNemico. Lo prepara il comando
// "magic-gnl > Gattabuia: crea la scena della gattabuia".
[RequireComponent(typeof(Bersaglio))]
public class Carceriere : MonoBehaviour
{
    [Header("Fase 2")]
    [SerializeField, Range(0f, 1f)] float faseDueSotto = 0.48f;
    [SerializeField, Min(0f)] float secondiSpezzaCatene = 3f;
    [Tooltip("In fase 2: di quanto si moltiplica il danno dei colpi.")]
    [SerializeField, Min(0f)] float dannoFaseDue = 1.15f;
    [Tooltip("In fase 2: di quanto si moltiplica il tempo fra un attacco e l'altro (meno di 1 = più spesso).")]
    [SerializeField, Min(0.1f)] float intervalloFaseDue = 0.8f;

    [Header("Bottino")]
    [SerializeField] DatiOggetto[] bottinoSempre;
    [SerializeField] DatiOggetto[] bottinoACaso;
    [SerializeField, Min(0)] int quantiACaso = 3;
    [SerializeField] UnityEvent allaSconfitta;

    Bersaglio bersaglio;
    bool faseDue, finito;

    void Awake() => bersaglio = GetComponent<Bersaglio>();

    void Update()
    {
        if (finito || Rete.Ospite) return;
        if (bersaglio.Morto) { finito = true; Bottino(); allaSconfitta.Invoke(); return; }
        if (!faseDue && bersaglio.Vita <= bersaglio.VitaMassima * faseDueSotto) StartCoroutine(FaseDue());
    }

    IEnumerator FaseDue()
    {
        faseDue = true;
        var vista = GetComponent<InseguimentoNemico>();
        if (vista != null) vista.enabled = false;
        bersaglio.attaccaIlGiocatore = false;
        MessaggiSchermo.Mostra(Lingua.T("msg.gb.catene"), 3f);
        for (float t = 0f; t < secondiSpezzaCatene; t += 0.5f)
        {
            Suoni.Suona(Suono.Serratura, transform.position + Vector3.up * 1.5f, 1f, 0.5f);
            CameraTerzaPersona.Scuoti(0.03f, 0.3f);
            yield return new WaitForSeconds(0.5f);
        }
        Suoni.Suona(Suono.GuardiaRotta, transform.position + Vector3.up * 1.5f, 1f, 0.6f);
        Suoni.Suona(Suono.UrloOrco, transform.position + Vector3.up * 1.5f, 1f, 0.7f);
        Moltiplica("dannoAttacco", dannoFaseDue);
        Moltiplica("intervalloAttacchi", intervalloFaseDue);
        if (vista != null) vista.enabled = true;
        bersaglio.attaccaIlGiocatore = true;
    }

    // Cambia un numero di Bersaglio (campi privati regolati nell'Inspector).
    void Moltiplica(string campo, float per)
    {
        var f = typeof(Bersaglio).GetField(campo, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (f != null && f.GetValue(bersaglio) is float v) f.SetValue(bersaglio, v * per);
    }

    void Bottino()
    {
        var lista = new List<DatiOggetto>();
        if (bottinoSempre != null) lista.AddRange(bottinoSempre);
        if (bottinoACaso != null)
        {
            var scelta = new List<DatiOggetto>(bottinoACaso);
            for (int i = 0; i < quantiACaso && scelta.Count > 0; i++)
            {
                int k = Random.Range(0, scelta.Count);
                lista.Add(scelta[k]);
                scelta.RemoveAt(k);
            }
        }
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i] == null) continue;
            Vector3 p = transform.position + Quaternion.Euler(0f, i * 360f / lista.Count, 0f) * Vector3.forward * 1.6f;
            p.y = 0.05f;
            var o = new GameObject("Bottino: " + lista[i].name);
            o.transform.position = p;
            var r = o.AddComponent<OggettoRaccoglibile>();
            r.Imposta(lista[i]);
        }
        MessaggiSchermo.Mostra(Lingua.T("msg.gb.bottino"), 4f);
    }
}
