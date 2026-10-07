# Oggetti di Ladro e Stregone (proposta, da approvare)

Stesso schema del Guerriero: 19 oggetti per classe, 4 caselle (Arma, seconda mano al posto dello scudo, Armatura, Amuleto). Statistiche mostrate nell'inventario: Vita, Armatura, Danno, Critico.
Tutti i numeri sono **valori di partenza**, da affinare giocando e da tenere modificabili in Unity. Una icona per oggetto (256x256, sfondo trasparente) va a Nazar.

Nota: questo documento è scritto senza avere sotto gli occhi `Docs/oggetti-guerriero.md` (sta sul ramo `lorenzoc/oggetti-guerriero`, non ancora unito a `main`). Quando sarà unito, va allineato allo schema del Guerriero (nomi dei campi, ordine, tipi).

## Ladro (19 oggetti)

Mani: **pugnale** (tasto 1) e **arco** (tasto 2). Niente scudo, quindi niente confronto con lo scudo in inventario. Leggero e veloce, poca armatura.

### Pugnali (casella Arma)
| Oggetto | Danno | Critico | Nota |
| --- | --- | --- | --- |
| Pugnale del bottino | 8 | 10% | Arma di partenza, trovata nel bottino degli orchi |
| Stiletto d'osso | 10 | 15% | |
| Lama di ragno | 12 | 20% | |
| Pugnale del silenzio | 14 | 25% | Colpo alle spalle più forte |

### Archi (seconda mano)
| Oggetto | Danno | Critico | Nota |
| --- | --- | --- | --- |
| Arco di salice | 10 | 5% | Arma di partenza |
| Arco d'osso | 13 | 8% | |
| Arco del cacciatore | 16 | 10% | |
| Arco del corvo | 19 | 15% | Frecce più veloci |

### Armature
| Oggetto | Armatura | Vita | Nota |
| --- | --- | --- | --- |
| Giacca di cuoio logora | 4 | +0 | Di partenza |
| Cappa da esploratore | 6 | +5 | |
| Giubba borchiata | 9 | +8 | |
| Mantello d'ombra | 8 | +10 | Più silenzioso |
| Armatura di scaglie leggere | 12 | +10 | |
| Veste del ladro di tombe | 14 | +15 | La migliore |

### Amuleti
| Oggetto | Effetto |
| --- | --- |
| Moneta bucata | +5% Critico |
| Dente di lupo | +10 Vita |
| Anello di corda | Resistenza che torna più in fretta |
| Chiave d'osso | +2 Danno |
| Occhio di corvo | +10% Critico |

## Stregone (19 oggetti)

Mani: **bastone incantato** (tasto 1) e **libro** (tasto 2). Poca vita e poca armatura, ma più mana. Il libro dà mana e potenza agli incantesimi, come lo scudo per il Guerriero.

### Bastoni (casella Arma)
| Oggetto | Danno | Critico | Nota |
| --- | --- | --- | --- |
| Bastone del bottino | 8 | 5% | Di partenza, trovato nel bottino degli orchi |
| Bastone di quercia nera | 11 | 5% | |
| Bastone d'ossidiana | 14 | 8% | |
| Bastone del lago | 17 | 10% | Incantesimi più potenti |

### Libri (seconda mano)
| Oggetto | Mana | Nota |
| --- | --- | --- |
| Libro consumato | +10 | Di partenza |
| Libro dei sussurri | +20 | |
| Libro delle braci | +30 | Incantesimi di fuoco più forti |
| Libro del lago nero | +40 | Il migliore |

### Vesti (armatura)
| Oggetto | Armatura | Mana | Nota |
| --- | --- | --- | --- |
| Tunica stracciata | 2 | +0 | Di partenza |
| Veste da novizio | 3 | +5 | |
| Mantello di lana grezza | 4 | +5 | |
| Veste dell'evocatore | 5 | +15 | Evocazioni più lunghe |
| Manto di cenere | 6 | +20 | |
| Veste del druido caduto | 8 | +25 | La migliore |

### Amuleti
| Oggetto | Effetto |
| --- | --- |
| Osso inciso | +10 Mana |
| Cristallo opaco | Mana che si ricarica più in fretta |
| Cenere benedetta | +5 Vita |
| Sigillo del focolare | Incantesimi di fuoco più forti |
| Occhio del lago | Evocazioni più lunghe |
