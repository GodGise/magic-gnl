# Logo e apertura di GNL Studio

Scritta il 12 ottobre 2026. All'avvio del gioco, prima della schermata del titolo, si vede il sigillo dello studio che si compone, poi "GNL STUDIO" e sotto, piccolo, "presenta". Dura circa 9 secondi (si salta con un tasto dopo il primo secondo). Si vede una volta sola a ogni avvio del gioco; tornando al menu da una partita non riparte.

## Il sigillo

Un anello di rune diviso in quattro archi, un rombo (lo stesso rombo dell'interfaccia del gioco), il cerchio del lago con i raggi del sole e una spada che lo attraversa. Colori: bronzo caldo su nero, con un lampo di brace.
- I pezzi sono 6 immagini bianche trasparenti in `Assets/Resources/Logo/` (`anello-1` ... `anello-4`, `rombo`, `spada`); il gioco le colora.
- Si rifanno con `Docs/logo-studio/genera_logo.py` (Python con Pillow): `python3 genera_logo.py cartella_di_uscita`. Per cambiare forma si modifica lo script e si rigenerano; se si cambia il disegno vero (per esempio da Nazar in Blender o in un programma di grafica) basta sostituire i 6 PNG tenendo la stessa tela 1024x1024 con il centro al centro.

## La sequenza (secondi dall'inizio)

| Quando | Cosa succede |
| --- | --- |
| 0,0 - 0,7 | Nero e silenzio (partenza del suono) |
| 0,7 - 3,0 | I quattro archi dell'anello arrivano da fuori, ruotando, uno dopo l'altro (ogni 0,18 s) |
| 2,2 - 3,5 | Il rombo si apre dal centro e gira in posizione |
| 3,1 - 4,0 | La spada cala dall'alto |
| **4,0** | **Lampo**: la spada tocca, onda che si allarga dall'anello, scintille |
| 4,4 - 6,3 | "GNL STUDIO": le lettere compaiono una a una e si stringono; filo di luce con rombo sotto |
| 6,0 - 7,0 | "presenta" (tradotto nelle 8 lingue) |
| 8,0 - 9,0 | Dissolvenza al nero |
| 9,35 | Parte il menu (musica e titolo) |

## Il suono

Se in `Assets/Resources/Audio/Effetti/` c'è un file chiamato **`logo-studio`** (`.ogg` o `.wav`), parte da solo a tempo zero, con il volume degli effetti scelto nelle opzioni. Non serve toccare il codice. Durata consigliata: **9 secondi**. Idea per far combaciare il suono con le immagini:
1. 0,0 - 0,7: quasi silenzio, un respiro basso o un drone che sale.
2. 0,7 - 3,0: un fruscio di metallo/vento che sale mentre i pezzi si avvicinano (un colpo leggero a ogni arco: 0,7 / 0,9 / 1,1 / 1,3 s circa).
3. 2,2 - 3,5: una vibrazione grave che si apre (il rombo).
4. 3,1 - 4,0: un sibilo di lama che cala.
5. **4,0: l'impatto** (colpo di campana/incudine con una coda lunga). È il momento più forte.
6. 4,4 - 7,0: la coda dell'impatto con un accordo scuro sostenuto; un lieve brillio quando compare "presenta".
7. 8,0 - 9,0: dissolvenza della coda.
Se nell'editor si salta con F2, il suono sfuma da solo in mezzo secondo.

## Cosa cambia nel codice

`Assets/Scripts/Interfaccia/LogoStudio.cs` si crea da solo all'avvio, solo se la scena è `Menu`: ferma `MenuPrincipale` (non parte né la musica né la sequenza del titolo) fino a logo finito. I tempi sono costanti all'inizio del file. In "Opzioni" non c'è modo di spegnerlo; per le prove nell'editor basta partire da un'altra scena o premere F2.
