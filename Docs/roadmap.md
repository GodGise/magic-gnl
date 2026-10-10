# Roadmap, rischio più grande e ritmo del team

Le date sono una stima per tre persone part-time. I compiti della settimana sono nel documento "Compiti del team" e in `Docs/compiti-lorenzo.md`; la vecchia tabella "Prossimi 3 giorni (5-7 ottobre)" è passata ed è rimasta solo nella storia di Git.

### Dove siamo (aggiornare alla fine di ogni lavoro importante)

| Cosa | Stato | Note |
| --- | --- | --- |
| Combattimento (parata, schivata, attacco, aggancio, esecuzione furtiva) | Fatto | Numeri regolati da Lorenzo |
| Oggetti di Guerriero e Ladro | Fatto | Stregone rifatto con gli incantesimi: `Docs/incantesimi-stregone.md` (approvato il 9 ottobre) |
| Interfaccia in partita (barre, inventario, pausa) | Fatto | Casella del Ladro e abilità Q aggiunte l'8 ottobre (ramo `giuseppec/interfaccia-completa`); libro dello Stregone con gli oggetti di Lorenzo |
| Co-op in locale, senza Steam (menu Multigiocatore, nemici e combattimento in rete) | Fatto e provato in due (8 ottobre) | PR #17 |
| Porte, leve, bauli, chiavi, muri, oggetti condivisi in co-op | Fatto, da provare | Ramo `giuseppec/livelli-coop` (sopra `interfaccia-completa`); checkpoint personali |
| Steam (App ID, inviti) | Dopo | Si paga quando il gioco è all'80-90% |
| Mana e oggetti dello Stregone nel codice | Da fare | Dopo l'approvazione di Lorenzo |
| Icone degli oggetti | Da fare | Nazar |
| Nome del gioco e del protagonista | Da decidere | |

### La roadmap

Quattro fasi, con un **gate** tra una e l'altra: si passa alla fase dopo solo se il gate è superato. Le date sono una stima per tre persone part-time, da rivedere dopo il gate 1.

| Fase | Periodo stimato | Contenuto | Gate per uscire |
| --- | --- | --- | --- |
| 1. Fondamenta | Ott - Dic 2026 | Motore e repository, personaggio in scena, co-op di prova | **Gate 1**: due giocatori nella stessa scena via Steam, senza crash |
| 2. Vertical slice | Gen - Apr 2027 | 1 regione, 1 dungeon, 1 boss, combattimento con le 3 mosse, grafica stile PS2 | **Gate 2**: la slice giocata in co-op da tutti e 3 |
| 3. Steam e demo | Mag - Ott 2027 | Pagina "Coming soon", trailer, capsule, demo, evento Steam (es. Next Fest) | **Gate 3**: demo approvata da Steam |
| 4. Contenuti | Da fine 2027 | Altre regioni e boss, storia completa | Accesso Anticipato, realisticamente nel 2028 |

Da ricordare: il supporto di Unity 6.3 LTS finisce a dicembre 2027, prima dell'Accesso Anticipato. Il passaggio a una versione LTS più nuova va pianificato, ma non adesso.

### Il rischio più grande

Il rischio più grande del progetto è la dimensione: un open world co-op è tra i generi più difficili da finire per un gruppo piccolo. Quando la persona propone nuove idee, aiutala a chiedersi quale pilastro servono e se servono alla fase attuale. Le idee buone ma premature vanno in una lista "dopo il lancio", non si buttano.

## Il team: chi decide, dove si comunica, ritmo

(dalla vecchia sezione 3; i ruoli e le aree di ognuno sono in `CLAUDE.md`)

### Chi decide cosa

- **Giuseppe** ha l'ultima parola su design, priorità e unione dei rami in `main`.
- **Nazar** decide sullo stile dei modelli, dentro le regole tecniche concordate (pochi poligoni, texture piccole, scala 1 unità = 1 metro).
- Le decisioni grandi (nome, prospettiva, co-op a 2 o 4, nuove persone nel team) si prendono **insieme** in riunione e si scrivono nel documento di progetto.

### Dove si lavora e si comunica

| Cosa | Dove |
| --- | --- |
| Piano di progetto, visione, roadmap, decisioni | Documento "Piano di progetto" condiviso da Giuseppe (con le schede Roadmap operativa e Prossimi 3 giorni) |
| Compiti dei prossimi giorni | Documento "Compiti dei prossimi 3 giorni" |
| Codice, modelli, scene | Repository GitHub `GodGise/magic-gnl` |
| Regole per Claude sul progetto | File `CLAUDE.md` nella repository |
| Chat veloce e riunioni | Il gruppo del team su Discord (canale dei lavori in corso e canale degli aggiornamenti) |

### Ritmo

- Una riunione di 30 minuti a settimana per allinearsi.
- Una build giocabile al mese, anche brutta: se non si gioca, non c'è progresso.
- Ognuno dichiara quante ore a settimana può dare: il piano si basa su ore realistiche, non sull'entusiasmo.
