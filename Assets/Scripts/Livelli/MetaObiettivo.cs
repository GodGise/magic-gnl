using UnityEngine;

// Il punto dove deve andare il giocatore per l'obiettivo attuale: la bussola in alto lo indica con un rombo.
// A cosa serve: dire alla bussola "adesso l'obiettivo è qui" (il capanno, il camino, la fucina...).
// Come montarlo: sull'oggetto da indicare (o su un oggetto vuoto nel punto giusto). Poi, nell'evento che fa
// iniziare quel passo della quest (per esempio "Al Termine" dell'Interlocutore), trascina questo oggetto e scegli
// MetaObiettivo > Attiva. La meta resta finché un'altra non prende il suo posto o finché Missioni > TogliObiettivo.
public class MetaObiettivo : MonoBehaviour
{
    public void Attiva()
    {
        if (Missioni.Istanza != null) Missioni.Istanza.ImpostaMeta(transform);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.95f, 0.63f, 0.3f, 0.9f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, new Vector3(0.5f, 0.5f, 0.5f));
    }
}
