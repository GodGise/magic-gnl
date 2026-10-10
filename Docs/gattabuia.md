# La gattabuia (capitolo 1)

Decisa da Lorenzo il 10 ottobre 2026, scritta con il suo Claude. È la parte del capitolo 1 dopo lo scontro perso con l'orco enorme in piazza (`Docs/capitolo-1.md`, `Docs/storia.md`). Livello: Lorenzo. Script: Claude di Lorenzo. Modelli: Nazar (`Docs/lista-modelli-nazar.txt`, parti 4 e 5).

## Decisioni di Lorenzo

- **Come ci si arriva:** l'orco della piazza non si può battere (statistiche troppo alte; dopo 3 minuti di scontro diventa invulnerabile a ogni danno e a ogni effetto). Quando si muore, invece di rinascere al checkpoint, **ci si risveglia nella gattabuia**, ognuno nella sua cella.
- **Durata:** circa 30 minuti, anche meno.
- **La cella:** si apre forzando una **sbarra arrugginita** (un po' di pressione e cede). La sbarra diventa la prima arma.
- **Co-op:** celle **separate nello stesso corridoio**. **Solo l'host si libera da solo**; gli altri vengono liberati da lui.
- **I prigionieri:** sono **tutti morti**: corpi, sangue, fosse comuni. Non si liberano.
- **Gli orchi:** per fortuna gran parte è uscita marciando, portandosi via quasi tutte le armi. Ne restano pochi: macellai distratti, qualche pattuglia, il carceriere.
- **Miniboss:** il **Carceriere**, nella piazzola sotterranea ai piedi della scalinata d'uscita. Statistiche e bottino da definire.
- **Una zona della mappa esplorabile solo in quel momento.** Dopo la fuga: o **si chiude** (crollo dell'entrata), oppure **diventa il campo base** dove si torna quando ci si riposa. Da scegliere con Giuseppe.

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
| Sbarra arrugginita | La propria cella | Lenta, poco danno, buona portata |
| Mannaia da macellaio | Sul tavolo degli orchi macellai | Veloce e tagliente, portata corta |
| Osso lungo (femore) | Fra i cadaveri del corridoio | Leggero, colpi rapidi e deboli |
| Catena con lucchetto | Appesa al muro di una cella | Portata lunga, lenta |

Sono armi di tutte le classi (anche lo Stregone le usa, come l'ascia della razzia). All'armeria si lasciano: da lì in poi si usa l'arma della propria classe.

## Da decidere

1. **Il Carceriere:** statistiche (vita, danno, attacchi speciali) e bottino. Proposta di bottino: una chiave o un oggetto che serve nel capitolo 2, e la "Balestra del carceriere" per il Ladro.
2. **I parassiti delle grotte:** come sono fatti (ragni con qualcosa di strano: troppi occhi, carapace d'osso, bagliore malato) e se sono legati al bosco morto fuori (lo stesso male che uccide le piante).
3. **Dopo la fuga:** la gattabuia crolla oppure diventa il campo base (con Giuseppe).
4. **Gli altri giocatori in co-op** possono già muoversi nella loro cella mentre aspettano l'host (consigliato), o restano svenuti finché lui non arriva?

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
- **mannaia-01**, **osso-lungo-01**, **catena-lucchetto-01**: le armi improvvisate;
- **cadavere-01/02** e **mucchio-cadaveri-01**: corpi coperti da stracci, sdraiati, seduti o ammassati;
- **tavolo-macellaio-01** e **gancio-carne-01**: la zona dei macellai;
- **fossa-comune-01**: una buca piena di ossa;
- **rastrelliera-vuota-01** e **baule-armi-01**: l'armeria;
- **portone-01**: il portone in cima alla scalinata (due ante, pivot sulle cerniere);
- più avanti, con Giuseppe: **parassita-01** (il ragno strano) e **carceriere** (personaggi con scheletro).
