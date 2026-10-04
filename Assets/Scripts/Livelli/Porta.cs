using UnityEngine;

// Porta che si apre scendendo sotto terra (come una lastra di pietra o una grata) oppure salendo.
// A cosa serve: chiusa blocca il passaggio; quando qualcosa la apre (per esempio una Leva) scivola
// via in modo morbido e il passaggio è libero.
// Come montarlo:
//   1. GameObject > 3D Object > Cube. Con Scale dagli la misura del passaggio (per la porta della
//      cripta: X 2, Y 2.8, Z 0.3) e mettilo nel vano della porta, con la base a terra.
//   2. Nell'Inspector clicca Add Component e scegli Porta. Il collider del cubo blocca il passaggio.
//   3. Per aprirla serve una Leva vicina, oppure un altro script che chiama Apri().
public class Porta : MonoBehaviour
{
    public enum Verso { Giu, Su }

    [Tooltip("Da che parte si muove la porta quando si apre.")]
    [SerializeField] Verso verso = Verso.Giu;
    [Tooltip("Di quanto si sposta aprendosi, in metri. Con 0 usa la sua altezza: sparisce tutta.")]
    [SerializeField] float spostamento = 0f;
    [Tooltip("Secondi per aprirsi o chiudersi del tutto.")]
    [SerializeField] float durataApertura = 1.5f;
    [Tooltip("Se attivo, la porta parte già aperta.")]
    [SerializeField] bool apertaAllInizio = false;

    Vector3 posizioneChiusa;
    Vector3 posizioneAperta;
    float progresso; // 0 = chiusa, 1 = aperta
    bool aperta;

    public bool Aperta => aperta;

    void Awake()
    {
        posizioneChiusa = transform.position;
        float distanza = spostamento > 0f ? spostamento : Altezza();
        Vector3 direzione = verso == Verso.Giu ? Vector3.down : Vector3.up;
        posizioneAperta = posizioneChiusa + direzione * distanza;

        aperta = apertaAllInizio;
        progresso = aperta ? 1f : 0f;
        AggiornaPosizione();
    }

    void Update()
    {
        float obiettivo = aperta ? 1f : 0f;
        if (Mathf.Approximately(progresso, obiettivo)) return;

        progresso = Mathf.MoveTowards(progresso, obiettivo, Time.deltaTime / Mathf.Max(0.01f, durataApertura));
        AggiornaPosizione();
    }

    public void Apri()
    {
        if (aperta) return;
        aperta = true;
        Debug.Log(name + " si apre");
    }

    public void Chiudi()
    {
        if (!aperta) return;
        aperta = false;
        Debug.Log(name + " si chiude");
    }

    // SmoothStep: parte piano, accelera e rallenta alla fine, come una pietra pesante.
    void AggiornaPosizione()
    {
        transform.position = Vector3.Lerp(posizioneChiusa, posizioneAperta, Mathf.SmoothStep(0f, 1f, progresso));
    }

    // Altezza della porta, più qualche centimetro così aprendosi scende tutta sotto terra.
    float Altezza()
    {
        Renderer aspetto = GetComponent<Renderer>();
        return (aspetto != null ? aspetto.bounds.size.y : transform.lossyScale.y) + 0.05f;
    }
}
