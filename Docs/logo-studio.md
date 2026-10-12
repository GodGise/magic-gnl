# Logo e apertura di GNL Studio

Scritta il 12 ottobre 2026. All'avvio del gioco, prima della schermata del titolo, si vede il sigillo dello studio che si compone, poi "GNL STUDIO" e sotto, piccolo, "presenta". Dura circa 9 secondi (si salta con un tasto dopo il primo secondo). Si vede una volta sola a ogni avvio del gioco; tornando al menu da una partita non riparte.

## Il sigillo

Un anello di rune diviso in quattro archi, un rombo (lo stesso rombo dell'interfaccia del gioco), il cerchio del lago con i raggi del sole e una spada che lo attraversa. Colori: bronzo caldo su nero, con un lampo di brace.
- I pezzi sono 6 immagini bianche trasparenti in `Assets/Resources/Logo/` (`anello-1` ... `anello-4`, `rombo`, `spada`); il gioco le colora.
- Si rifanno con `Docs/logo-studio/genera_logo.py` (Python con Pillow): `python3 genera_logo.py cartella_di_uscita`. Per cambiare forma si modifica lo script e si rigenerano; se si cambia il disegno vero (per esempio da Nazar in Blender o in un programma di grafica) basta sostituire i 6 PNG tenendo la stessa tela 1024x1024 con il centro al centro.

## La sequenza

Prima **schermo nero e silenzio per 3 secondi**; poi parte l'animazione (e il suono). I secondi qui sotto sono contati **dall'inizio dell'animazione**, cioè dopo il nero.

| Quando | Cosa succede |
| --- | --- |
| (prima) 3,0 s | Nero e silenzio |
| 0,0 - 2,0 | I quattro archi dell'anello arrivano da fuori, ruotando, uno dopo l'altro (ogni 0,18 s) |
| 1,5 - 2,8 | Il rombo si apre dal centro e gira in posizione |
| 2,4 - 3,3 | La spada cala dall'alto |
| **3,3** | **Lampo**: la spada tocca, onda che si allarga dall'anello, scintille |
| 3,7 - 5,6 | "GNL STUDIO": le lettere compaiono una a una e si stringono; filo di luce con rombo sotto |
| 5,3 - 6,3 | "presenta" (tradotto nelle 8 lingue) |
| 7,3 - 8,3 | Dissolvenza al nero |
| 8,65 | Parte il menu (musica e titolo) |

Totale dall'avvio del gioco: circa 11,7 secondi (3 di nero + 8,65 di animazione).

## Il suono

Se in `Assets/Resources/Audio/Effetti/` c'è un file chiamato **`logo-studio`** (`.ogg` o `.wav`), parte da solo **quando inizia l'animazione** (dopo il nero), con il volume degli effetti scelto nelle opzioni. Non serve toccare il codice. Durata consigliata: **8,5 secondi**, con l'**impatto al secondo 3,3** del file. Idea per far combaciare il suono con le immagini:
1. 0,0 - 2,0: un fruscio di metallo/vento che sale mentre i pezzi si avvicinano (un colpo leggero a ogni arco: 0,0 / 0,2 / 0,4 / 0,6 s circa).
2. 1,5 - 2,8: una vibrazione grave che si apre (il rombo).
3. 2,4 - 3,3: un sibilo di lama che cala.
4. **3,3: l'impatto** (colpo di campana/incudine con una coda lunga). È il momento più forte.
5. 3,7 - 6,3: la coda dell'impatto con un accordo scuro sostenuto; un lieve brillio quando compare "presenta".
6. 7,3 - 8,5: dissolvenza della coda.
Se nell'editor si salta con F2, il suono sfuma da solo in mezzo secondo.

## Cosa cambia nel codice

`Assets/Scripts/Interfaccia/LogoStudio.cs` si crea da solo all'avvio, solo se la scena è `Menu`: ferma `MenuPrincipale` (non parte né la musica né la sequenza del titolo) fino a logo finito. I tempi sono costanti all'inizio del file. In "Opzioni" non c'è modo di spegnerlo; per le prove nell'editor basta partire da un'altra scena o premere F2.
