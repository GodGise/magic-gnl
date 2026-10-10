# CLAUDE.md - istruzioni condivise per il progetto

Punto di partenza di ogni sessione di Claude. Va aggiornato quando si prende una decisione. Tenerlo **corto**: i dettagli stanno nei documenti di `Docs/`, qui solo una riga e il rimando.

Comportamento, flusso con Git, regole di Unity e convenzioni: `Docs/guida-claude.md`. Se le due fonti non coincidono, vale questo file.

## Il gioco
Dark fantasy open world per PC, **Sun of the Black Lake** (nome deciso il 10 ottobre 2026; "magic-GNL" resta il nome della repository), atmosfera alla Signore degli Anelli (nomi, luoghi e personaggi inventati da noi). Gli orchi razziano di notte Villaggio Lago Nero in cerca di una reliquia; il protagonista perde la famiglia, scappa dalla tana degli orchi e cerca il figlio, fino alla scelta fra recuperare la reliquia o vendicarsi del druido. Single player e co-op, uscita su Steam in Accesso Anticipato. Storia e note di design: `Docs/storia.md`.

## Decisioni prese
- Combattimento d'azione ispirato a Elden Ring ma accessibile. Tre mosse: **parata, schivata, attacco**. Niente d20. Molte scelte per il giocatore (build, approccio, storia).
- Tre classi, **tutte nella prima versione giocabile**: Guerriero (più vita e forza), Ladro (arco e pugnale), Stregone (attacchi a distanza ed evocazioni). Si sceglie all'inizio; l'arma si trova durante la fuga dalla prigione. Mana, oggetti e interfaccia: `Docs/mana.md`, `Docs/oggetti-guerriero.md`, `Docs/oggetti-ladro.md`, `Docs/incantesimi-stregone.md` (Stregone), `Docs/interfaccia.md`.
- Il figlio è morto (si scopre in una tana). La reliquia dà forza e potenza. Il druido è il boss finale; la reliquia è una missione facoltativa: chi la salta trova il druido con triplo della vita (dettagli in `Docs/storia.md`).
- Grafica retro PlayStation 2: pochi poligoni, texture 256-512 px, nebbia, atmosfera notturna e gotica. Terza persona.
- Budget quasi zero: solo strumenti gratuiti, asset CC0 o creati da zero. Non copiare mai modelli, texture o suoni da altri giochi.
- Caratteri dei menu (gratuiti, licenza SIL OFL, con la licenza accanto in `Assets/Resources/Caratteri/`): **Cinzel** e **IM Fell English** per le lingue latine, **Forum** e **Cormorant Garamond** per il russo, **ZCOOL XiaoWei** per il cinese. Stile dei menu in `Assets/Scripts/Interfaccia/GraficaMenu.cs`; il menu di pausa (`MenuPausa.cs`) si crea da solo in ogni scena di gioco, non va messo nelle scene.
- Interfaccia in partita (disegno approvato: `Docs/interfaccia.md`): barre di vita, resistenza e mana (`HudGioco.cs`) e inventario con Tab (`InventarioGioco.cs`) si creano da soli come il menu di pausa, non vanno messi nelle scene. F1 mostra il pannello di prova. Gli oggetti stanno nello `Zaino` del giocatore; nel mondo si raccolgono con `OggettoRaccoglibile` (E). Per provare l'inventario: in Play, menu "magic-gnl > Prova: metti tutti gli oggetti nello zaino". Ogni nuovo comando con E deve ignorare il tasto se `InventarioGioco.Aperto` o `MenuPausa.InPausa`.
- **Oggetti fra classi** (Lorenzo, 9 ottobre): armi, scudi, armature, archi e libri solo della propria classe; gli **amuleti** sono in comune, tranne Ultimo respiro (solo Ladro). Lo controlla `InventarioGioco.cs`. Bilanciamento e regole (esecuzione furtiva, allerta, moltiplicatori): `Docs/oggetti-guerriero.md`, `Docs/oggetti-ladro.md`, `Docs/incantesimi-stregone.md`.
- **Tasti**: il giocatore li cambia in Opzioni > **Comandi** (tastiera disegnata, `MenuComandi.cs`). Elenco unico in `Comandi.cs`: ogni nuova InputAction di tastiera o mouse va collegata con `Comandi.Collega(azione, Azione.X, this)` (le azioni nuove si aggiungono all'enum `Azione`). Tasti fissi: Esc pausa, 1-6 armi e incantesimi.
- **Opzioni** (menu iniziale e di pausa, stesse voci in `Impostazioni.AggiungiVoci`): pagina principale con Lingua e tre sezioni, **Audio** (volume generale, musica, effetti), **Video** (schermo intero, effetto retro, luminosità: `LuminositaSchermo.cs`) e **Controlli** (sensibilità camera, asse verticale invertito, Comandi). Ogni nuova opzione va in una sezione, con testi in 8 lingue. "Esci" chiede sempre conferma.
- **Salvataggio**: automatico ai falò e ai checkpoint, niente salvataggio a mano (il salvataggio vero è ancora da fare). **5 slot** di personaggi già nel menu (`Salvataggio.cs`, file `slotN.json` in persistentDataPath/Salvataggi): Nuova partita → slot vuoto → nome (lettere latine, max 16) → classe; slot occupato → Gioca o Cancella (con conferma). Continua è attivo solo se esiste un personaggio. In co-op l'host salverà il mondo e ogni giocatore il proprio personaggio (slot sul proprio PC); il nome compare sopra la testa (`GiocatoreRete.nome`). `DatiSlot` ha `versione` per aggiungere campi (oggetti, posizione, aspetto) senza rompere i vecchi salvataggi.
- Inventario: oggetti si trascinano sulle caselle (quello vecchio torna nello zaino); clic su una casella e poi su un oggetto lo sostituisce.
- Il mondo sono regioni grandi collegate, non una mappa infinita.
- **Il gioco inizia a Villaggio Lago Nero**, la notte della razzia (il protagonista perde contro l'orco enorme), poi gattabuia sotto terra, poi ritorno al villaggio in rovina. Il villaggio serve in due versioni: notte della razzia e distrutto. Mappa: `Docs/mappe/`. Modelli per Nazar: `Docs/lista-modelli-nazar.txt`.
- Ciclo giorno e notte: **45 minuti reali di luce** (6-18 del gioco) e **50 di notte** (18-6).
- Co-op **da 1 a 3 giocatori**. **Classi libere**: in co-op si possono avere doppioni (anche 3 Stregoni o 3 Guerrieri). **La difficoltà cresce con il numero di giocatori**: in 2 il gioco è molto più difficile che da soli, in 3 ancora di più (regole in `Docs/rete-coop.md`). Contro le combinazioni troppo forti (Lorenzo, 10 ottobre): controlli -30% per ogni Stregone, i nemici passano a chi fa più danno, in co-op si va **a terra** e un alleato rialza (contro un boss una volta sola, poi spettatori; tutti a terra = boss da capo). Dettagli in `Docs/rete-coop.md`.
- Aggancio del bersaglio (lock-on): clic della rotellina per agganciare e sganciare, uno scatto del mouse verso un nemico cambia bersaglio; con lo Stregone la rotellina sceglie l'incantesimo (Lorenzo, 10 ottobre). Oggetti addosso e a terra con forme provvisorie: `FormeOggetti.cs`.
- **Lingue**: 8 (italiano, inglese, spagnolo, francese, tedesco, portoghese del Brasile, russo, cinese semplificato), scelte dal menu. Ogni testo visibile passa da `Lingua.T("chiave")` (`Assets/Scripts/Interfaccia/Lingua.cs`) con le 8 traduzioni; niente frasi scritte nel codice.
- **Rete co-op**: Netcode for GameObjects 2.13 + trasporto SteamNetworkingSockets + Steamworks.NET (gratis, un giocatore ospita, nessun server). Per ora senza Steam: menu "Multigiocatore", ci si collega con l'indirizzo IP. I nemici pensano solo sull'host; i nemici cercano i giocatori in `ObiettiviNemici`, il danno ai nemici passa da `Bersaglio.RiceviColpo`. Regole e limiti: `Docs/rete-coop.md`.

## Motore
**Unity 6.3 LTS, versione 6000.3.25f1 (C#)**, uguale per tutti. Supporto fino a dicembre 2027: prima del lancio va pianificato il passaggio a un LTS più nuovo. Serializzazione asset "Force Text", controllo versione "Visible Meta Files".

## Decisioni ancora aperte
- Nome del protagonista (o se lo sceglie il giocatore). Se serve una terza persona in aiuto (profilo più utile: un programmatore, poi suoni e musica).

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
- **File `.meta`**: Claude li crea insieme allo script, nello stesso commit, così nessuno deve aspettare Unity. Formato minimo, due righe e nessun a capo finale: `fileFormatVersion: 2` e `guid: <32 cifre esadecimali casuali>` (per esempio con `uuid.uuid4().hex`). Un `.meta` si crea una volta e non si cambia più. Anche le cartelle nuove hanno il loro `.meta` (con `folderAsset: yes`). Unity apre il file senza problemi; se chi lo apre ha già un `.meta` diverso per lo stesso file, prima si decide quale tenere (vale quello già su `main`).

## Cartelle
- `Assets/Art/` modelli, texture, animazioni · `Assets/Scripts/Gameplay/` personaggio, combattimento, nemici · `Assets/Scripts/Rete/` co-op e Steam · `Assets/Scripts/Ambiente/` giorno e notte, torce, effetto PS2 · `Assets/Scripts/Livelli/` porte, leve, trappole, checkpoint · `Assets/Scripts/Interfaccia/` menu, classe, opzioni, pausa, barre in partita, inventario · `Assets/Segnaposto/` forme provvisorie da sostituire con l'arte vera.
- Menu dell'editor **magic-gnl**: "Crea scena di prova" (combattimento) e "Crea zona di prova (villaggio in rovina)".
- `Docs/` documenti di design; `Docs/archivio/` lavori finiti, non si leggono a inizio sessione. Il progetto Unity sta nella radice. Non committare `Library/`, `Temp/`, `Logs/`.
