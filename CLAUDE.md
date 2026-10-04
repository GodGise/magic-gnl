# CLAUDE.md - istruzioni condivise per il progetto

Questo file è il punto di partenza di ogni sessione di Claude su questo progetto. Va aggiornato quando si prende una decisione.

La guida completa per il Claude di ogni membro del team (configurazione, flusso di lavoro, regole, problemi comuni) è in `Docs/guida-claude.md`. Se le due fonti non coincidono, vale questo file.

## Il gioco
Dark fantasy open world per PC (nome provvisorio "magic-GNL", da scegliere). Il protagonista deve riprendersi l'anima rubata. Single player e co-op. Uscita su Steam, Accesso Anticipato.

## Decisioni prese
- Combattimento d'azione ispirato a Elden Ring ma accessibile. Tre mosse fondamentali: **parata, schivata, attacco**. Niente d20.
- Molte scelte per il giocatore (build, approccio, storia).
- Tre personaggi giocabili: Guerriero, Ladro, Stregone. Prima versione giocabile (vertical slice): solo il Guerriero (proposta).
- Grafica retro in stile PlayStation 2: pochi poligoni, texture piccole (256-512 px), nebbia, atmosfera notturna e gotica.
- Budget quasi zero: solo strumenti gratuiti, asset con licenza libera (CC0) o creati da zero. Non copiare mai modelli, texture o suoni da altri giochi.
- Il mondo sono regioni grandi collegate, non una mappa infinita.
- Prospettiva: **terza persona**.
- Co-op: **da 1 a 3 giocatori** (si gioca anche da soli).
- Aggancio del bersaglio (lock-on): la camera resta puntata sul nemico; **con la rotellina del mouse si cambia bersaglio**.

## Motore
- **Unity 6.3 LTS, versione 6000.3.25f1 (C#)**, deciso. Tutti e tre devono usare esattamente questa versione. Il supporto della 6.3 LTS finisce a dicembre 2027: prima del lancio va pianificato il passaggio a un LTS più nuovo. Serializzazione degli asset su "Force Text", controllo versione "Visible Meta Files".

## Decisioni ancora aperte
- Nome del gioco, ruolo di Lorenzo.

## Ruoli e cartelle
- Giuseppe: team leader, design, coordinamento, rete co-op, Steam.
- Nazar: arte 3D in Blender (cartella `Art/`), con l'aiuto di Giuseppe.
- Lorenzo: da assegnare.
Ogni persona modifica solo la propria area; per toccare quella di un altro, chiedere prima.

## Convenzioni
- Messaggi di commit in italiano, imperativi, con una riga di riepilogo.
- Modelli 3D: nomi in minuscolo con trattini, scala 1 unità = 1 metro, pochi poligoni.
- Nessun token, chiave o password nel repository.

## Come lavora Claude su questo progetto
- Claude si occupa del **codice C#** (gameplay, combattimento, rete co-op). Blender e i modelli sono di Giuseppe e Nazar.
- Claude carica il codice su un **ramo a parte** (`claude/nome-funzione`), mai direttamente su `main`.
- Dopo ogni caricamento Claude scrive cosa controllare. Chi lo prova in Unity dice se va bene; solo allora il ramo si unisce a `main`.
- Claude non può aprire Unity né vedere il gioco in esecuzione: i test nel motore si fanno sul PC di chi lavora.
- Ogni script C# ha un commento iniziale che dice a cosa serve e come montarlo su un oggetto della scena.

## Cartelle
- `Assets/Art/` modelli, texture, animazioni (Nazar e Giuseppe)
- `Assets/Scripts/Gameplay/` personaggio, combattimento, nemici
- `Assets/Scripts/Rete/` co-op e integrazione Steam
- `Docs/` documenti di design
- Il progetto Unity sta nella radice della repository. Non committare `Library/`, `Temp/`, `Logs/`.
