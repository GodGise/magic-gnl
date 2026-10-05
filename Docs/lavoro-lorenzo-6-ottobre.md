# Lavoro di Lorenzo: blockout di Villaggio Lago Nero

Riepilogo per Giuseppe e Nazar. Fatto il 5 ottobre sera (in anticipo sul compito del 6), sul ramo `lorenzo/villaggio-blockout`.

## Aggiornamento: villaggio allargato

Dopo la prima prova in Play il villaggio sembrava troppo piccolo e concentrato. Lorenzo ha chiesto di allargarlo:
- tutte le distanze sono **più larghe del 50%** (terreno 315 x 255 m invece di 210 x 170);
- la **piazzetta** passa da 32 a **48 m**; le case attorno restano attaccate e chiudono il giro, ma sono più larghe;
- **strade** più larghe: principali 4,4 m, vicoli 2,9 m;
- molte **case** sono più grandi (fino al 35% in più) e più alte, **alcune sono rimaste come sulla mappa**; tutte sono più distanziate;
- edifici speciali più grandi del 25%; il **bosco** è stato infittito (1672 alberi), perché allargandolo era diventato rado.
La pianta resta quella della mappa di Giuseppe: cambiano solo le misure. MappaGuida è allargata della stessa quantità.

## Cosa c'è

- Scena nuova **`Assets/Scenes/VillaggioLagoNero.unity`**, versione intatta del villaggio, costruita con forme grigie (blockout) nei punti esatti della mappa di Giuseppe. Le posizioni sono state prese direttamente dallo script `Docs/mappe/genera-mappa-villaggio.py`, non ricalcate a mano.
- Conversione usata: X di Unity = x della mappa × 1,5, Z = (170 − y) × 1,5, 1 unità = 1 metro. Nord = +Z, il lago è a est (+X).
- Nella Hierarchy, sotto **Villaggio**: `Terreno`, `Lago` (acqua 30 cm più bassa, riva, molo con 3 barche), `Strade` (principali 4,4 m, vicoli 2,9 m), `Piazza` (lastricato di 48 m, pozzo, 4 torce), `Edifici speciali` (Mercante, Taverna, Fabbro, Erborista, Tempio con abside, Casa dell'eroe), `Case` (71), `Cimitero` (recinto con cancello verso il tempio, 12 lapidi), `Orti`, `Bosco` (1672 alberi: cilindro e sfera), `Confini` (muri invisibili sul bordo), `MappaGuida`.
- **`MappaGuida`** è l'immagine della mappa stesa sul terreno, spenta. Si accende con la casella accanto al nome nell'Inspector, per confrontare la scena con la mappa.
- **Prefab `casa-blocco`** (`Assets/Segnaposto/Villaggio/`): tutte le case e gli edifici speciali sono sue copie, con un cubetto scuro "Porta" sul lato della porta. Le porte seguono le regole di Giuseppe: verso il centro della piazzetta nell'anello, verso la strada lungo le strade. Quando Nazar farà il modello, basterà cambiare il prefab.
- Materiali in `Assets/Segnaposto/Villaggio/`. I nomi contengono Terreno, Sentiero, Pietra e Legno, così i passi cambiano suono su erba, strade, piazzetta e molo.
- **Giocatore** davanti alla porta della casa dell'eroe. Si parte alle 16 per vedere bene gli spazi; con T si accelera fino alla notte.
- **Orco della piazza** (nemico di prova, un po' più grosso e verdastro), con il nuovo script `Assets/Scripts/Gameplay/InseguimentoNemico.cs`: sta fermo e si guarda intorno; si accorge del giocatore solo se entra nel suo campo visivo (110 gradi davanti a lui, fino a 18 m, senza muri in mezzo) oppure se lo colpisce alle spalle. Allora mostra un "!", insegue e attacca; se lo perde di vista per 4 secondi torna al suo posto. Tutti i numeri sono nell'Inspector; selezionandolo si vede il cono giallo della vista. Provato in Play: alle spalle non si accorge, davanti insegue e colpisce, lontano torna indietro. `Bersaglio` ora avvisa quando viene colpito (evento `Colpito`).
- **Segnalini (proposte da confermare con Giuseppe)**: cubo rosso dove arrivano gli orchi (strada ovest, dal bosco) e cubo viola in piazzetta per lo scontro con l'orco enorme. Non hanno collider. Lorenzo li ha provati e gli sembrano nel posto giusto; resta la conferma di Giuseppe.

## Lo strumento che l'ha creata

Menu **magic-gnl → Crea Villaggio Lago Nero (blockout)** (`Assets/Editor/CreaVillaggioLagoNero.cs`, con i dati in `DatiVillaggioLagoNero.cs`). Serviva una volta sola: d'ora in poi la scena si modifica a mano. Se la scena esiste già, il menu chiede conferma: ricrearla cancella le modifiche fatte a mano, quindi **non va usato** salvo per ripartire da zero.

Se Giuseppe cambia la mappa, i dati si rigenerano dallo script Python (chiedere al Claude di Lorenzo).

## Distanze a piedi (calcolate sulla mappa)

Corsa 5 m/s, sprint 8 m/s.

| Dalla casa dell'eroe a | Distanza | Corsa | Sprint |
| --- | --- | --- | --- |
| Bordo della piazzetta | 59 m | 12 s | 7 s |
| Pozzo (centro della piazzetta) | 83 m | 17 s | 10 s |
| Inizio del molo | 44 m | 9 s | 5 s |
| Primi alberi del bosco | 14 m | 3 s | 2 s |
| Tempio | 184 m | 37 s | 23 s |
| Ingresso degli orchi (bosco a ovest) | 208 m | 42 s | 26 s |

Dalla piazzetta all'ingresso degli orchi: 145 m, 29 s di corsa.

## Da provare in Play (impressioni di Lorenzo)

- Le strade sono larghe abbastanza per combattere, con sprint e camera? *(da scrivere)*
- La piazzetta è troppo grande o troppo piccola? Nella prima versione (32 m) era troppo piccola e tutto sembrava concentrato; dopo l'allargamento del 50% Lorenzo dice che la mappa va bene e gli spazi sono giusti.
- I tempi a piedi sembrano giusti? Sì, con la versione allargata.

## Da sapere

- L'acqua del lago per ora si può attraversare a piedi (è solo più bassa): andrà bloccata o resa profonda.
- Strade e alberi sono tanti oggetti separati: per il blockout va bene, con i modelli veri si potranno unire per le prestazioni.
- Il molo inizia 3 metri prima della riva, come nella mappa.
- `ZonaProva` non è stata toccata.
