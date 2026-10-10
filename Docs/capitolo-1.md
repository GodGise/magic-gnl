# Capitolo 1: scaletta per farlo uscire

Scritta il 10 ottobre 2026 dal Claude di Giuseppe, guardando il codice e i documenti della repository, e aggiornata lo stesso giorno con la sequenza decisa da Giuseppe. Il capitolo 1 è, in ordine:

1. **Il giorno a Villaggio Lago Nero:** si appare di giorno; tutorial (principi del combattimento), prime quest, esplorazione.
2. **La notte e la razzia degli orchi.**
3. **La boss fight con l'orco enorme: si perde** (scritta per essere persa).
4. **La gattabuia:** risveglio nella cella.
5. **La fuga dalla gattabuia:** si trova l'arma della classe e si esce all'aria aperta.
6. **Fine del capitolo 1.**

È in pratica la "vertical slice" della roadmap (`Docs/roadmap.md`, Gate 2). Le durate sono stime per tre persone part-time: vanno riviste dopo il primo traguardo.

## Decisioni

**Già prese da Giuseppe (10 ottobre):** il capitolo comincia di giorno al villaggio; la boss fight con l'orco enorme si perde; il capitolo finisce dopo la fuga dalla gattabuia (il ritorno al villaggio in rovina è nel capitolo 2).

**Ancora da decidere (Giuseppe):**

1. **Che cosa vuol dire "uscire"?** Una build da far provare agli amici, una demo su Steam, oppure il gioco intero? Cambia tutto il lavoro di rifinitura e di Steam. Proposta: prima una build per gli amici (traguardo 4), poi la demo.
2. **Quanto dura la parte di giorno?** Proposta: 20-30 minuti tra tutorial, quest ed esplorazione, poi il tramonto e la razzia. Il ciclo del gioco ha già 45 minuti di luce.
3. **Come cade l'eroe contro l'orco enorme?** Proposta: si combatte davvero; a un certo punto l'orco diventa invincibile o cala un colpo scenico. In co-op cadono tutti e tre.
4. **Quanto è lunga la fuga?** Proposta: 15-25 minuti, con celle, corridoi con guardie, grotte, un paio di scontri e il ritrovamento dell'arma.
5. **Chi scrive i dialoghi e le quest?** Servono 8 traduzioni per ogni riga: meno righe, meno lavoro. Proposta: massimo 80-100 righe in tutto il capitolo e 4-6 quest brevi.
6. **Il suono.** Oggi tutti i suoni sono provvisori generati dal codice e c'è solo la musica del menu. Serve qualcuno per musica e suoni (profilo già segnato come "terza persona" in `CLAUDE.md`), oppure si usano solo asset CC0.

## A che punto siamo

| Pezzo | Stato |
| --- | --- |
| Combattimento, schivata, parata, aggancio | Fatto, bilanciato da Lorenzo |
| Tre classi con oggetti e incantesimi | Fatto (icone da fare) |
| Interfaccia, menu, opzioni, 8 lingue | Fatto |
| Co-op in locale, difficoltà per numero di giocatori, a terra e rialza | Fatto, da provare ancora molto |
| Porte, leve, bauli, chiavi, checkpoint | Fatto |
| Slot di salvataggio e nome | In prova (ramo `giuseppec/slot-personaggio`) |
| Villaggio Lago Nero | Scena generata dalla mappa, tutto con forme provvisorie (versione intatta; quella in rovina è del capitolo 2) |
| Gattabuia (celle, sotterranei, grotte) | **Non esiste** |
| Tutorial e quest | **Non esistono** |
| Nemici | Un solo tipo: l'orco sgherro. Un'infrastruttura per i boss |
| Modelli 3D veri | Solo 3: una spada, un albero, un pozzo |
| Personaggi | Figura a blocchi provvisoria, senza animazioni vere |
| Dialoghi, PNG, missioni | **Non esistono** |
| Scene scritte (razzia, sconfitta) | **Non esistono** |
| Salvataggio vero | **Non esiste** (solo gli slot) |
| Musica e suoni veri | Solo il menu |

## I traguardi, in ordine

Ogni traguardo finisce con una build giocabile. Si passa al successivo solo se quella build si gioca in co-op senza crash.

### Traguardo 1: il capitolo "in grigio" (tutto con forme provvisorie)
Obiettivo: dal risveglio al villaggio fino all'uscita dalla gattabuia, giocabile per intero, anche brutto.

| Lavoro | Chi | Note |
| --- | --- | --- |
| Sistema di dialogo e PNG (riquadro di testo, tasto E, 8 lingue) | Claude di Giuseppe | Serve prima di tutto il resto della storia |
| Obiettivo a schermo (una riga: "Trova...") e diario minimo | Claude di Giuseppe | Per guidare nell'esplorazione |
| Sistema di scene scritte: dissolvenza, camera, tamburi, trigger | Claude di Giuseppe | Serve per la razzia e la sconfitta, anche in co-op sincronizzato |
| Giorno e notte fissi per il capitolo (tramonto, poi notte della razzia) | Claude di Giuseppe | Il ciclo c'è già, va "diretto" |
| Tutorial: suggerimenti a schermo per muoversi, schivare, parare, attaccare, agganciare, usare l'inventario (con un manichino e qualche nemico facile) | Claude di Giuseppe + Lorenzo | Si attiva al momento giusto, ignorabile |
| Quest: 4-6 brevi (parlare con X, portare Y, difendere Z), con registro nel diario | Giuseppe (idee e testi) + Claude (codice) | Servono dialoghi e diario |
| Percorso di esplorazione di giorno: dove andare, cosa trovare, bauli, chiavi, segreti | Lorenzo | Sulla scena del villaggio (area sua: prima va deciso con Giuseppe chi la possiede) |
| La gattabuia: cella, corridoi con guardie, sotterranei con catene e ossa, grotte, uscita al bosco | Lorenzo (livello) + Claude (script) | Scena nuova; furtività e arma della classe nel bottino |
| Risveglio e uscita: scena scritta dalla cella al bosco, scelta dell'arma della classe | Claude di Giuseppe | Dopo la scena della boss fight |
| Casa dell'eroe, famiglia, 5-8 PNG con poche righe | Giuseppe (testi) | Testi in italiano, poi 8 lingue |
| Razzia: ondate di orchi, case che bruciano, abitanti che fuggono | Lorenzo + Claude | Fuoco e fumo provvisori |
| Orco enorme: scontro scritto per essere perso, vita e attacchi | Claude + Lorenzo | Dopo la decisione 3 |
| Salvataggio vero (falò e checkpoint, posizione, oggetti, nome, classe) | Claude di Giuseppe | Dopo gli slot |
| Un secondo tipo di nemico (per esempio orco con torcia o arciere) | Lorenzo + Claude | Per non avere solo lo sgherro |

### Traguardo 2: il capitolo completo (storia e livello veri, arte ancora provvisoria)
Dialoghi e quest finiti e tradotti, tutorial rifinito, percorso rifinito, razzia bilanciata per 1, 2 e 3 giocatori, boss provato in co-op, gattabuia bilanciata, salvataggio e Continua funzionanti. Si fa provare a due o tre amici che non conoscono il gioco: dove si perdono? Dove muoiono troppo?

### Traguardo 3: l'arte vera (Nazar)
Qui il collo di bottiglia è l'arte: oggi ci sono 3 modelli. Ordine consigliato, dal più visto al meno visto:
1. Personaggio giocabile con scheletro e animazioni base (camminare, correre, attacco, parata, schivata, caduta). Le animazioni sono la parte più lunga: valutare asset CC0 compatibili.
2. Orco sgherro, orco enorme, guardie della gattabuia e abitanti del villaggio.
3. Case del villaggio (3-4 varianti intatte e le versioni bruciate), tempio, taverna, casa dell'eroe, e gli ambienti della gattabuia (celle, sbarre, catene, ossa, grotte).
4. Armi di partenza (ascia da legna, poi spada/scudo, arco/pugnale, bastone).
5. Dettagli: pozzo (fatto), barche, recinti, casse, alberi, rocce, lago.
6. Icone degli oggetti.
Fino a che i modelli veri non ci sono, il villaggio resta in grigio: si fanno i modelli un gruppo alla volta, si testa, si passa al gruppo dopo.

### Traguardo 4: suono, rifinitura e prove
Musica per giorno, tramonto, razzia e boss; suoni veri per colpi, fuoco, orchi; riduzione dei difetti; prove su altri PC (non solo il PC di Giuseppe); opzioni video (risoluzione, limite fotogrammi); nome del protagonista deciso; build per gli amici.

### Traguardo 5: uscita
Dipende dalla decisione 1. Se è una demo su Steam: App ID e Steamworks (inviti e co-op via Steam), pagina "Coming soon", immagini e trailer, build caricata e approvata. Da pianificare con calma, e solo quando il traguardo 4 è superato.

## Cosa NON fare adesso
Altre classi, altre regioni, il villaggio in rovina, le tane degli orchi, la scelta finale della storia, vestiario e personalizzazione del personaggio: tutto bello ma dopo il capitolo 1. Le idee vanno in una lista "dopo", non si buttano.

## Rischi principali
- **Il capitolo è cresciuto:** con tutorial, quest e gattabuia sono due livelli completi invece di uno. Meglio tagliare quest e righe che allungare i tempi.
- **L'arte è il collo di bottiglia**, non il codice: il codice si scrive in giorni, i modelli e le animazioni in settimane.
- **Il suono non ha un responsabile.**
- **Il co-op raddoppia le prove**: ogni scena scritta e ogni boss va provato in 1, 2 e 3 giocatori.
- **Troppe idee nuove**: ogni novità deve rispondere a "serve al capitolo 1?".
