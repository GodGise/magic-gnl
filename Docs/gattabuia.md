# La gattabuia (capitolo 1)

Decisa da Lorenzo il 10 ottobre 2026, scritta con il suo Claude. È la parte del capitolo 1 dopo lo scontro perso con l'orco enorme in piazza (`Docs/capitolo-1.md`, `Docs/storia.md`). Livello: Lorenzo. Script: Claude di Lorenzo. Modelli: Nazar (`Docs/lista-modelli-nazar.txt`, parti 4 e 5).

## Decisioni di Lorenzo

- **Come ci si arriva:** l'orco della piazza non si può battere (statistiche troppo alte; dopo 3 minuti di scontro diventa invulnerabile a ogni danno e a ogni effetto). Quando si muore, invece di rinascere al checkpoint, **ci si risveglia nella gattabuia**, ognuno nella sua cella.
- **Durata:** circa 30 minuti, anche meno.
- **La cella:** si apre forzando una **sbarra arrugginita** (un po' di pressione e cede). La sbarra diventa la prima arma.
- **Co-op:** celle **separate nello stesso corridoio**. **Solo l'host si libera da solo**; gli altri vengono liberati da lui. Mentre aspettano, gli altri **possono muoversi dentro la loro cella** (11 ottobre).
- **I prigionieri:** sono **tutti morti**: corpi, sangue, fosse comuni. Non si liberano.
- **Gli orchi:** per fortuna gran parte è uscita marciando, portandosi via quasi tutte le armi. Ne restano pochi: macellai distratti, qualche pattuglia, il carceriere.
- **Miniboss:** il **Carceriere**, nella piazzola sotterranea ai piedi della scalinata d'uscita. È il **primo miniboss vero che si può battere**: uno scontro impegnativo ma fattibile con l'equipaggiamento povero di inizio gioco (11 ottobre). Proposta di numeri e mosse più sotto.
- **Parassiti delle grotte:** come **ragni con 6 zampe invece di 8**, deformi e strani, **grandi la metà del personaggio** (11 ottobre).
- **Dopo la fuga la gattabuia non è più accessibile.** Forse in futuro si potrà tornarci, ma non subito (11 ottobre). Niente campo base qui.

## Il percorso

| Zona | Cosa succede | Cosa insegna | Minuti |
| --- | --- | --- | --- |
| 1. Le celle | Risveglio. Si forza la sbarra arrugginita: la cella si apre e la sbarra resta in mano come arma. In co-op l'host apre le celle degli altri | Interagire tenendo premuto, il co-op | 2-3 |
| 2. Il corridoio delle celle | Celle vuote sporche di sangue, altre con cadaveri ammassati dentro. Porta ai corridoi delle guardie | Atmosfera, esplorazione | 2 |
| 3. Gli orchi macellai | Orchi distratti che macellano dei prigionieri: per passare bisogna superarli. Si combatte con la sbarra, con armi improvvisate trovate sul percorso o a mani nude | Furtività ed esecuzione su nemici "ignari", combattere senza la propria arma | 5 |
| 4. L'armeria | Gli orchi si sono portati via quasi tutto: rastrelliere vuote. Nel **baule delle armi** restano solo le **armi base della classe** di ogni giocatore. **Checkpoint subito dopo aver preso le armi** | Inventario, equipaggiarsi | 3 |
| 5. I sotterranei | Dove vengono buttati i resti dei prigionieri: **fosse comuni**, ossa, catene. Qualche orco di pattuglia | Combattere con l'arma vera, aggirare le pattuglie | 6 |
| 6. Le grotte | Un **buco nel muro** dei sotterranei porta a un sentiero di grotte, molto buio. Creature nascoste nell'ombra come **parassiti infestanti**: ragni dall'aspetto diverso dal normale | Buio, aggancio, nemici numerosi e piccoli | 6 |
| 7. La piazzola del Carceriere | Una piazzola sotterranea con una **scalinata** che porta all'uscita. Qui c'è il **Carceriere** | Lo scontro finale della gattabuia | 4 |
| 8. Il portone e il bosco | In cima alla scalinata un **portone**: lo si apre e si è liberi. Fuori, un **bosco malridotto**: vegetazione spoglia e morta, come se qualcosa di nocivo per le piante passasse spesso di lì. Fine del capitolo 1 | | 1 |

## Armi improvvisate (prima dell'armeria)

La sbarra della cella è sempre disponibile. Lungo i corridoi e nella zona dei macellai se ne possono piazzare altre (proposte, da regolare):

| Arma | Dove | Com'è |
| --- | --- | --- |
| Sbarra arrugginita | La propria cella | Lenta, poco danno, buona portata (ancora da programmare) |
| Mannaia del macellaio | Sul tavolo degli orchi macellai | La più forte: taglia bene, portata corta |
| Pugnale arrugginito | Addosso a un cadavere o in un secchio | Velocissimo e poco faticoso, colpisce pochissimo |
| Osso lungo (femore) | Fra i cadaveri del corridoio | Leggero, colpi rapidi e deboli |
| Catenaccio (catena con lucchetto) | Appeso al muro di una cella | Portata lunga, lento |

Sono armi di tutte le classi (anche lo Stregone le usa, al posto del bastone). All'armeria si lasciano: da lì in poi si usa l'arma della propria classe. Numeri esatti in `Docs/armi-improvvisate.md` (ramo `lorenzoc/armi-improvvisate`).

## Il Carceriere

Il primo miniboss che si può battere. Deciso da Lorenzo l'11 ottobre (fase 1 e vita); fase 2, attacchi singoli e bottino sono proposte del suo Claude, da confermare.

**Aspetto:** un orco molto grosso e grasso, avvolto da catene strette come una camicia di forza. Arma: un grosso **tubo di metallo insanguinato** (ci ha già ucciso altri intrusi). In fase 2 spezza le catene: tubo in una mano, un pezzo di catena nell'altra, usato come frusta.

**Come combatte:** alterna le sue **combo** ad **attacchi singoli**. Tutte le mosse si caricano in modo visibile. Come tutti i boss: niente blocchi né stordimenti, rallentamenti dimezzati.

### Fase 1 (da 620 a 300 di vita)

| Mossa | Cosa fa | Danno |
| --- | --- | --- |
| Combo 1 | Mazzata dall'alto, breve recupero, poi due spazzate | 36 + 24 + 24 (84 se prendi tutto) |
| Combo 2 | Tre spazzate rapide, poi **piroetta**: rotea 2 s su se stesso, colpisce ogni 0,6 s chi tocca (3 colpi), intanto si muove il 30% più lento | 24 × 3, poi 12 × 3 (108 se prendi tutto) |
| Singolo: colpo di tubo | Un colpo secco in avanti, carica corta | 28 |
| Singolo: panciata | Si butta avanti con il ventre e spinge via di 3 m | 22 |

### Fase 2 (sotto 300 di vita)

**Si libera:** si arrabbia e spezza le catene, animazione di **3 s** in cui **si può colpire**. Poi tubo in una mano e catena come frusta nell'altra. Regola di Lorenzo: nelle mosse proposte nessun colpo sotto i 21 di danno.

| Mossa | Cosa fa | Danno |
| --- | --- | --- |
| Combo 1: frusta e tubo | Frustata di catena in avanti (arriva a 5 m), passo avanti e mazzata dall'alto, poi una spazzata | 24 + 40 + 26 (90) |
| Combo 2: turbine di catene | Piroetta con la catena tesa (raggio 4 m, più largo della fase 1) per 2 s, colpisce ogni 0,6 s (3 colpi); finisce con uno schianto del tubo a terra (cerchio di 3 m) | 21 × 3, poi 36 (99) |
| Singolo: frustata lunga | Frustata da lontano (6 m): punisce chi resta a distanza | 23 |
| Singolo: catena che afferra | Lancia la catena in linea (8 m): se prende, ti tira davanti a sé e segue un colpo di tubo. Non si para, si schiva di lato | 21 + 28 |

### Numeri per numero di giocatori

Regole di `DifficoltaCoop`: vita dei boss ×2,5 con 2 giocatori e ×4 con 3; attacchi più frequenti ×1,25 e ×1,5; **danno ricevuto dai giocatori ×1,25 e ×1,5** (deciso da Lorenzo l'11 ottobre, prima era ×1,5 e ×2). Il danno è prima dell'armatura del giocatore (vita del giocatore: 100).

| | 1 giocatore | 2 giocatori | 3 giocatori |
| --- | --- | --- | --- |
| Vita totale | 620 | 1550 | 2480 |
| Fase 2 da (48% della vita) | 300 | 750 | 1200 |
| **Fase 1** | | | |
| Mazzata | 36 | 45 | 54 |
| Spazzata | 24 | 30 | 36 |
| Colpo della piroetta | 12 | 15 | 18 |
| Colpo di tubo | 28 | 35 | 42 |
| Panciata | 22 | 27,5 | 33 |
| Combo 1 presa tutta | 84 | 105 | 126 |
| Combo 2 presa tutta | 108 | 135 | 162 |
| **Fase 2** | | | |
| Frustata della combo | 24 | 30 | 36 |
| Mazzata | 40 | 50 | 60 |
| Spazzata | 26 | 32,5 | 39 |
| Colpo del turbine | 21 | 26,25 | 31,5 |
| Schianto | 36 | 45 | 54 |
| Frustata lunga | 23 | 28,75 | 34,5 |
| Catena che afferra (presa + colpo) | 49 | 61,25 | 73,5 |
| Combo 1 presa tutta | 90 | 112,5 | 135 |
| Combo 2 presa tutta | 99 | 123,75 | 148,5 |
| Tempo fra un attacco e l'altro (se 3 s da soli) | 3 s | 2,4 s | 2 s |

Nota per il codice: la fase 2 parte a una **percentuale** della vita (300 su 620, cioè circa il 48%), così vale anche in co-op.

### Bottino

Regola di Lorenzo (11 ottobre): **ogni nemico e ogni boss ha il suo bottino**, che non deve per forza servire a tutti i giocatori. Il Carceriere lascia **una sola cosa forte, sempre la stessa**, più qualche cosa debole a caso (roba da poco ma utile all'inizio).

- **Sempre:** la **chiave del portone** (apre il portone della zona 8; il boss non si può saltare).
- **Sempre, la cosa forte:** **Catena del carceriere**, **armatura media del Guerriero** (deciso da Lorenzo l'11 ottobre). Le catene che lo stringevano, avvolte sul petto: armatura 31, schivata +9 di resistenza, velocità -7% (più armatura della Cotta di maglia rattoppata, 25 / +6,5 / -5%, ma più pesante). Una sola; serve solo al Guerriero.
- **A caso, 3 fra queste 5** (deciso da Lorenzo l'11 ottobre: alla sconfitta il Carceriere ne lascia 3 scelte a caso, le altre 2 no):
  - **Mazza chiodata:** arma improvvisata di tutte le classi, 24 di danno, 0,90 s a colpo;
  - **Mannaia affilata:** arma improvvisata di tutte le classi, 26 di danno, un po' più lenta della mazza (0,98 s a colpo);
  - **Dente d'orco:** amuleto debole, 3 di vita per ogni nemico ucciso;
  - **Lacci di cuoio:** amuleto debole, +10% di ricarica della resistenza;
  - **Pietra torbida del lago:** amuleto debole, 3 di mana per ogni nemico ucciso (utile allo Stregone).

Numeri delle armi in `Docs/armi-improvvisate.md`. Tutti gli oggetti sono già programmati (menu "Crea armi improvvisate", che fa anche i tre amuleti deboli, e "Crea oggetti del Guerriero"). Gli amuleti deboli per ora non hanno malus. Da fare: il sistema del bottino per nemico (oggetti fissi più N a caso, uguali per tutti i giocatori in co-op), da programmare insieme al Carceriere.

## I parassiti delle grotte (proposta, da confermare)

Come li vuole Lorenzo: **ragni con 6 zampe**, deformi e strani, **alti la metà del personaggio** (circa 1 m). Idea per l'aspetto: zampe di lunghezze diverse, corpo gonfio e storto, carapace pallido come osso.

| Cosa | Proposta |
| --- | --- |
| Vita | 30 (un colpo di catenaccio o due di spada) |
| Danno | 8 a morso |
| Movimento | Veloci, si muovono a scatti; escono dall'ombra e dalle pareti |
| Attacco speciale | Un **balzo** da 4 m, visibile un attimo prima (si accucciano) |
| Gruppi | 3-5 per volta, nelle grotte buie |

Pochi danni ciascuno ma tanti insieme: insegnano a usare l'aggancio e a non farsi circondare.

## Da decidere

1. **I parassiti:** confermare i numeri e decidere se sono legati al bosco morto fuori (lo stesso male che uccide le piante).

## Cosa serve fare

**Codice (Claude di Lorenzo):**
- Risveglio nella gattabuia dopo lo scontro con l'orco: ogni giocatore nella sua cella (in co-op l'host nella prima).
- Sbarra arrugginita: si tiene premuto Interagisci per qualche secondo, la cella si apre e la sbarra va in mano come arma. Solo l'host dalla sua cella; poi apre quelle degli altri dall'esterno (oggetto condiviso, come porte e leve).
- Le armi improvvisate (oggetti da raccogliere, di tutte le classi).
- Orchi macellai: ignari finché non si accorgono di te, occupati sul loro tavolo; si possono giustiziare alle spalle.
- Baule delle armi dell'armeria: ognuno trova le armi base della **propria** classe; checkpoint subito dopo.
- I parassiti delle grotte: un nemico nuovo, piccolo, veloce, che arriva in gruppo dall'ombra.
- Il Carceriere, con i numeri regolabili dall'Inspector e le regole del co-op già scritte (a terra una volta sola, poi spettatori).
- Il portone finale e la fine del capitolo (insieme al Claude di Giuseppe, che fa le scene scritte).
- Facoltativo: un comando dell'editor "Crea gattabuia a blocchi" che prepara una scena grigia con le 8 zone, da rifinire a mano.

**Livello (Lorenzo):** la scena della gattabuia in grigio con le 8 zone, i percorsi, i nascondigli, le pattuglie, le armi improvvisate e i checkpoint; poi i numeri (vita e danno di guardie, parassiti e Carceriere).

**Modelli (Nazar):** quasi tutti sono già nella sua lista (parte 4, numeri 44-57: muri, sbarre, porta della cella, catene, torce, paglia, secchio, tavolo, scale, baule, leva; parte 5, numeri 58-69: rocce, stalattiti, pareti di grotta, radici, funghi, ossa, ponte, ragnatele, uscita, alberi morti). Mancano:
- **sbarra-arrugginita-01**: una sbarra della cella staccata e piegata (è anche un'arma);
- **mannaia-01**, **pugnale-arrugginito-01**, **osso-lungo-01**, **catena-lucchetto-01**: le armi improvvisate;
- **cadavere-01/02** e **mucchio-cadaveri-01**: corpi coperti da stracci, sdraiati, seduti o ammassati;
- **tavolo-macellaio-01** e **gancio-carne-01**: la zona dei macellai;
- **fossa-comune-01**: una buca piena di ossa;
- **rastrelliera-vuota-01** e **baule-armi-01**: l'armeria;
- **portone-01**: il portone in cima alla scalinata (due ante, pivot sulle cerniere);
- più avanti, con Giuseppe: **parassita-01** (ragno deforme a 6 zampe, alto circa 1 m) e **carceriere** (orco grosso con mazza ferrata e catena con gancio), personaggi con scheletro.
