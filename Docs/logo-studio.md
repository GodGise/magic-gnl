# Logo e apertura di GNL Studio

Scritta il 12 ottobre 2026. All'avvio del gioco, prima della schermata del titolo, si vede il sigillo dello studio che si compone, poi "GNL STUDIO" e sotto, piccolo, "presenta". Dura circa 10,5 secondi dopo il nero iniziale (non si salta, solo nell’editor con F2). Si vede una volta sola a ogni avvio del gioco; tornando al menu da una partita non riparte.

## Il sigillo

Disegnato **grande al centro dello schermo**. Un filo esterno con tacche (gira piano), un anello di quattro archi con **28 rune** che si accendono una dopo l'altra, un rombo (lo stesso dell'interfaccia del gioco), il cerchio del lago con il sole e una **runa centrale, nel lago, che si accende quando la scintilla la raggiunge** (la runa dell'**energia**, una bindrune: asta con rombo e due bracci a V; scelta da Giuseppe). Colori: bronzo caldo su nero, con un lampo di brace (invariati).
- I pezzi sono immagini bianche trasparenti in `Assets/Resources/Logo/`: `anello-1..4`, `anello-esterno`, `rombo`, `runa-centrale`, `onda`, `raggi` (1024x1024) e `runa-1..7` (256x256); il gioco le colora e fa gli aloni.
- Si rifanno con `Docs/logo-studio/genera_logo.py` (Python con Pillow e numpy): `python3 genera_logo.py cartella_di_uscita`. Se si cambia il disegno vero (per esempio da Nazar) basta sostituire i PNG tenendo la stessa tela con il centro al centro. Le 28 rune stanno sull'anello a 425 px dal centro (angoli come in `AngoloRuna` nel codice).

## La sequenza

Prima **schermo nero e silenzio per 3 secondi**; poi parte l'animazione (e il suono). I secondi qui sotto sono contati **dall'inizio dell'animazione**, cioè dopo il nero.

| Quando | Cosa succede |
| --- | --- |
| (prima) 3,0 s | Nero e silenzio |
| 0,0 - 1,4 | Un punto di luce al centro cresce; il filo esterno si accende e comincia a girare piano |
| 0,3 - 2,1 | I quattro archi arrivano da fuori ruotando, uno dopo l'altro (ogni 0,16 s); scintille quando si posano |
| 1,3 - 2,6 | Le 28 rune si accendono una dopo l'altra in giro (una ogni 0,045 s), ognuna con un lampo e scintille |
| 1,7 - 2,9 | Il rombo si apre dal centro e gira in posizione |
| 1,7 - 3,3 | La runa centrale compare appena visibile, spenta |
| 2,3 - 3,3 | Una scintilla con la scia cala dall'alto, sempre più veloce, e cade nel lago |
| **3,3** | **Impatto**: la scintilla tocca il lago e la **runa centrale si accende**. Lampo sullo schermo, tremito, onde nel lago, onda d'urto, raggi di luce, esplosione di scintille (circa 300) e gocce che schizzano; le rune si accendono tutte insieme |
| 3,3 - 5,0 | Pausa: il sigillo resta acceso, le rune pulsano a onda, le braci salgono. Vignetta scura ai bordi dello schermo, braci grandi sfocate in lontananza |
| 5,0 - 6,1 | Il sigillo **sale un po' e rimpicciolisce**; sigillo, nome e "presenta" formano un gruppo **centrato nello schermo** |
| 5,5 - 7,4 | "GNL STUDIO" compare **al centro dello schermo**: lettere una a una che si stringono, filo di luce con rombo |
| 7,0 - 8,0 | "presenta" (tradotto nelle 8 lingue) |
| 9,0 - 10,0 | Dissolvenza al nero |
| 10,35 | Parte il menu (musica e titolo) |

Totale dall'avvio del gioco: circa 13,4 secondi (3 di nero + 10,35 di animazione).

## Il suono (versione attuale)

Il file `logo-studio` è quello **montato da Giuseppe su CapCut** sul video della registrazione del logo (13 ottobre): nel file l'istante 0 è l'inizio della registrazione, il suono d'apertura parte a circa 3,2 s, **le rune cominciano ad accendersi a 4,43 s** e il colpo (impatto metallico) è a circa 6,0 s. Durata 13,2 s.
L'audio è l'**orologio dell'animazione**: il file parte subito e, finché suona, l'animazione segue la sua posizione di lettura (così i blocchi del PC non possono sfasare). Il colpo secco nel file è a 6,0 s (`ColpoNelFile`) e la runa centrale si accende a 3,0 + 3,3 = 6,3 s: `Differenza` = 0,3 s. Se il file viene rimontato, cambiare `ColpoNelFile`.
**Se cambi il file:** tienilo montato nello stesso modo (rune a 4,43 s, tutto il resto riferito a quell'istante) e non serve toccare il codice. Per ritocchi fini nell'editor: durante il logo **F3/F4** (passi da 0,1 s, **Maiusc** 0,01 s, **Ctrl** 0,5 s), **F5** azzera; il numero in basso a sinistra è il ritocco **in più** rispetto alla costante. Per renderlo definitivo si somma alla costante.
Le note qui sotto ("dopo il nero", secondi dall'animazione) valgono come idea del sonoro; per il file valgono i secondi sopra, dall'avvio del gioco.


Se in `Assets/Resources/Audio/Effetti/` c'è un file chiamato **`logo-studio`** (`.ogg` o `.wav`), parte da solo **quando inizia l'animazione** (dopo il nero), con il volume degli effetti scelto nelle opzioni. Non serve toccare il codice. Se il file ha un vuoto (silenzio) all'inizio, il gioco lo **salta da solo**: parte dal primo suono udibile (soglia molto bassa), quindi il colpo va messo a 3,3 secondi **dal primo suono**, non dall'inizio del file. Durata consigliata: **10,5 secondi**, con l'**impatto al secondo 3,3** del file (come prima). Idea per far combaciare il suono con le immagini:
1. 0,0 - 1,4: un respiro grave e un brillio sottile che salgono (il punto di luce e il filo esterno).
2. 0,3 - 2,1: un colpo leggero di metallo a ogni arco (circa 0,3 / 0,46 / 0,62 / 0,78 s).
3. 1,3 - 2,6: tintinnii cristallini veloci, uno ogni 0,045 s, che salgono di tono (le rune).
4. 1,7 - 2,9: una vibrazione grave che si apre (il rombo).
5. 2,3 - 3,3: un sibilo sottile e luminoso che cala sempre più acuto e veloce (la scintilla).
6. **3,3: l'impatto** (colpo di campana/incudine con spruzzo d'acqua e una coda lunga). È il momento più forte.
7. 3,3 - 7,4: la coda dell'impatto con un accordo scuro sostenuto; un lieve brillio quando compare il nome (5,5) e "presenta" (7,0).
8. 9 - 10,5: dissolvenza della coda.
Se il colpo non combacia: in `LogoStudio.cs` la costante `CorrezioneSuono` sposta l'inizio del suono (in secondi: positivo = più tardi, negativo = prima).
Se nell'editor si salta con F2, il suono sfuma da solo in mezzo secondo.

## Cosa cambia nel codice

`Assets/Scripts/Interfaccia/LogoStudio.cs` si crea da solo all'avvio, solo se la scena è `Menu`: ferma `MenuPrincipale` (non parte né la musica né la sequenza del titolo) fino a logo finito. I tempi sono costanti all'inizio del file. In "Opzioni" non c'è modo di spegnerlo; per le prove nell'editor basta partire da un'altra scena o premere F2.
