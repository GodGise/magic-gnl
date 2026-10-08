using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Una voce di un menu: il testo, cosa succede quando la si conferma e, nelle opzioni, il valore a destra
// che si cambia con sinistra e destra.
public class VoceMenu
{
    public Func<string> testo;
    public Func<string> valore;      // solo nelle opzioni
    public bool attiva = true;
    public Action conferma;
    public Action<int> regola;       // -1 sinistra, +1 destra
}

// I comandi letti in un fotogramma.
public struct ComandiMenu
{
    public int verticale, orizzontale;
    public bool conferma, indietro;
}

// Elenco di voci di un menu, con la selezione e i comandi da mouse, tastiera e pad, e il disegno
// nello stile di GraficaMenu. Lo usano il menu iniziale e il menu di pausa.
// Tastiera: frecce o WASD, Invio o Spazio per confermare, Esc o Backspace per tornare indietro.
// Pad: croce o levetta sinistra, A per confermare, B per tornare indietro.
// Come montarlo: non si monta su nessun oggetto, si crea dal codice (new ElencoMenu()).
public class ElencoMenu
{
    public readonly List<VoceMenu> voci = new List<VoceMenu>();
    public int selezione;

    // Collegamenti facoltativi per i suoni: chi usa l'elenco (per esempio il menu iniziale) li imposta.
    // alMuovere: la selezione cambia o un valore viene regolato. alConfermare: una voce attiva viene confermata.
    // Se restano vuoti (menu di pausa, inventario) l'elenco funziona come prima.
    public Action alMuovere, alConfermare;

    float prossimoScatto;
    Vector2 ultimoMouse = new Vector2(-1f, -1f);
    bool mouseMosso;

    public void Pulisci()
    {
        voci.Clear();
        selezione = 0;
    }

    public VoceMenu Aggiungi(Func<string> testo, Action conferma)
    {
        var voce = new VoceMenu { testo = testo, conferma = conferma };
        voci.Add(voce);
        return voce;
    }

    public VoceMenu Selezionata => selezione >= 0 && selezione < voci.Count ? voci[selezione] : null;

    // ---------- comandi ----------

    public ComandiMenu LeggiComandi()
    {
        var c = new ComandiMenu();
        var tastiera = Keyboard.current;
        var pad = Gamepad.current;

        if (tastiera != null)
        {
            if (tastiera.upArrowKey.wasPressedThisFrame || tastiera.wKey.wasPressedThisFrame) c.verticale = -1;
            if (tastiera.downArrowKey.wasPressedThisFrame || tastiera.sKey.wasPressedThisFrame) c.verticale = 1;
            if (tastiera.leftArrowKey.wasPressedThisFrame || tastiera.aKey.wasPressedThisFrame) c.orizzontale = -1;
            if (tastiera.rightArrowKey.wasPressedThisFrame || tastiera.dKey.wasPressedThisFrame) c.orizzontale = 1;
            c.conferma |= tastiera.enterKey.wasPressedThisFrame || tastiera.numpadEnterKey.wasPressedThisFrame || tastiera.spaceKey.wasPressedThisFrame;
            c.indietro |= tastiera.escapeKey.wasPressedThisFrame || tastiera.backspaceKey.wasPressedThisFrame;
        }
        if (pad != null)
        {
            if (pad.dpad.up.wasPressedThisFrame) c.verticale = -1;
            if (pad.dpad.down.wasPressedThisFrame) c.verticale = 1;
            if (pad.dpad.left.wasPressedThisFrame) c.orizzontale = -1;
            if (pad.dpad.right.wasPressedThisFrame) c.orizzontale = 1;
            c.conferma |= pad.buttonSouth.wasPressedThisFrame;
            c.indietro |= pad.buttonEast.wasPressedThisFrame;

            // levetta sinistra, con ripetizione se resta inclinata (tempo reale: funziona anche in pausa)
            Vector2 leva = pad.leftStick.ReadValue();
            if (leva.magnitude < 0.5f) prossimoScatto = 0f;
            else if (Time.unscaledTime >= prossimoScatto)
            {
                if (Mathf.Abs(leva.y) > Mathf.Abs(leva.x)) c.verticale = leva.y > 0 ? -1 : 1;
                else c.orizzontale = leva.x > 0 ? 1 : -1;
                prossimoScatto = Time.unscaledTime + (prossimoScatto == 0f ? 0.35f : 0.15f);
            }
        }
        return c;
    }

    // Su e giù spostano, sinistra e destra regolano, conferma conferma. "Indietro" lo gestisce chi usa l'elenco.
    public void Applica(ComandiMenu c)
    {
        if (c.verticale != 0) Sposta(c.verticale);
        if (c.orizzontale != 0 && Selezionata?.regola != null) { Selezionata.regola(c.orizzontale); alMuovere?.Invoke(); }
        if (c.conferma) Conferma(selezione);
    }

    public void Sposta(int direzione)
    {
        if (voci.Count == 0) return;
        int prima = selezione;
        for (int i = 0; i < voci.Count; i++)
        {
            selezione = (selezione + direzione + voci.Count) % voci.Count;
            if (voci[selezione].attiva)
            {
                if (selezione != prima) alMuovere?.Invoke();
                return;
            }
        }
    }

    public void Conferma(int indice)
    {
        if (indice < 0 || indice >= voci.Count) return;
        var voce = voci[indice];
        if (voce.attiva && voce.conferma != null) { alConfermare?.Invoke(); voce.conferma(); }
    }

    // ---------- mouse ----------

    // Da chiamare in OnGUI dopo GraficaMenu.FoglioVirtuale(), prima di disegnare le voci.
    public void InizioGUI()
    {
        var e = Event.current;
        mouseMosso = false;
        if (e.type != EventType.Repaint) return;
        mouseMosso = ultimoMouse.x >= 0f && (e.mousePosition - ultimoMouse).sqrMagnitude > 1f;
        ultimoMouse = e.mousePosition;
    }

    // Passando sopra una voce la seleziona, il clic la conferma. Restituisce true se ha cliccato:
    // il menu può essere cambiato, quindi chi chiama deve smettere di disegnare.
    public bool Mouse(Rect area, int indice, bool bloccato = false)
    {
        var e = Event.current;
        if (bloccato || indice < 0 || indice >= voci.Count || !voci[indice].attiva || !area.Contains(e.mousePosition)) return false;
        if (mouseMosso && selezione != indice) { selezione = indice; alMuovere?.Invoke(); }
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            selezione = indice;
            e.Use();
            Conferma(indice);
            return true;
        }
        return false;
    }

    // ---------- disegno ----------

    // Riquadro con le voci una sotto l'altra. Restituisce true se è stato cliccato qualcosa.
    public bool DisegnaElenco(float alto, float larghezza, float alfa, bool bloccato = false)
    {
        const float passo = 72f, margine = 38f;
        var riquadro = new Rect(GraficaMenu.Larghezza * 0.5f - larghezza * 0.5f, alto, larghezza, voci.Count * passo + margine * 2f - 12f);
        GraficaMenu.Cornice(riquadro, alfa, false);
        for (int i = 0; i < voci.Count; i++)
        {
            var area = new Rect(riquadro.x + 24f, alto + margine + i * passo, larghezza - 48f, 60f);
            if (Mouse(area, i, bloccato)) return true;
            VoceCentrata(area, voci[i], i == selezione, alfa);
        }
        return false;
    }

    // Opzioni: nome a sinistra, valore a destra fra due frecce (cliccabili). Le voci senza valore
    // (per esempio "Indietro") sono centrate.
    public bool DisegnaOpzioni(float alto, float alfa, bool bloccato = false)
    {
        const float larghezza = 1000f, passo = 74f, margine = 38f;
        var riquadro = new Rect(GraficaMenu.Larghezza * 0.5f - larghezza * 0.5f, alto, larghezza, voci.Count * passo + margine * 2f - 12f);
        GraficaMenu.Cornice(riquadro, alfa, false);
        var e = Event.current;

        for (int i = 0; i < voci.Count; i++)
        {
            var voce = voci[i];
            var area = new Rect(riquadro.x + 24f, alto + margine + i * passo, larghezza - 48f, 60f);

            if (voce.valore == null)
            {
                if (Mouse(area, i, bloccato)) return true;
                VoceCentrata(area, voce, i == selezione, alfa);
                continue;
            }

            float centroValore = area.xMax - 200f;
            var frecciaSinistra = new Rect(centroValore - 175f, area.y, 50f, area.height);
            var frecciaDestra = new Rect(centroValore + 125f, area.y, 50f, area.height);
            if (!bloccato && e.type == EventType.MouseDown && e.button == 0 && voce.regola != null &&
                (frecciaSinistra.Contains(e.mousePosition) || frecciaDestra.Contains(e.mousePosition)))
            {
                selezione = i;
                voce.regola(frecciaSinistra.Contains(e.mousePosition) ? -1 : 1);
                alMuovere?.Invoke();
                e.Use();
                return true;
            }
            if (Mouse(area, i, bloccato)) return true;

            bool scelta = i == selezione;
            if (scelta)
            {
                GraficaMenu.FasciaLuce(new Rect(area.x - 10f, area.y + 4f, area.width + 20f, area.height - 8f), GraficaMenu.Con(GraficaMenu.Selezione, 0.16f * alfa));
                GraficaMenu.Rombo(new Vector2(area.x + 18f, area.center.y), 9f, GraficaMenu.Con(GraficaMenu.Selezione, alfa));
            }
            Color colore = scelta ? GraficaMenu.Selezione : GraficaMenu.Testo;
            GraficaMenu.Scritta(new Rect(area.x + 44f, area.y, area.width * 0.6f, area.height), voce.testo(), GraficaMenu.VoceSinistra, colore, alfa);
            GraficaMenu.Scritta(new Rect(centroValore - 125f, area.y, 250f, area.height), voce.valore(), GraficaMenu.Voce, colore, alfa);
            Color frecce = scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo;
            bool cinese = Lingua.Indice == 7;   // il carattere cinese non ha ‹ ›
            GraficaMenu.Scritta(frecciaSinistra, cinese ? "<" : "‹", GraficaMenu.Voce, frecce, alfa);
            GraficaMenu.Scritta(frecciaDestra, cinese ? ">" : "›", GraficaMenu.Voce, frecce, alfa);
        }
        return false;
    }

    // Una voce centrata; se è quella scelta, color fiamma con una fascia di luce e due rombi ai lati.
    public void VoceCentrata(Rect area, VoceMenu voce, bool scelta, float alfa)
    {
        scelta &= voce.attiva;
        string testo = voce.testo();
        if (scelta)
        {
            GraficaMenu.FasciaLuce(new Rect(area.x - 20f, area.y + 4f, area.width + 40f, area.height - 8f), GraficaMenu.Con(GraficaMenu.Selezione, 0.2f * alfa));
            float w = GraficaMenu.Voce.CalcSize(new GUIContent(testo)).x;
            GraficaMenu.Rombo(new Vector2(area.center.x - w * 0.5f - 30f, area.center.y), 10f, GraficaMenu.Con(GraficaMenu.Selezione, alfa));
            GraficaMenu.Rombo(new Vector2(area.center.x + w * 0.5f + 30f, area.center.y), 10f, GraficaMenu.Con(GraficaMenu.Selezione, alfa));
        }
        Color colore = !voce.attiva ? GraficaMenu.Spento : scelta ? GraficaMenu.Selezione : GraficaMenu.Testo;
        GraficaMenu.Scritta(area, testo, GraficaMenu.Voce, colore, alfa);
    }
}
