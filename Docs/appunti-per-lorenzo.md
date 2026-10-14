# Appunti di Giuseppe per Lorenzo (lista aperta)

Giuseppe aggiunge qui le modifiche man mano che prova il gioco. Vanno lette insieme a `decisioni-14-ottobre.md` (compito: riscrivere i dialoghi del capitolo 1, bozza 3). Quando una voce è fatta, si sposta in "Fatto".

## Da fare

### 14 ottobre: cripta (quest 5, "La leggenda del lago")
La cripta (`Assets/Scenes/Cripta.unity`) deve avere qualcosa in più di cancello, leva e baule:
- **Un tunnel con tracce strane** (segni sul pavimento o sulle pareti) che fanno presagire che **qualcuno o qualcosa sia già stato qui**. Cosa lasci le tracce non si dice: resta un mistero, coerente con Edric e Ilse che anticipano il segreto del lago senza spiegarlo.
- **Un corridoio che non porta a nulla** (vicolo cieco).
- **Una trappola a spuntoni**, visto che le trappole sono già state fatte. Posizione proposta: nel corridoio cieco, così chi esplora per curiosità la paga.
- Per i dialoghi: Edric deve poter commentare le tracce al ritorno (non le conosce, o fa finta di non conoscerle). Da collegare al filo logico della giornata.
- Per il livello: in co-op la trappola deve funzionare per ogni giocatore; da provare in 1, 2 e 3.

### 14 ottobre: orchi della razzia, udito e furtività (`RazziatoreOrco`)
Oggi i razziatori si accorgono del giocatore solo quando entra nel loro campo visivo. Aggiungere l'**udito**:
- **Sentono i passi** se il giocatore **corre (sprint)** o fa la **schivata**: in quel caso si accorgono di lui anche senza vederlo, entro un raggio da decidere (valore modificabile in Unity).
- Se il giocatore si **avvicina camminando, senza sprintare**, restano **ignari**: e se non entra nel loro campo visivo può fare l'**esecuzione furtiva** (alle spalle, già prevista).
- In co-op conta ogni giocatore: basta uno che corre perché gli orchi sentano.
- Valori (raggio dell'udito, raggio più piccolo con la camminata, tempo prima che si calmino) tutti regolabili dall'Inspector. Da provare in 1, 2 e 3.

### 14 ottobre: durante la razzia il villaggio è spoglio, servono gli abitanti
Durante il raid in giro non c'è nessuno. Aggiungere degli **abitanti (PNG) che corrono in giro e fuggono, inseguiti dagli orchi**:
- Gli orchi li **inseguono** e **a volte li prendono e li uccidono** (animazione e suono provvisori vanno bene).
- Il giocatore **può salvarli uccidendo l'orco** che li insegue, ma **non influiscono sul conteggio** della razzia (le percentuali di orchi abbattuti, il 65% e il 70% delle ondate, restano calcolate solo sugli orchi).
- Gli abitanti uccisi non devono bloccare nulla: nessuna quest dipende da loro.
- Per le prestazioni: pochi alla volta (per esempio 4-6 in scena, con ricambio), forme provvisorie come gli altri PNG; i modelli veri li fa Nazar (da aggiungere in `Docs/lista-modelli-nazar.txt`).
- In co-op gli abitanti sono comandati dall'host, come i nemici.
- Collegamento con la storia: la colpa del sindaco (vedi `decisioni-14-ottobre.md`) pesa di più se si vede la gente morire.

## Fatto
(vuoto)
