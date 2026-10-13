# Decisioni di Giuseppe, 14 ottobre 2026 (storia e dialoghi del capitolo 1)

Queste decisioni **valgono più** di `storia.md`, `quest-capitolo-1.md` e `dialoghi-capitolo-1.md` dove c'è contrasto. Giuseppe giudica i dialoghi della bozza 2 troppo scollegati: quest e battute sembrano messe a caso. Vanno rifatti (vedi "Compito per Lorenzo").

**Nota:** le quest 1-6 sono già nel codice e in `Lingua.cs` (8 lingue), quindi la riscrittura cambia cose già fatte. Per questo si scrive prima la bozza 3 e si approva; solo dopo si toccano il codice e le traduzioni.

## 1. Il protagonista è il sindaco del villaggio

- L'eroe è il **capo di Villaggio Lago Nero** (sindaco / capovillaggio). Così si spiega perché gira ad aiutare tutti: è il suo lavoro e la gente si rivolge a lui.
- Resta valido tutto il resto: ex guerriero, ex ladro o ex stregone, vita semplice, Maren, Tobin. Il passato di classe è il motivo per cui gli altri lo hanno scelto o gli si affidano.
- **Cambiano di conseguenza:**
  - Maren non "manda" più l'eroe a fare commissioni: gli ricorda i suoi doveri di sindaco.
  - Brannoc, Ilse, Edric e Hobb non fanno favori: portano **problemi al sindaco** e chiedono una decisione o un aiuto.
  - Gli abitanti si rivolgono a lui con rispetto (nome o titolo ancora da decidere, insieme al nome del protagonista).
  - Nel racconto di `storia.md` va aggiunto che era il capo del villaggio.
- Effetto sulla storia: perdere il villaggio e la gente è anche una **colpa**, non solo un lutto. Può dare più peso alla razzia e alla scelta finale.

## 2. Quest del fabbro: minigioco per forgiare

- Da Brannoc **non si prova solo la lama**: c'è un **minigioco di forgiatura**. Si scalda il ferro, si batte a tempo con il martello, si tempra e si crea l'arma (una spada, o altro per le altre classi).
- Idea di base (da confermare con Lorenzo):
  - ritmo: clic al momento giusto mentre il ferro è incandescente; la qualità dell'arma dipende da quanti colpi sono precisi;
  - Guerriero: forgia la lama che userà nel tutorial;
  - Ladro: forgia punte di freccia o un pugnale;
  - Stregone: niente forgia: un'attività simile con il bastone (per esempio preparare un oggetto con Edric al tempio).
- Collegamento con la storia: l'arma forgiata dal sindaco è quella che gli orchi rubano. Brannoc forgia con lui perché il villaggio deve armarsi dopo le voci sugli orchi.
- Co-op: il minigioco si fa per ogni giocatore, senza bloccare gli altri.
- Per il codice: nuova funzione; la scrive il Claude di Giuseppe dopo che i dialoghi sono approvati. Non è area di Lorenzo.

## 3. Quest dell'addestramento: più dialogo, più motivazione

- La quest 2 (Prova la lama / Lo studio delle magie) è **troppo corta**. Deve durare di più e avere più dialogo.
- **Va spiegato perché l'eroe si è allontanato** da ciò che sapeva fare:
  - **Stregone:** perché ha giurato di non usare più la magia (o perché si è allontanato dalle arti mistiche). La storia dice che chi usa la magia spesso non torna lo stesso: serve un **fatto concreto** (cosa gli è successo, chi ha perso, cosa ha capito). Edric lo sa.
  - **Guerriero:** perché ha appeso la spada al chiodo.
  - **Ladro:** perché ha smesso di rubare.
- Il motivo si racconta a poco a poco, non in un'unica battuta, e **torna più avanti**: nella notte della razzia (riprendere in mano l'arma costa) e nella scelta finale.
- Il giocatore deve capire perché oggi **ricomincia**: ora il villaggio ha bisogno di lui.
- Motivi proposti (da scegliere o cambiare, ne decide Giuseppe):
  - Guerriero: una guerra lontana in cui ha ucciso più di quanto serviva; ha giurato di non tornare a combattere.
  - Ladro: un colpo finito male in cui qualcuno è morto al posto suo.
  - Stregone: un incantesimo uscito di mano che ha ferito qualcuno che amava.
- Meglio un motivo diverso per classe, ma con lo stesso tema: **la colpa e il rifiuto**, che la razzia mette in crisi.

## 4. Dialoghi e quest devono avere un filo logico

- Problema (parole di Giuseppe): quest e dialoghi sono scollegati e a caso.
- Regola nuova: **ogni quest nasce dalla precedente** e porta alla successiva. Tutta la giornata ha un solo filo.
- Filo proposto (Lorenzo lo può cambiare, ma deve essercene uno):
  1. **Mattina:** circolano voci su fuochi e tamburi nel bosco (le porta Hobb, o un viandante). Il sindaco deve decidere cosa fare.
  2. **Preparare il villaggio:** armi dal fabbro (forgiatura), un po' di allenamento per ricordare come si combatte (con la ragione sopra).
  3. **Provviste e sicurezza:** il granaio infestato dai ratti, segno che il lago sta cambiando.
  4. **Cure:** le erbe e le pozioni di Ilse, che sa più di quello che dice sul lago.
  5. **Il passato:** Edric e la cripta, la leggenda e l'amuleto.
  6. **Sera:** taverna, Hobb conferma le voci, i tamburi.
  Ogni personaggio **cita** quello che è successo prima e quello che succederà.
- Più lunghi: **tutti i dialoghi** vanno ampliati (la bozza era circa 90 righe: si può salire). Le quest 3 e 5 si tagliano solo se proprio non c'è tempo.
- Dialoghi di Maren e Tobin: più presenti lungo la giornata (non solo all'inizio e alla fine), perché la perdita in serata pesi.
- Sempre valido: niente testi narrativi lunghi dati in blocco al giocatore (preferenza di Giuseppe); si allunga con **scambi brevi e collegati**, non con monologhi.

## Compito per Lorenzo

**Assegnato da Giuseppe il 14 ottobre.** Lorenzo rivede i dialoghi con il suo Claude. Cosa serve:

1. Leggere questo file, `appunti-per-lorenzo.md`, `quest-capitolo-1.md` e `dialoghi-capitolo-1.md`.
2. **Riscrivere i dialoghi** (bozza 3) applicando i punti 1-4:
   - il protagonista è il sindaco;
   - quest 2 del fabbro con il minigioco di forgiatura (indicare dove si inserisce nel dialogo);
   - quest dell'addestramento più lunga, con il motivo per cui l'eroe si era allontanato (uno per classe);
   - **filo logico** unico fra tutte le quest e le battute che si richiamano a vicenda.
3. Consegnare in `Docs/dialoghi-capitolo-1.md` (bozza 3) e scrivere in `Docs/da-approvare.md` che è pronta per Giuseppe.
4. **Non tradurre ancora** nelle 8 lingue e non toccare `Lingua.cs` né il codice delle quest già fatte: prima approva Giuseppe.
5. Per ogni quest, annotare una riga con cosa serve al codice (nuovi oggetti, trigger, minigioco, cosa cambia rispetto a quanto già fatto), così il Claude di Giuseppe può farlo.

Da decidere insieme prima di scrivere: nome del protagonista e come lo chiamano gli abitanti (titolo di sindaco).
