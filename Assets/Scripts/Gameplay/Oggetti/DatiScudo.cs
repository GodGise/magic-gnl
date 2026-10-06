using UnityEngine;

// Uno scudo del Guerriero: grande, medio o piccolo. Si porta con spada, ascia o mazza (non con lo spadone).
// A cosa serve: decide come si para. È una scelta fra protezione e agilità:
// - Grande: para quasi tutto il danno, costa poca resistenza, copre un arco largo, ma pesa (più lento,
//   schivata più cara).
// - Medio: una via di mezzo.
// - Piccolo: para meno, ma è leggero e permette la PARATA PERFETTA: se il colpo arriva entro pochi istanti
//   da quando si alza lo scudo, non si perde né vita né resistenza e il nemico resta sbilanciato.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Scudo.
[CreateAssetMenu(fileName = "nuovo-scudo", menuName = "magic-gnl/Oggetti/Scudo")]
public class DatiScudo : DatiOggetto
{
    public enum Taglia { Grande, Medio, Piccolo }

    [Header("Tipo")]
    public Taglia taglia = Taglia.Medio;

    [Header("Parata")]
    [Tooltip("Quota di danno fermata parando (0,9 = 90%).")]
    [Range(0f, 1f)] public float dannoAssorbito = 0.85f;
    [Tooltip("Resistenza persa per ogni colpo parato.")]
    public float costoColpoParato = 20f;
    [Tooltip("Ampiezza in gradi dell'arco davanti coperto dallo scudo.")]
    public float arcoParata = 120f;
    [Tooltip("Secondi dall'inizio della parata in cui un colpo viene parato alla perfezione (0 = nessuna parata perfetta).")]
    public float finestraParataPerfetta = 0f;
    [Tooltip("Dopo una parata perfetta, per quanti secondi il nemico resta sbilanciato (non attacca).")]
    public float sbilanciamento = 1.5f;

    [Header("Peso")]
    [Tooltip("Resistenza in più spesa per ogni schivata.")]
    public float costoSchivataExtra = 0f;
    [Tooltip("Velocità di corsa rispetto a quella normale (1 = nessun rallentamento, 0,9 = -10%).")]
    [Range(0.5f, 1f)] public float moltiplicatoreVelocita = 1f;
}
