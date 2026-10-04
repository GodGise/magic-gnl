# Lavoro di Lorenzo, 4 ottobre 2026

Riepilogo per Giuseppe e Nazar di quello che Lorenzo ha fatto con il suo Claude. Tutto è sui rami `lorenzo/...`: niente è ancora in `main`.

## Pull request e ordine di unione

| PR | Ramo | Contenuto |
| --- | --- | --- |
| #3 | `lorenzo/camera-sprint` | Camera che non attraversa muri e pavimento, sprint con Shift, aspetto provvisorio da figura umana |
| #4 | `lorenzo/checkpoint-trappola` | Tutto quello della #3, più checkpoint, trappole, leva e porta, muro crepato, animazioni, suoni e le modifiche alla zona di prova |

La #4 parte dalla #3 e la contiene. Si può unire la #3 e poi la #4, oppure direttamente la #4 (poi la #3 si chiude). Il ramo non va in conflitto con `main` (controllato dopo l'aggiunta di `Docs/storia.md`).

## Cosa c'è

**Camera e movimento**
- Camera: non entra più in muri, strutture, pavimento e nemici. Vicino a un muro si avvicina in modo graduale, si alza un poco e guarda davanti al personaggio; se resta incastrata, il personaggio si nasconde invece di mostrarne l'interno.
- Sprint con Shift (o pressione della levetta sinistra): velocità 8, consuma 15 di resistenza al secondo, riparte con almeno 10.

**Morte e checkpoint**
- Con la vita a zero o cadendo nel vuoto compare "SEI MORTO" e dopo 2 secondi si rinasce all'ultimo checkpoint acceso (o alla partenza).
- Checkpoint (`Livelli/Checkpoint.cs`): si accende con E, cambia colore e fa luce; accenderne un altro spegne il precedente.

**Elementi della zona** (`Assets/Scripts/Livelli/`)
- `TrappolaSpuntoni`: invisibile, scatta dopo che il giocatore ci passa sopra (mezzo secondo di avviso con le punte appena fuori), 35 di danno; non si para ma si schiva. Nella vista Scene è un riquadro rosso.
- `Leva` (tasto E) e `Porta`: la porta è un cubo che scende sotto terra. Se la leva non ha porte collegate, apre la più vicina.
- `MuroFragile`: muro con crepe, trema a ogni colpo e crolla in blocchi dopo 4 colpi. Per questo l'attacco del giocatore ora colpisce anche questi muri.

**Figura provvisoria e animazioni** (segnaposto finché non arrivano i modelli di Nazar)
- `AspettoUmanoide`: durante il Play capsula e cilindri diventano figure a blocchi con spada (giocatore) o mazza (nemici). Agisce solo su capsule e cilindri di Unity, quindi si spegne da solo con i modelli veri.
- `AnimazioneUmanoide`: camminata, combo di tre colpi diversi (orizzontale, rovescio, dall'alto), schivata come scatto inclinato, parata, colpito, caduta. I nemici caricano durante il preavviso rosso e colpiscono alternando i tre movimenti.

**Suoni provvisori** (`Assets/Scripts/Ambiente/`)
- `Suoni`: tutti i suoni sono calcolati dal codice, senza file audio, quindi originali e senza problemi di licenza. Passi, fendenti, schivata, impatto, parata, guardia rotta, colpito, morte, rinascita, leva, porta, checkpoint, trappola, muro.
- `PassiSonori`: passi diversi su erba, terra del sentiero, pietra e legno, riconosciuti dal materiale del pavimento. `SuperficieSonora` serve a forzare il tipo su un pavimento o su una zona.

## Modifiche alla scena `ZonaProva`
- Due trappole: appena dentro la porta della cripta e nella casa in rovina a sud-est della piazza.
- Checkpoint "Checkpoint altare" subito dopo il pozzo.
- "Porta cripta" nel vano d'ingresso e "Leva cripta" accanto, a sinistra.
- Il muro sinistro della cripta (`Lato sinistro`) è diviso in due pezzi più un architrave; nel varco c'è il "Muro crepato": un passaggio segreto per entrare senza leva.

## Script esistenti toccati (area del Claude di Giuseppe)
`GiocatoreControllo`, `CameraTerzaPersona`, `Resistenza`, `Bersaglio`. Le aggiunte sono valori pubblici in sola lettura per l'animazione, il danno dall'ambiente (`RiceviDannoAmbiente`), la rinascita al checkpoint, lo sprint, la combo e gli agganci dei suoni. Il combattimento di base non è cambiato.

## Come provare
1. GitHub Desktop: Fetch origin, poi Current branch → `lorenzo/checkpoint-trappola`.
2. In Unity aprire `ZonaProva` e premere Play.
3. Percorso consigliato: accendere l'altare dopo il pozzo con E, combattere il Guardiano (combo da tre, schivata, parata), andare alla cripta, aprire la porta con la leva (E) ed entrare nella trappola; poi girare sul lato sinistro della cripta e rompere il muro crepato.

## Da sapere
- Tutti i numeri (camera, sprint, trappole, muro, animazioni) si regolano dall'Inspector.
- Lorenzo ha provato in Play camera, sprint, checkpoint, leva, porta e trappole. Animazioni d'attacco e suoni sono stati scritti per ultimi: vanno ascoltati e guardati con attenzione.
- Con la nuova storia, il "frammento d'anima" in fondo alla cripta previsto dai compiti non vale più: l'obiettivo della zona è da decidere.
- Il vecchio ramo di prova `lorenzo/prova` (pull request #2, chiusa) si può cancellare da GitHub: dalla sessione di Claude la cancellazione dei rami è bloccata.
