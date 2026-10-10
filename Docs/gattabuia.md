# La gattabuia (capitolo 1)

Decisa da Lorenzo il 10 ottobre 2026, scritta con il suo Claude. È la parte del capitolo 1 dopo lo scontro perso con l'orco enorme in piazza (`Docs/capitolo-1.md`, `Docs/storia.md`). Livello: Lorenzo. Script: Claude di Lorenzo. Modelli: Nazar (`Docs/lista-modelli-nazar.txt`, parti 4 e 5).

## Decisioni di Lorenzo

- **Come ci si arriva:** l'orco della piazza non si può battere (statistiche troppo alte; dopo 3 minuti di scontro diventa invulnerabile a ogni danno e a ogni effetto). Quando si muore, invece di rinascere al checkpoint, **ci si risveglia nella gattabuia**, ognuno nella sua cella.
- **Durata:** circa 30 minuti, anche meno.
- **La cella:** si apre forzando una **sbarra arrugginita** (un po' di pressione e cede).
- **Co-op:** celle **separate nello stesso corridoio**. **Solo l'host si libera da solo**; gli altri vengono liberati da lui.
- **I prigionieri:** sono **tutti morti**. Sono ambientazione (corpi, ossa), non si liberano.
- **Miniboss:** nelle grotte, è **l'ultimo scontro prima dell'uscita**: battuto lui si è liberi.
- **Una zona della mappa esplorabile solo in quel momento.** Dopo la fuga: o **si chiude** (crollo dell'entrata), oppure **diventa il campo base** dove si torna quando ci si riposa. Da scegliere con Giuseppe.

## Il percorso

| Zona | Cosa succede | Cosa insegna | Minuti |
| --- | --- | --- | --- |
| 1. Celle | Risveglio. Si forza la sbarra arrugginita; in co-op l'host apre le celle degli altri. Corpi degli altri prigionieri | Interagire tenendo premuto, il co-op | 2-3 |
| 2. Corridoi delle guardie | Orchi che dormono e qualcuno di ronda; nicchie e ombre per nascondersi | Furtività, esecuzione furtiva, "ignaro" e allerta | 5 |
| 3. Primo scontro | Una guardia sveglia che non si può aggirare | Schivata e attacco senza la propria arma | 3 |
| 4. Sala del bottino | Il mucchio delle cose rubate ai villaggi: l'**arma della propria classe** e qualche oggetto | Inventario, equipaggiarsi | 3 |
| 5. Sotterranei | Catene, ossa, prigionieri morti, trappole a spuntoni, una leva e una porta, un checkpoint | Trappole, leve, checkpoint | 6 |
| 6. Grotte | Spazi grandi e bui, acqua bassa, "strane cose che si muovono nell'ombra", stretti passaggi | Combattere con l'arma vera, aggancio | 6 |
| 7. Miniboss | Lo scontro finale della gattabuia, in una grotta grande | Tutto insieme | 4 |
| 8. Uscita | Si sale verso una luce lontana e si esce nel bosco. Fine del capitolo 1 | | 1 |

## Da decidere

1. **Con che cosa si combatte prima del bottino?** L'ascia della razzia l'hanno presa gli orchi. Proposta: la **sbarra arrugginita** strappata alla cella diventa un'arma improvvisata (poco danno, lenta) fino alla sala del bottino. In alternativa a mani nude.
2. **Chi è il miniboss delle grotte?** Proposte: il **Carceriere** (un orco grosso con le chiavi, collegato alla "Balestra del carceriere" del Ladro), oppure una **creatura delle grotte** (una delle "cose nell'ombra").
3. **Dopo la fuga:** la gattabuia crolla oppure diventa il campo base (con Giuseppe).
4. **Gli altri giocatori in co-op** possono già muoversi nella loro cella mentre aspettano l'host (consigliato), o restano svenuti finché lui non arriva?

## Cosa serve fare

**Codice (Claude di Lorenzo, dopo le decisioni):**
- Risveglio nella gattabuia dopo lo scontro con l'orco: ogni giocatore nella sua cella (in co-op l'host nella prima).
- Sbarra arrugginita: si tiene premuto Interagisci per qualche secondo e la cella si apre; solo l'host può farlo dalla sua cella, poi apre quelle degli altri dall'esterno (oggetto condiviso, come porte e leve).
- Orchi che dormono: non si guardano intorno, si svegliano se colpiti, se vedono un'esecuzione vicina (allerta) o se il giocatore corre loro accanto.
- Arma improvvisata (se si sceglie la sbarra).
- Il miniboss delle grotte, con i numeri regolabili dall'Inspector e le regole del co-op già scritte (a terra una volta sola, poi spettatori).
- Fine del capitolo all'uscita (insieme al Claude di Giuseppe, che fa le scene scritte).
- Facoltativo: un comando dell'editor "Crea gattabuia a blocchi" che prepara una scena grigia con le 8 zone, da rifinire a mano.

**Livello (Lorenzo):** la scena della gattabuia in grigio, con le 8 zone, i percorsi, i nascondigli, le guardie, le trappole e i checkpoint; poi i numeri (vita e danno di guardie e miniboss, durata delle zone).

**Modelli (Nazar):** quasi tutti sono già nella sua lista (parte 4, numeri 44-57: muri, sbarre, porta della cella, catene, torce, paglia, secchio, tavolo, scale, baule, leva; parte 5, numeri 58-66: rocce, stalattiti, pareti di grotta, radici, funghi, ossa, ponte di legno, ragnatele, uscita). Mancano:
- **sbarra-arrugginita-01**: una sbarra della cella piegata o staccata (anche come arma, se si sceglie così);
- **mucchio-bottino-01**: un mucchio di oggetti rubati (sacchi, casse rotte, monete, armi sparse);
- **prigioniero-morto-01/02**: corpi coperti da stracci, sdraiati o seduti contro il muro;
- il **miniboss**, quando sarà deciso.
