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

**Decise da Giuseppe l'11 ottobre:**

- **"Uscire" = una demo su Steam** con il capitolo 1 (o un po' meno), per avere qualcosa di concreto e tenere alto il morale. Il gioco completo viene dopo. La durata minima di 2 ore (limite del rimborso su Steam) vale per il gioco venduto, non per la demo.
- **Giorno e notte:** già decisi, 45 minuti di giorno e 50 di notte.
- **Come cade l'eroe:** scontro normale contro l'orco della piazza, che dopo 3 minuti diventa invulnerabile; chi muore si risveglia nella gattabuia (proposta di Lorenzo, approvata).
- **Gattabuia dopo la fuga:** **crolla**, come una vendetta contro gli orchi. **Niente campo base lì**: le tane degli orchi sono collegate da un enorme sistema di caverne e ne arriverebbero altri (Giuseppe, 13 ottobre).
- **Dialoghi e sonoro:** li fa Giuseppe (i dialoghi li rivede anche Lorenzo).
- **Scena `VillaggioLagoNero.unity`:** la gestisce Lorenzo, perché Giuseppe sta finendo le impostazioni e altre cose.

**Ancora da decidere:**

1. **Quanto è lunga la fuga?** Proposta: 15-25 minuti, con celle, corridoi con guardie, grotte, un paio di scontri e il ritrovamento dell'arma.
2. **Quanto dura la parte di giorno?** Lorenzo propone 20-30 minuti di percorso, da confermare giocando.
3. **Quanto lunga deve essere la demo?** Capitolo 1 intero o una parte; si decide dopo la prima build per gli amici.

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
