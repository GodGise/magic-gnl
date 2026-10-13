# Monete e mercato

Richiesta di Lorenzo, 13 ottobre 2026: monete legate alla storia (niente le solite monete d'argento), un mercato dove ogni cosa si trova in un posto preciso, prezzi provvisori.

## La moneta scelta per ora: le Lacrime del lago

Sassolini neri, lisci e lucidi come vetro, con un riflesso blu. Il Lago Nero li lascia sulla riva **solo dopo le tempeste**: nessuno sa da dove vengano. Sono rare e non si possono falsificare, per questo al villaggio valgono più del ferro. I vecchi dicono che sono le lacrime di quello che dorme sotto il lago (gancio per la storia: forse c'entrano con la reliquia).

- Nel gioco: "45 lacrime", "1 lacrima". Nel codice: `Borsa` (un numero, non riempie lo zaino).
- Si cambiano nome e testi in `Lingua.cs` (chiavi `valuta.*`) se si sceglie un'altra proposta qui sotto: il codice non cambia.

## Altre proposte (da scegliere insieme)

| Nome | Materiale | Legame con la storia |
|---|---|---|
| **Lacrime del lago** (scelta per ora) | Ossidiana nera lasciata dal lago dopo le tempeste | Il lago nero e quello che ci dorme sotto |
| **Scaglie nere** | Scaglie dure e cangianti dei grossi pesci del lago, forate e infilate su cordini di cuoio (10 scaglie = un filo) | Il villaggio vive del lago anche se nessuno ci fa il bagno |
| **Chiodi di guardia** | Chiodi quadrati di ferro nero battuti alla fucina | Gli antenati pagavano le sentinelle in chiodi per costruire le case. Moneta povera, da villaggio |
| **Segni di veglia** | Dischetti d'osso o di corno incisi con la runa della guardia | Gli antichi li davano a chi vegliava sul lago; oggi nessuno crede più alla storia e sono solo soldi |
| **Gocce di pece** | Grumi di resina nera dei pini del bosco morto, induriti; profumano se si scaldano | Gli orchi le cercano per le torce: avere pece è pericoloso |

Si possono anche usare **tre monete a gradini**, per esempio Chiodi (spiccioli) → Scaglie (10 chiodi) → Lacrime (100 chiodi). Per ora una sola moneta: è più semplice da bilanciare.

## Da dove arrivano le Lacrime

- **15** all'inizio della partita (campo "Partenza" della `Borsa`).
- **20** da Hobb, alla fine della quest 3 (i ratti del granaio). Componente `DaiMonete`, si usa per ogni ricompensa.
- Vendendo i fiori a Ilse.
- Da fare: bottino dei nemici (orchi, Carceriere), bauli, ricompense delle altre quest.

## Il mercato: ogni cosa ha il suo posto

Ogni mercante vende e compra solo le sue cose. Lo stesso componente `Negozio` serve per tutti, con tre schede: **Baratto** (il modo più conveniente), **Compra** (con le Lacrime, costa di più), **Vendi**.

| Dove | Chi | Vende | Compra | Stato |
|---|---|---|---|---|
| Bottega delle erbe | **Ilse** | Pozioni (baratto con i fiori o con le Lacrime) | Fiori | **Fatto** |
| Fucina | Brannoc | Armi, scudi e armature, riparazioni | Ferro vecchio, armi trovate | Proposta (dal capitolo 2, quando le armi si trovano in gattabuia) |
| Tempio, studio | Edric | Libri e incantesimi dello Stregone | Pagine e oggetti antichi | Proposta |
| Granaio sul lago | Hobb | Provviste, torce, corda | Pesce, pelli dei ratti giganti | Proposta |
| Taverna | L'oste | Bevande che danno piccoli effetti, voci e indizi | — | Proposta (quest 6) |
| Ambulante | Un mercante di passaggio | Amuleti rari, a prezzi alti | Tutto, ma pagando poco | Proposta |

## Prezzi provvisori (bottega di Ilse)

| Cosa | Baratto | Compra | Vendi a Ilse |
|---|---|---|---|
| Fiore di riva | — | — | 4 lacrime |
| Cardo nero | — | — | 5 lacrime |
| Lunaria | — | — | 7 lacrime |
| Pozione di Ilse | 3 Fiori di riva → 2 pozioni | 30 lacrime | — |
| Tonico di cardo | 2 Cardi neri | 35 lacrime | — |
| Infuso di lunaria | 2 Lunarie | 40 lacrime | — |
| Balsamo di riva | 2 Fiori di riva + 1 Cardo nero | 70 lacrime | — |
| Elisir del crepuscolo | 1 di ogni fiore | 90 lacrime | — |

Il conto: 3 Fiori di riva venduti valgono 12 lacrime, barattati danno 2 pozioni che costerebbero 60. Comprare è sempre possibile ma costa circa cinque volte di più: conviene andare a raccogliere i fiori.

I numeri si cambiano dall'Inspector, voce per voce, in `Assets/Dati/Ricette/`.
