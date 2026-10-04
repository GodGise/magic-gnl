using UnityEngine;

// Tipo di pavimento per il suono dei passi.
// A cosa serve: di solito i passi capiscono da soli su cosa si cammina guardando il materiale del
// pavimento (Terreno = erba, Sentiero = terra, Pietra / PietraScura / Lapide / Buio = pietra, Legno = legno).
// Questo componente serve quando vuoi decidere tu, per esempio per un pavimento con un materiale
// diverso, o per una zona intera (l'interno di una casa, una grotta).
// Come montarlo, due modi:
//   - su un pavimento con collider: Add Component > Superficie Sonora e scegli il Tipo;
//   - su una zona: crea un Cube, togli la spunta a Mesh Renderer, nel Box Collider spunta "Is Trigger",
//     ingrandiscilo sulla zona e aggiungi Superficie Sonora. Dentro la zona vale il Tipo scelto.
public class SuperficieSonora : MonoBehaviour
{
    public enum Tipo { Erba, Terra, Pietra, Legno }

    public Tipo tipo = Tipo.Pietra;

    // Indovina il tipo dal nome del materiale. "trovato" è falso se il materiale non è tra quelli conosciuti.
    public static Tipo DaMateriale(Material materiale, out bool trovato)
    {
        trovato = true;
        string nome = materiale != null ? materiale.name : "";
        if (nome.Contains("Terreno") || nome.Contains("Foglie")) return Tipo.Erba;
        if (nome.Contains("Sentiero")) return Tipo.Terra;
        if (nome.Contains("Legno")) return Tipo.Legno;
        if (nome.Contains("Pietra") || nome.Contains("Lapide") || nome.Contains("Buio")) return Tipo.Pietra;
        trovato = false;
        return Tipo.Pietra;
    }

    public static Suono SuonoPasso(Tipo tipo)
    {
        switch (tipo)
        {
            case Tipo.Erba: return Suono.PassoErba;
            case Tipo.Terra: return Suono.PassoTerra;
            case Tipo.Legno: return Suono.PassoLegno;
            default: return Suono.PassoPietra;
        }
    }

    // Riquadro azzurro nella vista Scene quando è usato come zona.
    void OnDrawGizmos()
    {
        Collider zona = GetComponent<Collider>();
        if (zona == null || !zona.isTrigger) return;
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireCube(zona.bounds.center, zona.bounds.size);
    }
}
