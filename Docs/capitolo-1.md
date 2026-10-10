# Capitolo 1: scaletta per farlo uscire

Scritta il 10 ottobre 2026 dal Claude di Giuseppe, guardando il codice e i documenti della repository. Il capitolo 1 è **l'esplorazione di Villaggio Lago Nero e, di notte, la razzia degli orchi**. È in pratica la "vertical slice" della roadmap (`Docs/roadmap.md`, Gate 2). Le durate sono stime per tre persone part-time: vanno riviste dopo il primo traguardo.

## Decisioni che servono prima di tutto (Giuseppe)

1. **Che cosa vuol dire "uscire"?** Una build da far provare agli amici, una demo su Steam, oppure il gioco intero? Cambia tutto il lavoro di rifinitura e di Steam. Proposta: prima una build per gli amici (traguardo 4), poi la demo.
2. **Dove finisce il capitolo?** Proposta: con l'eroe a terra davanti all'orco enorme, schermo nero. La gattabuia sarebbe il capitolo 2.
3. **L'orco enorme si può battere?** Proposta: no, è uno scontro scritto. Si combatte, ma a un certo punto l'orco diventa invincibile o cala un colpo scenico e l'eroe cade. In co-op cadono tutti e tre.
4. **Quanto dura l'esplorazione?** Proposta: 20-30 minuti, di giorno e al tramonto, poi la razzia di notte. Il ciclo del gioco ha già 45 minuti di luce.
5. **Chi scrive i dialoghi?** Servono 8 traduzioni per ogni riga: meno righe, meno lavoro. Proposta: massimo 60-80 righe in tutto il capitolo.
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
| Villaggio Lago Nero | Scena generata dalla mappa, tutto con forme provvisorie |
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
Obiettivo: dall'inizio alla caduta davanti all'orco, giocabile per intero, anche brutto.

| Lavoro | Chi | Note |
| --- | --- | --- |
| Sistema di dialogo e PNG (riquadro di testo, tasto E, 8 lingue) | Claude di Giuseppe | Serve prima di tutto il resto della storia |
| Obiettivo a schermo (una riga: "Trova...") e diario minimo | Claude di Giuseppe | Per guidare nell'esplorazione |
| Sistema di scene scritte: dissolvenza, camera, tamburi, trigger | Claude di Giuseppe | Serve per la razzia e la sconfitta, anche in co-op sincronizzato |
| Giorno e notte fissi per il capitolo (tramonto, poi notte della razzia) | Claude di Giuseppe | Il ciclo c'è già, va "diretto" |
| Percorso di esplorazione: dove andare, cosa trovare, bauli, chiavi, segreti | Lorenzo | Sulla scena del villaggio (area sua: prima va deciso con Giuseppe chi la possiede) |
| Casa dell'eroe, famiglia, 5-8 PNG con poche righe | Giuseppe (testi) | Testi in italiano, poi 8 lingue |
| Razzia: ondate di orchi, case che bruciano, abitanti che fuggono | Lorenzo + Claude | Fuoco e fumo provvisori |
| Orco enorme: scontro scritto, vita e attacchi | Claude + Lorenzo | Dopo la decisione 3 |
| Salvataggio vero (falò e checkpoint, posizione, oggetti, nome, classe) | Claude di Giuseppe | Dopo gli slot |
| Un secondo tipo di nemico (per esempio orco con torcia o arciere) | Lorenzo + Claude | Per non avere solo lo sgherro |

### Traguardo 2: il capitolo completo (storia e livello veri, arte ancora provvisoria)
Dialoghi finiti e tradotti, percorso rifinito, razzia bilanciata per 1, 2 e 3 giocatori, boss provato in co-op, salvataggio e Continua funzionanti. Si fa provare a due o tre amici che non conoscono il gioco: dove si perdono? Dove muoiono troppo?

### Traguardo 3: l'arte vera (Nazar)
Qui il collo di bottiglia è l'arte: oggi ci sono 3 modelli. Ordine consigliato, dal più visto al meno visto:
1. Personaggio giocabile con scheletro e animazioni base (camminare, correre, attacco, parata, schivata, caduta). Le animazioni sono la parte più lunga: valutare asset CC0 compatibili.
2. Orco sgherro e orco enorme.
3. Case del villaggio (3-4 varianti intatte e le versioni bruciate), tempio, taverna, casa dell'eroe.
4. Armi di partenza (ascia da legna, poi spada/scudo, arco/pugnale, bastone).
5. Dettagli: pozzo (fatto), barche, recinti, casse, alberi, rocce, lago.
6. Icone degli oggetti.
Fino a che i modelli veri non ci sono, il villaggio resta in grigio: si fanno i modelli un gruppo alla volta, si testa, si passa al gruppo dopo.

### Traguardo 4: suono, rifinitura e prove
Musica per giorno, tramonto, razzia e boss; suoni veri per colpi, fuoco, orchi; riduzione dei difetti; prove su altri PC (non solo il PC di Giuseppe); opzioni video (risoluzione, limite fotogrammi); nome del protagonista deciso; build per gli amici.

### Traguardo 5: uscita
Dipende dalla decisione 1. Se è una demo su Steam: App ID e Steamworks (inviti e co-op via Steam), pagina "Coming soon", immagini e trailer, build caricata e approvata. Da pianificare con calma, e solo quando il traguardo 4 è superato.

## Cosa NON fare adesso
Altre classi, altre regioni, la scelta finale della storia, la gattabuia completa, costruzione di personaggi e vestiario: tutto bello ma dopo il capitolo 1. Le idee vanno in una lista "dopo", non si buttano.

## Rischi principali
- **L'arte è il collo di bottiglia**, non il codice: il codice si scrive in giorni, i modelli e le animazioni in settimane.
- **Il suono non ha un responsabile.**
- **Il co-op raddoppia le prove**: ogni scena scritta e ogni boss va provato in 1, 2 e 3 giocatori.
- **Troppe idee nuove**: ogni novità deve rispondere a "serve al capitolo 1?".
