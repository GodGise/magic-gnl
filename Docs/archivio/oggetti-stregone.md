# Oggetti dello Stregone

**Stato (9 ottobre 2026):** i 19 oggetti esistono in Unity (menu *magic-gnl → Crea oggetti dello Stregone*, file in `Assets/Dati/Oggetti/Stregone/`), con nomi e descrizioni nelle 8 lingue, il Libro nella seconda casella dell'inventario e la ricarica del mana. I numeri sono quelli qui sotto: vanno provati e ritoccati oggetto per oggetto, come per Guerriero e Ladro (poi menu *Aggiorna oggetti dello Stregone*).

Proposta del 7 ottobre 2026, scritta sullo stesso schema di `Docs/oggetti-guerriero.md` e `Docs/oggetti-ladro.md` (stesse categorie, stesse colonne, stesso riferimento). **Tutti i numeri sono valori di partenza**: vanno rivisti con Lorenzo oggetto per oggetto, come per le altre due classi, e tenuti modificabili dall'Inspector.
La regola del mana (si ricarica da solo, lo Stregone più in fretta delle altre classi, deve uccidere quanto Guerriero e Ladro) è in `Docs/mana.md`.

Lo Stregone uccide **a distanza, con la magia**, ed è fragile: poca armatura e poca vita, ma più mana. Schiva, non para.

## Categorie

| Categoria | Tipi |
| --- | --- |
| Arma | bastone, verga (leggera e veloce), bastone del lago (a due mani: niente libro) |
| Libro | nella casella dello **Scudo**; dà mana, ricarica e potenza |
| Veste (armatura) | leggera, media, pesante |
| Amuleto | magico (bonus semplici), arcano (effetti speciali); ogni amuleto ha anche un malus |

Ogni oggetto è unico, con un nome suo (niente livelli di rarità). Totale: **19 oggetti** (5 armi, 5 libri, 3 vesti, 6 amuleti), come il Ladro.

## Meccaniche nuove (fatte il 9 ottobre, tranne le evocazioni)

- **Lancio base dell'arma**: ogni colpo del bastone è un incantesimo che **costa mana** invece di resistenza. Il danno, la carica e il recupero funzionano come per le armi del Guerriero; in più c'è la portata in metri, come per le armi a distanza del Ladro.
- **Mana massimo e ricarica**: il mana pieno base è 100; libri, vesti e amuleti lo aumentano. La ricarica parte dopo una breve pausa dall'ultimo lancio (regola in `Docs/mana.md`).
- **Ignora armatura**: la magia scavalca in parte l'armatura. La colonna esiste già per le mazze del Guerriero.
- **Libro**: nuovo tipo di oggetto (`DatiLibro`) nella casella dello Scudo, come il Ladro usa la stessa casella per l'arma a distanza. Con il bastone del lago (due mani) il libro si toglie.
- **Statistiche nuove**: *Mana Massimo*, *Recupero Mana*, *Potenza Incantesimi* (percentuale di danno in più) e *Durata Evocazioni*. Le evocazioni e gli incantesimi forti non esistono ancora: i numeri sono pronti per quando si faranno.
- **Come è fatto nel codice**: bastoni e verga sono `DatiArma` di tipo *Bastone* o *Verga* (campi *Costo Mana* e *Velocità Incantesimo*; *Portata* è la distanza a cui la sfera cerca il nemico). Equipaggiandoli il bastone si impugna da solo e la sfera usa i loro numeri. Il libro è `DatiLibro`; le vesti sono `DatiArmatura` con il campo *Mana Massimo*. Amuleti arcani nuovi: *Mana Per Uccisione* e *Ruba Mana*.
- **Ricarica del mana**: in Giocatore Controllo, *Pausa Ricarica Mana* (0,8 s dopo l'ultimo lancio) e *Secondi Ricarica Mana* (12,5 s per la barra intera). Resta anche il 15% di mana per ogni nemico sconfitto che c'era già col bastone della chiesetta: se lo Stregone risulta troppo forte, è il primo numero da abbassare.
- **Velocità della sfera** (non era nella tabella): 16 m/s per i bastoni, 20 per la verga, 13 per il bastone del lago.

## Oggetti

Riferimenti (gli stessi delle altre classi):
- giocatore senza oggetti: critico 10% da ×1,75, armatura 0, vita 100, resistenza 100, **mana 100**;
- orco sgherro: vita 160, armatura 25, danno 26.

Danno efficace contro un'armatura = danno × 100 / (100 + armatura × (1 − quota ignorata)). I critici delle armi si sommano a quelli del giocatore. "Colpi all'orco" = lanci normali senza critici.
Il mana pieno basta per circa 20 secondi di fuoco continuo con il bastone di partenza (regola di `Docs/mana.md`); libri e vesti lo allungano.

### Armi

| # | Oggetto (chiave) | Tipo | Danno | Costo mana | Carica / recupero (s) | Portata | Critico in più | Ignora armatura | Colpi all'orco (mana usato) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Bastone della vecchia vita (`bastone_vecchia_vita`) | Bastone | 22 | 3 | 0,30 / 0,30 | 22 m | — | 15% | 9 (27) |
| 2 | Bastone di quercia nera (`bastone_quercia_nera`) | Bastone | 26 | 3,5 | 0,32 / 0,32 | 22 m | +5% | 15% | 8 (28) |
| 3 | Bastone d'ossidiana (`bastone_ossidiana`) | Bastone | 30 | 4 | 0,35 / 0,35 | 24 m | +5% | 25% | 7 (28) |
| 4 | Verga d'osso (`verga_osso`) | Verga | 15 | 2 | 0,15 / 0,20 | 18 m | +5% | 10% | 14 (28) |
| 5 | Bastone del lago (`bastone_lago`), due mani | Bastone | 48 | 8 | 0,55 / 0,65 | 30 m | +5% | 35% | 4 (32) |

Tempo per uccidere l'orco con i soli lanci normali: da 4,8 s (bastone del lago) a 5,4 s (bastone di partenza), come le armi del Guerriero (4,8 s con la spada).
Parata senza scudo: tutti i bastoni e la verga 10% (costo 30), bastone del lago 15% (costo 35). Lo Stregone schiva, non para.

### Libri (casella dello Scudo)

| # | Oggetto (chiave) | Mana massimo | Recupero mana | Potenza incantesimi | Durata evocazioni | Peso schivata / corsa |
| --- | --- | --- | --- | --- | --- | --- |
| 6 | Libro della vecchia vita (`libro_vecchia_vita`) | +20 | — | — | — | — |
| 7 | Libro dei sussurri (`libro_sussurri`) | +30 | +10% | — | — | — |
| 8 | Libro delle braci (`libro_braci`) | +20 | — | +15% | — | — |
| 9 | Libro dell'evocatore (`libro_evocatore`) | +25 | — | — | +40% | — |
| 10 | Libro del lago nero (`libro_lago_nero`) | +50 | +20% | +10% | — | schivata +4, corsa -1% |

### Vesti (armatura)

| # | Oggetto (chiave) | Peso | Armatura | Mana massimo | Peso sulla schivata / corsa |
| --- | --- | --- | --- | --- | --- |
| 11 | Tunica stracciata (`tunica_stracciata`) | Leggera | 3 | — | — |
| 12 | Veste dell'evocatore (`veste_evocatore`) | Media | 6 | +15 | — |
| 13 | Manto di cenere (`manto_cenere`) | Pesante | 10 | +25 | schivata +2, corsa -1% |

### Amuleti

| # | Oggetto (chiave) | Tipo | Effetto |
| --- | --- | --- | --- |
| 14 | Osso inciso (`osso_inciso`) | Magico | +15 mana massimo; malus: -7% vita massima |
| 15 | Cristallo opaco (`cristallo_opaco`) | Magico | il mana si ricarica il 15% più in fretta; malus: lanci il 7,5% più lenti |
| 16 | Cenere benedetta (`cenere_benedetta`) | Magico | +12% potenza degli incantesimi; malus: -10% mana massimo |
| 17 | Sigillo del focolare (`sigillo_focolare`) | Arcano | +8 mana per nemico ucciso; malus: -5% danno |
| 18 | Occhio del lago (`occhio_lago`) | Arcano | evocazioni il 50% più lunghe; malus: -15% vita massima |
| 19 | Cuore del lago nero (`cuore_lago_nero`) | Arcano | il 5% del danno inflitto torna come mana; malus: -20% vita massima |

## Cose da decidere con Lorenzo

- Se i numeri dei lanci (danno, costo mana, tempi) fanno uccidere lo Stregone alla stessa velocità di Guerriero e Ladro, provando in Unity. Se è più lento, si alza la ricarica del mana; se è troppo forte, si abbassa (`Docs/mana.md`).
- Il mana pieno base è 100 e la ricarica completa 10-15 secondi dopo la pausa (valore dello Stregone): sono i valori di partenza di `Docs/mana.md`, da regolare.
- I nomi sono provvisori, come quelli delle altre classi.

## Per Giuseppe

- Fatti (ramo `lorenzoc/oggetti-stregone`): nomi e descrizioni dei 19 oggetti in `Lingua.cs`, casella **Libro** al posto dello Scudo per lo Stregone (inventario e barra in basso), scheda *Scudi* che per lo Stregone diventa *Libri*, menu *Crea oggetti dello Stregone*.
- Ancora da fare: una **icona** (256x256, sfondo trasparente) per ognuno dei 19 oggetti, da Nazar.
