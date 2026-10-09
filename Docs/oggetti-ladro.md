# Oggetti del Ladro (stile assassino): stato del lavoro

Ramo `lorenzoc/oggetti-ladro`, 7 ottobre 2026. Parte da `lorenzoc/oggetti-guerriero` (PR #12), quindi va unito **dopo** la #12. Numeri **rivisti con Lorenzo oggetto per oggetto** il 7 ottobre.

Il Ladro uccide **a distanza o di soppiatto**. Il nome "Assassino" al posto di "Ladro" è una proposta di Lorenzo da decidere con Giuseppe.

## Categorie

| Categoria | Tipi |
| --- | --- |
| Arma a distanza | arco corto, arco lungo, balestra |
| Arma corta | pugnale, stiletto, doppi pugnali (a due mani: niente scudo) |
| Armatura | leggera, cuoio, ombra |
| Amuleto | magico, arcano (ognuno con bonus e malus) |

## Meccaniche nuove (già funzionanti)

- **Furtività** (statistica nuova): accorcia la vista dei nemici. Con 30% di furtività l'orco ti vede da 12,6 m invece che da 18 m. La danno armature e amuleti del Ladro.
- **Colpo alle spalle**: ogni arma ha un *Moltiplicatore Alle Spalle*. Colpendo un nemico da dietro, nell'arco di 120°, il danno si moltiplica. Vale anche se il nemico ti ha visto; l'esecuzione furtiva resta in più.
- **Svanire nell'ombra** (amuleto Ultimo respiro, tasto **Q**): per 2,5 s i nemici non vedono il giocatore, chi lo inseguiva torna al suo posto e nessuno lo attacca. L'invisibilità finisce allo scadere del tempo o appena si attacca (anche con l'esecuzione furtiva). Ricarica 6 minuti, barra viola nel pannello.
- **Arma a distanza**: nuovo tipo di oggetto (`DatiArmaDistanza`) e nuova casella in `Equipaggiamento`. **Il tiro con l'arco non c'è ancora**: gli oggetti sono pronti con i loro numeri per quando si farà la meccanica.

## Oggetti

Riferimenti: orco sgherro con vita 160, armatura 25 e vista 18 m. "Colpi all'orco" = colpi normali senza critici.

### Armi corte

| Arma | Tipo | Danno | Costo | Carica / recupero (s) | Portata / arco | Critico in più | Ignora armatura | Alle spalle | Colpi all'orco (davanti / alle spalle) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Pugnale da scuoiare | Pugnale | 16 | 12 | 0,15 / 0,22 | 1,4 m / 90° | +10% | — | ×2,5 | 13 / 5 |
| Pugnale ricurvo degli orchi | Pugnale | 22 | 14 | 0,18 / 0,27 | 1,5 m / 100° | +5% | — | ×2 | 10 / 5 |
| Stiletto del tagliagole | Stiletto | 18 | 13 | 0,17 / 0,25 | 1,4 m / 60° | +15%, crit +0,25 | 40% | ×3 | 11 / 4 |
| Stiletto d'ombra | Stiletto | 19 | 14 | 0,16 / 0,24 | 1,4 m / 60° | +15%, crit +0,25 | 30% | ×3 | 9 / 3 |
| Pugnali gemelli | Doppi pugnali (2 mani) | 11 | 10 | 0,10 / 0,18 | 1,4 m / 120° | +10% | — | ×2 | 17 / 9 (ma colpi rapidissimi) |

Parata senza scudo: pugnale da scuoiare 15% (costo 20), ricurvo 20% (25), tagliagole 10% (26), stiletto d'ombra 15% (28), gemelli 10% (30). Il Ladro schiva, non para.

### Armi a distanza (tiro da fare)

| Arma | Tipo | Danno | Costo | Carica / ricarica (s) | Portata | Ignora armatura | Contro chi non ti ha visto | Colpi all'orco (visto / non visto) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Arco corto da caccia | Arco corto | 18 | 12 | 0,4 / 0,35 | 25 m | — | ×1,5 | 12 / 8 |
| Arco d'osso degli orchi | Arco corto | 23 | 14 | 0,45 / 0,4 | 28 m | 7% | ×1,5 | 9 / 6 |
| Arco lungo di tasso | Arco lungo | 30 | 20 | 0,9 / 0,6 | 45 m | 12% | ×2,3 | 7 / 3 |
| Balestra da posta | Balestra | 45 | 18 | 0,3 / 1,6 | 23 m | 40% | ×2 | 5 / 3 |
| Balestra del carceriere | Balestra | 55 | 30 | 0,4 / 2,2 | 40 m | 70% | ×1,75 | 4 / 2 |

### Armature

| Armatura | Tipo | Armatura | Furtività | Vista dell'orco | Peso schivata / corsa |
| --- | --- | --- | --- | --- | --- |
| Giubba scura | Leggera | 7 | 12% | 15,8 m | — |
| Corpetto di cuoio rinforzato | Cuoio | 15 | 8% | 16,6 m | +4 / -1% |
| Manto dell'ombra | Ombra | 3 | 35% | 11,7 m | — |

### Amuleti

| Amuleto | Tipo | Bonus | Malus |
| --- | --- | --- | --- |
| Piuma di civetta | Magico | +15% furtività | -7% vita massima |
| Dente di vipera | Magico | +12% critico | attacchi il 10% più lenti |
| Laccio del borsaiolo | Magico | attacchi il 16% più veloci | -5% critico, -5% vita massima |
| Ultimo respiro | Arcano | tasto Q: svanisce nell'ombra, invisibile ai nemici per 2,5 s (ricarica 6 minuti) | -6% danno |
| Fiato del predatore | Arcano | resistenza +40% più veloce | -17% vita massima |
| Goccia di sangue nero | Arcano | l'8% del danno torna come vita | -5% critico, -7% vita massima |

## Bilanciamento del 9 ottobre (Lorenzo)

Dopo i conti su tutte le combinazioni contro l'orco sgherro (di fronte e alle spalle):

| Oggetto | Prima | Ora | Perché |
| --- | --- | --- | --- |
| Stiletto d'ombra | danno 22, critico +20% | danno 19, critico +15% | Era il migliore anche di fronte (4,7 s per l'orco); ora 5,7 s di fronte e 1,9 s alle spalle |
| Pugnali gemelli | danno 12 | danno 11 + sanguinamento a fine combo (da fare) | Erano i peggiori in tutto, senza un'identità |
| Laccio del borsaiolo | attacchi +10% | attacchi +16% | Con i suoi malus era peggio di nessun amuleto |
| Fiato del predatore | -20% vita massima | -17% vita massima | Malus troppo pesante per un bonus che non aggiunge danno |

Meccaniche nuove decise (da programmare):
- **Sanguinamento dei Pugnali gemelli**: completando la combo intera (4 colpi) il nemico sanguina per 4 s, 4 danni al secondo. Serve una combo propria per ogni arma, con le sue animazioni e il suo numero di colpi (oggi tutte le armi hanno la stessa combo da 3 colpi). Da decidere: se una seconda combo rinnova il sanguinamento o lo somma.
- **Colpi alle spalle**: pieni sui nemici ignari; su un nemico che sta già combattendo il moltiplicatore si dimezza.
- **Allerta** (vale per tutte le classi): un nemico che ti perde di vista (invisibilità, nebbia, furtività) resta in allerta per 15 s: ti cerca ma non è ignaro, quindi niente esecuzione furtiva e niente bonus da "ignaro". Un nemico colpito da lontano resta in allerta 15 s e va verso il punto da cui è arrivato il colpo.
- Pugnale da scuoiare: alle spalle ×2,5 resta così (con il critico diventa ×2 per la regola dei moltiplicatori).

## Per Giuseppe

- Chiavi da aggiungere in `Lingua.cs`: `oggetto.<chiave>.nome` e `.descrizione` per pugnale_scuoiare, pugnale_ricurvo, stiletto_tagliagole, stiletto_ombra, pugnali_gemelli, arco_caccia, arco_osso, arco_tasso, balestra_posta, balestra_carceriere, giubba_scura, corpetto_cuoio, manto_ombra, piuma_civetta, dente_vipera, laccio_borsaiolo, ultimo_respiro, fiato_predatore, goccia_sangue_nero.
- `Equipaggiamento` ha la casella nuova `ArmaDistanza`, con `TogliArmaDistanza()`. Per l'inventario approvato (`Docs/interfaccia.md`, 4 caselle) il Ladro ne ha una quinta: proposta di usare la casella dello **Scudo** come **Arma a distanza** quando la classe è il Ladro, così le caselle restano 4 e i tasti 1 e 2 diventano pugnale e arco.
- L'amuleto Ultimo respiro usa il tasto **Q** (pad: croce su): va previsto nell'HUD (rombo dell'amuleto con la ricarica).
- File degli oggetti: menu *magic-gnl → Crea oggetti del Ladro*, in `Assets/Dati/Oggetti/Ladro/`.
