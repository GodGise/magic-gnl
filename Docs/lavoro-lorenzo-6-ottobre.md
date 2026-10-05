# Lavoro di Lorenzo: blockout di Villaggio Lago Nero

Riepilogo per Giuseppe e Nazar. Fatto il 5 ottobre sera (in anticipo sul compito del 6), sul ramo `lorenzo/villaggio-blockout`.

## Cosa c'è

- Scena nuova **`Assets/Scenes/VillaggioLagoNero.unity`**, versione intatta del villaggio, costruita con forme grigie (blockout) nei punti esatti della mappa di Giuseppe. Le posizioni sono state prese direttamente dallo script `Docs/mappe/genera-mappa-villaggio.py`, non ricalcate a mano.
- Conversione usata: X di Unity = x della mappa, Z = 170 − y, 1 unità = 1 metro. Nord = +Z, il lago è a est (+X).
- Nella Hierarchy, sotto **Villaggio**: `Terreno`, `Lago` (acqua 30 cm più bassa, riva, molo con 3 barche), `Strade` (principali 3,4 m, vicoli 2,2 m), `Piazza` (lastricato di 32 m, pozzo, 4 torce), `Edifici speciali` (Mercante, Taverna, Fabbro, Erborista, Tempio con abside, Casa dell'eroe), `Case` (71), `Cimitero` (recinto con cancello verso il tempio, 12 lapidi), `Orti`, `Bosco` (882 alberi: cilindro e sfera), `Confini` (muri invisibili sul bordo), `MappaGuida`.
- **`MappaGuida`** è l'immagine della mappa stesa sul terreno, spenta. Si accende con la casella accanto al nome nell'Inspector, per confrontare la scena con la mappa.
- **Prefab `casa-blocco`** (`Assets/Segnaposto/Villaggio/`): tutte le case e gli edifici speciali sono sue copie, con un cubetto scuro "Porta" sul lato della porta. Le porte seguono le regole di Giuseppe: verso il centro della piazzetta nell'anello, verso la strada lungo le strade. Quando Nazar farà il modello, basterà cambiare il prefab.
- Materiali in `Assets/Segnaposto/Villaggio/`. I nomi contengono Terreno, Sentiero, Pietra e Legno, così i passi cambiano suono su erba, strade, piazzetta e molo.
- **Giocatore** davanti alla porta della casa dell'eroe. Si parte alle 16 per vedere bene gli spazi; con T si accelera fino alla notte.
- **Manichino di prova** in piazzetta (non attacca), per provare il combattimento in mezzo alle case.
- **Segnalini (proposte da confermare con Giuseppe)**: cubo rosso dove arrivano gli orchi (strada ovest, dal bosco) e cubo viola in piazzetta per lo scontro con l'orco enorme. Non hanno collider.

## Lo strumento che l'ha creata

Menu **magic-gnl → Crea Villaggio Lago Nero (blockout)** (`Assets/Editor/CreaVillaggioLagoNero.cs`, con i dati in `DatiVillaggioLagoNero.cs`). Serviva una volta sola: d'ora in poi la scena si modifica a mano. Se la scena esiste già, il menu chiede conferma: ricrearla cancella le modifiche fatte a mano, quindi **non va usato** salvo per ripartire da zero.

Se Giuseppe cambia la mappa, i dati si rigenerano dallo script Python (chiedere al Claude di Lorenzo).

## Distanze a piedi (calcolate sulla mappa)

Corsa 5 m/s, sprint 8 m/s.

| Dalla casa dell'eroe a | Distanza | Corsa | Sprint |
| --- | --- | --- | --- |
| Bordo della piazzetta | 38 m | 8 s | 5 s |
| Pozzo (centro della piazzetta) | 54 m | 11 s | 7 s |
| Inizio del molo | 26 m | 5 s | 3 s |
| Primi alberi del bosco | 10 m | 2 s | 1 s |
| Tempio | 121 m | 24 s | 15 s |
| Ingresso degli orchi (bosco a ovest) | 137 m | 28 s | 17 s |

Dalla piazzetta all'ingresso degli orchi: 95 m, 19 s di corsa.

## Da provare in Play (impressioni di Lorenzo)

- Le strade sono larghe abbastanza per combattere, con sprint e camera? *(da scrivere)*
- La piazzetta è troppo grande o troppo piccola? *(da scrivere)*
- I tempi a piedi sembrano giusti? *(da scrivere)*

## Da sapere

- L'acqua del lago per ora si può attraversare a piedi (è solo più bassa): andrà bloccata o resa profonda.
- Strade e alberi sono tanti oggetti separati: per il blockout va bene, con i modelli veri si potranno unire per le prestazioni.
- Il molo inizia 3 metri prima della riva, come nella mappa.
- `ZonaProva` non è stata toccata.
