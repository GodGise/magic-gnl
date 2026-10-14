using UnityEngine;

// Fuoco fatuo, evocazione dello Stregone (Docs/incantesimi-stregone.md): una piccola luce che gira attorno allo
// Stregone e ogni "Intervallo Colpi" secondi lancia una scintilla al nemico più vicino. Dura "Durata" secondi;
// al massimo 3 insieme. I nemici non la possono colpire.
// Come montarlo: non si monta. La crea MagiaStregone con FuocoFatuo.Crea.
public class FuocoFatuo : Evocazione
{
    const float Portata = 12f;
    float prossimoColpo;
    float angolo;

    protected override bool Bersagliabile => false;

    public static FuocoFatuo Crea(MagiaStregone chi, DatiIncantesimo inc, Vector3 posto)
    {
        var oggetto = new GameObject("Fuoco fatuo");
        oggetto.transform.position = posto + Vector3.up * 1.6f;
        var fuoco = oggetto.AddComponent<FuocoFatuo>();
        Forma(PrimitiveType.Sphere, oggetto.transform, Vector3.zero, Vector3.one * 0.22f, inc.colore, 3f);
        var luce = oggetto.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.color = inc.colore;
        luce.range = 4f;
        luce.intensity = 1.8f;
        fuoco.angolo = Random.Range(0f, 360f);
        fuoco.Prepara(chi, inc, 1f, inc.durata);
        fuoco.prossimoColpo = Time.time + inc.intervalloColpi;
        return fuoco;
    }

    protected override void Aggiorna()
    {
        // Gira attorno allo Stregone, a mezz'aria.
        angolo += 90f * Time.deltaTime;
        Vector3 meta = padrone.transform.position + Quaternion.Euler(0f, angolo, 0f) * Vector3.forward * 1.2f + Vector3.up * (1.7f + 0.15f * Mathf.Sin(Time.time * 3f));
        transform.position = Vector3.Lerp(transform.position, meta, 1f - Mathf.Exp(-6f * Time.deltaTime));

        if (Time.time < prossimoColpo) return;
        Bersaglio nemico = NemicoVicino(Portata);
        if (nemico == null) { prossimoColpo = Time.time + 0.3f; return; }
        prossimoColpo = Time.time + dati.intervalloColpi;
        Vector3 direzione = (nemico.transform.position + Vector3.up * 0.3f - transform.position).normalized;
        var inc = dati;
        var chi = padrone;
        SferaMagica.LanciaIncantesimo(transform.position, direzione, nemico, 16f, padrone.transform, inc.colore, 0.5f, 2f,
            (b, punto) => { if (b != null && chi != null) chi.DannoNelTempo(inc, b, inc.dannoEvocazione, 1f, punto, true); });
    }
}
