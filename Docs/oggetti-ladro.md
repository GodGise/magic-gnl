# Oggetti del Ladro (stile assassino): stato del lavoro

Ramo `lorenzoc/oggetti-ladro`, 7 ottobre 2026. Parte da `lorenzoc/oggetti-guerriero` (PR #12), quindi va unito **dopo** la #12. È una prima proposta: i numeri si rivedono con Lorenzo oggetto per oggetto, come per il Guerriero.

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
- **Svanire nell'ombra** (amuleto Ultimo respiro, tasto **Q**): per 2,5 s i nemici non vedono il giocatore, chi lo inseguiva torna al suo posto e nessuno lo attacca. Ricarica 6 minuti, barra viola nel pannello.
- **Arma a distanza**: nuovo tipo di oggetto (`DatiArmaDistanza`) e nuova casella in `Equipaggiamento`. **Il tiro con l'arco non c'è ancora**: gli oggetti sono pronti con i loro numeri per quando si farà la meccanica.

## Oggetti

Riferimenti: orco sgherro con vita 160, armatura 25 e vista 18 m. "Colpi all'orco" = colpi normali senza critici.

### Armi corte

| Arma | Tipo | Danno | Costo | Carica / recupero (s) | Portata / arco | Critico in più | Ignora armatura | Alle spalle | Colpi all'orco (davanti / alle spalle) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Pugnale da scuoiare | Pugnale | 16 | 12 | 0,15 / 0,22 | 1,4 m / 90° | +10% | — | ×2,5 | 13 / 5 |
| Pugnale ricurvo degli orchi | Pugnale | 22 | 14 | 0,18 / 0,27 | 1,5 m / 100° | +5% | — | ×2 | 10 / 5 |
| Stiletto del tagliagole | Stiletto | 20 | 13 | 0,17 / 0,25 | 1,4 m / 60° | +15%, crit +0,25 | 50% | ×3 | 9 / 3 |
| Stiletto d'ombra | Stiletto | 24 | 14 | 0,16 / 0,24 | 1,4 m / 60° | +20%, crit +0,25 | 30% | ×3 | 8 / 3 |
| Pugnali gemelli | Doppi pugnali (2 mani) | 12 | 10 | 0,10 / 0,18 | 1,4 m / 120° | +10% | — | ×2 | 17 / 9 (ma colpi rapidissimi) |

Parata senza scudo: dal 10% al 25%. Il Ladro schiva, non para.

### Armi a distanza (tiro da fare)

| Arma | Tipo | Danno | Costo | Carica / ricarica (s) | Portata | Ignora armatura | Contro chi non ti ha visto | Colpi all'orco (visto / non visto) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Arco corto da caccia | Arco corto | 18 | 12 | 0,4 / 0,35 | 25 m | — | ×1,5 | 12 / 8 |
| Arco d'osso degli orchi | Arco corto | 22 | 14 | 0,45 / 0,4 | 28 m | — | ×1,5 | 10 / 7 |
| Arco lungo di tasso | Arco lungo | 32 | 20 | 0,9 / 0,6 | 45 m | — | ×2 | 7 / 4 |
| Balestra da posta | Balestra | 45 | 18 | 0,3 / 1,6 | 35 m | 50% | ×2 | 4 / 2 |
| Balestra del carceriere | Balestra | 60 | 25 | 0,4 / 2,2 | 40 m | 70% | ×1,5 | 3 / 2 |

### Armature

| Armatura | Tipo | Armatura | Furtività | Vista dell'orco | Peso schivata / corsa |
| --- | --- | --- | --- | --- | --- |
| Giubba scura | Leggera | 5 | 10% | 16,2 m | — |
| Corpetto di cuoio rinforzato | Cuoio | 15 | 5% | 17,1 m | +2 / -1% |
| Manto dell'ombra | Ombra | 3 | 30% | 12,6 m | — |

### Amuleti

| Amuleto | Tipo | Bonus | Malus |
| --- | --- | --- | --- |
| Piuma di civetta | Magico | +15% furtività | -5% danno |
| Dente di vipera | Magico | +12% critico | -10% armatura totale |
| Laccio del borsaiolo | Magico | attacchi il 10% più veloci | -10% vita massima |
| Ultimo respiro | Arcano | tasto Q: svanisce nell'ombra, invisibile ai nemici per 2,5 s (ricarica 6 minuti) | -6% danno |
| Fiato del predatore | Arcano | resistenza +40% più veloce | -20% vita massima |
| Goccia di sangue nero | Arcano | l'8% del danno torna come vita | -10% vita massima |

## Per Giuseppe

- Chiavi da aggiungere in `Lingua.cs`: `oggetto.<chiave>.nome` e `.descrizione` per pugnale_scuoiare, pugnale_ricurvo, stiletto_tagliagole, stiletto_ombra, pugnali_gemelli, arco_caccia, arco_osso, arco_tasso, balestra_posta, balestra_carceriere, giubba_scura, corpetto_cuoio, manto_ombra, piuma_civetta, dente_vipera, laccio_borsaiolo, ultimo_respiro, fiato_predatore, goccia_sangue_nero.
- `Equipaggiamento` ha la casella nuova `ArmaDistanza`, con `TogliArmaDistanza()`.
- File degli oggetti: menu *magic-gnl → Crea oggetti del Ladro*, in `Assets/Dati/Oggetti/Ladro/`.
