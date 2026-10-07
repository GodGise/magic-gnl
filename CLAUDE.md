# CLAUDE.md - istruzioni condivise per il progetto

Punto di partenza di ogni sessione di Claude. Va aggiornato quando si prende una decisione. Tenerlo **corto**: i dettagli stanno nei documenti di `Docs/`, qui solo una riga e il rimando.

Comportamento, flusso con Git, regole di Unity e convenzioni: `Docs/guida-claude.md`. Se le due fonti non coincidono, vale questo file.

## Il gioco
Dark fantasy open world per PC (nome provvisorio "magic-GNL", da scegliere), atmosfera alla Signore degli Anelli (nomi, luoghi e personaggi inventati da noi). Gli orchi razziano di notte Villaggio Lago Nero in cerca di una reliquia; il protagonista perde la famiglia, scappa dalla tana degli orchi e cerca il figlio, fino alla scelta fra recuperare la reliquia o vendicarsi del druido. Single player e co-op, uscita su Steam in Accesso Anticipato. Storia e note di design: `Docs/storia.md`.

## Decisioni prese
- Combattimento d'azione ispirato a Elden Ring ma accessibile. Tre mosse: **parata, schivata, attacco**. Niente d20. Molte scelte per il giocatore (build, approccio, storia).
- Tre classi, **tutte nella prima versione giocabile**: Guerriero (più vita e forza), Ladro (arco e pugnale), Stregone (attacchi a distanza ed evocazioni). Si sceglie all'inizio; l'arma si trova durante la fuga dalla prigione. Mana, oggetti e interfaccia: `Docs/mana.md`, `Docs/oggetti-ladro-stregone.md`, `Docs/interfaccia.md`.
- Il figlio è morto (si scopre in una tana). La reliquia dà forza e potenza. Il druido è il boss finale; la reliquia è una missione facoltativa: chi la salta trova il druido con triplo della vita (dettagli in `Docs/storia.md`).
- Grafica retro PlayStation 2: pochi poligoni, texture 256-512 px, nebbia, atmosfera notturna e gotica. Terza persona.
- Budget quasi zero: solo strumenti gratuiti, asset CC0 o creati da zero. Non copiare mai modelli, texture o suoni da altri giochi.
- Il mondo sono regioni grandi collegate, non una mappa infinita.
- **Il gioco inizia a Villaggio Lago Nero**, la notte della razzia (il protagonista perde contro l'orco enorme), poi gattabuia sotto terra, poi ritorno al villaggio in rovina. Il villaggio serve in due versioni: notte della razzia e distrutto. Mappa: `Docs/mappe/`. Modelli per Nazar: `Docs/lista-modelli-nazar.txt`.
- Ciclo giorno e notte: **45 minuti reali di luce** (6-18 del gioco) e **50 di notte** (18-6).
- Co-op **da 1 a 3 giocatori**. Aggancio del bersaglio (lock-on): la rotellina del mouse cambia bersaglio.
- **Lingue**: 8 (italiano, inglese, spagnolo, francese, tedesco, portoghese del Brasile, russo, cinese semplificato), scelte dal menu. Ogni testo visibile passa da `Lingua.T("chiave")` (`Assets/Scripts/Interfaccia/Lingua.cs`) con le 8 traduzioni; niente frasi scritte nel codice.
- **Rete co-op**: Netcode for GameObjects 2.13 + trasporto SteamNetworkingSockets + Steamworks.NET (gratis, un giocatore ospita, nessun server). Si prova prima in locale. Chi scrive codice di giocatore, nemici o combattimento deve tenerne conto: diventeranno oggetti di rete. Dettagli e fonti: `Docs/rete-coop.md`.

## Motore
**Unity 6.3 LTS, versione 6000.3.25f1 (C#)**, uguale per tutti. Supporto fino a dicembre 2027: prima del lancio va pianificato il passaggio a un LTS più nuovo. Serializzazione asset "Force Text", controllo versione "Visible Meta Files".

## Decisioni ancora aperte
- Nome del gioco. Nome del protagonista (o se lo sceglie il giocatore). Se serve una terza persona in aiuto (profilo più utile: un programmatore, poi suoni e musica).

## Ruoli e cartelle
- Giuseppe: team leader, design, coordinamento, rete co-op, Steam.
- Nazar: arte 3D in Blender (`Assets/Art/`), con l'aiuto di Giuseppe.
- Lorenzo: **level design e bilanciamento**. Possiede `Assets/Scenes/ZonaProva.unity`, regola i numeri del combattimento dall'Inspector e scrive codice con il suo Claude (script delle zone in `Assets/Scripts/Livelli/`).
- Ogni persona modifica solo la propria area; per toccare quella di un altro, chiedere prima.
- La zona di prova è di Lorenzo: nessuno usa il menu "Crea zona di prova" (cancellerebbe il suo lavoro). Claude non modifica quella scena: consegna nemici e oggetti come **prefab** e Lorenzo li mette nella scena.
- Ogni sera chi ha lavorato carica il suo ramo e lo scrive nel gruppo; Giuseppe prova e unisce a `main`.

## Chi lavora su cosa (per non sovrapporsi)
- **Prima di iniziare** un lavoro, la persona lo annuncia nel canale Discord dei lavori in corso: cosa fa e quali file o aree tocca (esempio: "Lorenzo: oggi porta della cripta, tocco ZonaProva e Assets/Scripts/Livelli/PortaCripta.cs"). **Prima di annunciare** legge il canale: se altri lavorano sugli stessi file, ci si accorda.
- **Ogni Claude, prima di fare qualsiasi cosa in una sessione**: fa fare Pull di `main` e aggiorna il ramo di lavoro (GitHub Desktop: **Branch → Update from main**), poi **rilegge questo file da capo**, perché possono esserci regole nuove.
- **Ogni Claude, a inizio sessione (dopo il Pull di `main`)**: legge `Docs/da-approvare.md` (documenti e richieste fra il Claude di Giuseppe e quello di Lorenzo) e dice alla persona cosa c'è di nuovo. Le risposte si scrivono in quel file.
- **Ogni Claude, a inizio lavoro**: chiede su cosa si lavora e quali file si toccano, ricorda di leggere il canale e di annunciare, crea subito il ramo (vedi sotto) e lo pubblica (**Publish branch**): GitHub è collegato al canale, la notifica arriva da sola.
- **Alla fine**: si carica il ramo, si apre la pull request e si scrive "finito" nel canale.
- Il Claude di Giuseppe non scrive su Discord: i suoi lavori si vedono dalle notifiche dei rami `giuseppec/...`.

## Nomi dei rami
- Nome della persona e del lavoro, minuscolo con trattini. Con l'aiuto di Claude si aggiunge una **c**: `giuseppec/menu-iniziale`, `lorenzoc/combattimento`, `nazarc/esportazione-fbx`. Senza Claude solo il nome: `nazar/lapide`, `giuseppe/pacchetto-audio`.
- Non si usano più rami `claude/...`: quelli vecchi sono già uniti e vanno cancellati da GitHub (pagina Branches).
- Compiti della settimana: documento "Compiti del team". Istruzioni per il Claude di Lorenzo: `Docs/compiti-lorenzo.md`.

## Convenzioni
- Commit in italiano, imperativi, una riga di riepilogo. Nei commit e nelle pull request **niente righe di firma** ("Co-Authored-By", link alla sessione, "Generated with ..."): l'unico segno del lavoro con Claude è la **c** nel nome del ramo.
- Modelli 3D: nomi minuscoli con trattini, 1 unità = 1 metro, pochi poligoni. Nessun token, chiave o password nel repository.

## Come lavora Claude su questo progetto
- Claude scrive il **codice C#** (gameplay, combattimento, rete co-op). Blender e modelli sono di Giuseppe e Nazar.
- Carica su un **ramo a parte** (per Giuseppe `giuseppec/nome-funzione`), mai su `main`. Dopo ogni caricamento scrive esattamente cosa provare; solo quando chi prova in Unity dice che va bene, il ramo si unisce.
- Le prove in Unity le fa Giuseppe (o chi lavora), per non consumare crediti: Claude non prende il controllo del PC per provare, salvo richiesta esplicita.
- Ogni script C# ha un commento iniziale: a cosa serve e come montarlo su un oggetto.
- Claude crea gli script senza file `.meta` (li genera Unity). La prima volta che un ramo si apre in Unity, i `.meta` nuovi vanno salvati e caricati sullo stesso ramo prima dell'unione, altrimenti ogni PC ne genera di diversi e i collegamenti nelle scene si rompono.

## Cartelle
- `Assets/Art/` modelli, texture, animazioni · `Assets/Scripts/Gameplay/` personaggio, combattimento, nemici · `Assets/Scripts/Rete/` co-op e Steam · `Assets/Scripts/Ambiente/` giorno e notte, torce, effetto PS2 · `Assets/Scripts/Livelli/` porte, leve, trappole, checkpoint · `Assets/Scripts/Interfaccia/` menu, classe, opzioni · `Assets/Segnaposto/` forme provvisorie da sostituire con l'arte vera.
- Menu dell'editor **magic-gnl**: "Crea scena di prova" (combattimento) e "Crea zona di prova (villaggio in rovina)".
- `Docs/` documenti di design; `Docs/archivio/` lavori finiti, non si leggono a inizio sessione. Il progetto Unity sta nella radice. Non committare `Library/`, `Temp/`, `Logs/`.
