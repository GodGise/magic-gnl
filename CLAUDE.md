# CLAUDE.md - istruzioni condivise per il progetto

Questo file è il punto di partenza di ogni sessione di Claude su questo progetto. Va aggiornato quando si prende una decisione.

La guida completa per il Claude di ogni membro del team (configurazione, flusso di lavoro, regole, problemi comuni) è in `Docs/guida-claude.md`. Se le due fonti non coincidono, vale questo file.

## Il gioco
Dark fantasy open world per PC (nome provvisorio "magic-GNL", da scegliere), con un'atmosfera alla Signore degli Anelli (nomi, luoghi e personaggi tutti inventati da noi). Gli orchi razziano di notte Villaggio Lago Nero in cerca di una reliquia antica; il protagonista perde la famiglia, si sveglia nella tana degli orchi, scappa e parte alla ricerca del figlio, fino alla scelta fra recuperare la reliquia o vendicarsi del druido che comanda gli orchi. Storia completa e note di design: `Docs/storia.md`. Single player e co-op. Uscita su Steam, Accesso Anticipato.

## Decisioni prese
- Combattimento d'azione ispirato a Elden Ring ma accessibile. Tre mosse fondamentali: **parata, schivata, attacco**. Niente d20.
- Molte scelte per il giocatore (build, approccio, storia).
- Tre personaggi giocabili, **tutti e tre già nella prima versione giocabile**: Guerriero (più vita, forza e potenza), Ladro (arco e pugnale), Stregone (attacchi a distanza ed evocazioni). La classe si sceglie all'inizio; l'arma si trova durante la fuga dalla prigione: Guerriero spada e scudo, Ladro arco e pugnale, Stregone bastone incantato e libro.
- Storia: il figlio del protagonista è morto (lo si scopre in una tana degli orchi). La reliquia dà forza e potenza: più vita e attacchi più forti.
- Il druido è il boss finale. Recuperare la reliquia è una missione secondaria facoltativa che potenzia il giocatore; chi la salta trova il druido con la reliquia: triplo della vita e attacchi più forti.
- Grafica retro in stile PlayStation 2: pochi poligoni, texture piccole (256-512 px), nebbia, atmosfera notturna e gotica.
- Budget quasi zero: solo strumenti gratuiti, asset con licenza libera (CC0) o creati da zero. Non copiare mai modelli, texture o suoni da altri giochi.
- Caratteri dei menu (tutti gratuiti, licenza SIL OFL, file di licenza accanto in `Assets/Resources/Caratteri/`): **Cinzel** e **IM Fell English** per le lingue latine, **Forum** e **Cormorant Garamond** per il russo, **ZCOOL XiaoWei** per il cinese. Lo stile dei menu è in `Assets/Scripts/Interfaccia/GraficaMenu.cs`; il menu di pausa (`MenuPausa.cs`) si crea da solo in ogni scena di gioco, non va messo nelle scene.
- Il mondo sono regioni grandi collegate, non una mappa infinita.
- **Il gioco inizia a Villaggio Lago Nero**, la notte della razzia degli orchi (prologo: il protagonista combatte e perde contro l'orco enorme). Poi si risveglia sotto terra nella gattabuia, la tana degli orchi: celle, sotterranei, grotte, uscita in superficie. Infine torna al villaggio, ormai in rovina. Il villaggio serve quindi in due versioni: di notte durante la razzia, e distrutto. Mappa del villaggio: `Docs/mappe/` (script che la genera). Lista dei modelli per Nazar: `Docs/lista-modelli-nazar.txt`.
- Ciclo giorno e notte: **45 minuti reali di luce** (dalle 6 alle 18 del gioco) e **50 minuti di notte** (dalle 18 alle 6), per l'atmosfera.
- Prospettiva: **terza persona**.
- Co-op: **da 1 a 3 giocatori** (si gioca anche da soli).
- **Lingue**: il gioco esce in 8 lingue, scelte dalle opzioni del menu: italiano, inglese, spagnolo, francese, tedesco, portoghese (Brasile), russo, cinese semplificato. Ogni testo che vede il giocatore passa da `Lingua.T("chiave")` (`Assets/Scripts/Interfaccia/Lingua.cs`), con le 8 traduzioni; niente frasi scritte direttamente nel codice.
- **Rete co-op: Netcode for GameObjects 2.13** (ufficiale Unity, gratis) con il trasporto **SteamNetworkingSockets** e **Steamworks.NET**. Un giocatore ospita, gli altri entrano con invito Steam; il traffico passa dal relay di Valve, niente server da pagare. Si sviluppa e prova prima in locale (trasporto di base di Unity), poi si collega Steam. Ricerca e fonti: `Docs/rete-coop.md`. Chi scrive codice di giocatore, nemici o combattimento deve tenerne conto: diventeranno oggetti di rete.
- Aggancio del bersaglio (lock-on): la camera resta puntata sul nemico; **con la rotellina del mouse si cambia bersaglio**.

## Motore
- **Unity 6.3 LTS, versione 6000.3.25f1 (C#)**, deciso. Tutti e tre devono usare esattamente questa versione. Il supporto della 6.3 LTS finisce a dicembre 2027: prima del lancio va pianificato il passaggio a un LTS più nuovo. Serializzazione degli asset su "Force Text", controllo versione "Visible Meta Files".

## Decisioni ancora aperte
- Nome del gioco.

## Ruoli e cartelle
- Giuseppe: team leader, design, coordinamento, rete co-op, Steam.
- Nazar: arte 3D in Blender (cartella `Art/`), con l'aiuto di Giuseppe.
- Lorenzo: **level design e bilanciamento**. Possiede la scena `Assets/Scenes/ZonaProva.unity`, regola i numeri del combattimento dall'Inspector e scrive codice con il suo Claude (gli script nuovi delle zone vanno in `Assets/Scripts/Livelli/`).
Ogni persona modifica solo la propria area; per toccare quella di un altro, chiedere prima.
- La zona di prova è di Lorenzo: nessuno usa più il menu "Crea zona di prova" (cancellerebbe il suo lavoro). Claude non modifica quella scena: consegna nemici e oggetti come **prefab** e Lorenzo li mette nella scena.
- Ogni sera chi ha lavorato carica il suo ramo e lo scrive nel gruppo; Giuseppe prova e unisce a `main`.

## Chi lavora su cosa (per non sovrapporsi)
- **Prima di iniziare** un lavoro, la persona lo annuncia nel canale Discord del team dedicato ai lavori in corso: cosa fa e quali file o aree tocca. Esempio: "Lorenzo: oggi porta della cripta, tocco ZonaProva e Assets/Scripts/Livelli/PortaCripta.cs".
- **Prima di annunciare**, si legge il canale: se qualcun altro sta già lavorando sugli stessi file o sulla stessa area, ci si accorda prima di iniziare.
- **Ogni Claude, prima di fare qualsiasi cosa in una sessione**: fa fare Pull di `main` e aggiorna il ramo di lavoro da `main` (GitHub Desktop: **Branch → Update from main**), poi **rilegge questo file da capo**, perché possono esserci regole e decisioni nuove.
- **Ogni Claude, all'inizio di una sessione di lavoro**: chiede alla persona su cosa lavora e quali file toccherà; le ricorda di leggere il canale e di annunciare; crea subito il ramo con un nome che descrive il lavoro (vedi "Nomi dei rami" qui sotto) e lo pubblica su GitHub (**Publish branch**). GitHub è collegato al canale, quindi la creazione del ramo arriva come notifica automatica.
- **Alla fine**: si carica il ramo, si apre la pull request e si scrive "finito" nel canale. I caricamenti e le pull request arrivano anche loro come notifica.
- Il Claude di Giuseppe non scrive su Discord: i suoi lavori si vedono dalle notifiche dei rami `giuseppec/...`.

## Nomi dei rami
- Il ramo porta il nome della persona e del lavoro, in minuscolo con trattini.
- Se il lavoro è fatto con l'aiuto di Claude, al nome si aggiunge una **c**: `giuseppec/menu-iniziale`, `lorenzoc/combattimento`, `nazarc/esportazione-fbx`.
- Se la persona lavora da sola, senza Claude, solo il nome: `nazar/lapide`, `giuseppe/pacchetto-audio`.
- Non si usano più rami `claude/...`: quelli vecchi sono già uniti a `main` e vanno cancellati da GitHub (pagina Branches).
- Compiti della settimana: documento "Compiti del team". Istruzioni dettagliate per il Claude di Lorenzo: `Docs/compiti-lorenzo.md`.

## Convenzioni
- Messaggi di commit in italiano, imperativi, con una riga di riepilogo.
- Nei commit e nelle pull request **niente righe di firma** ("Co-Authored-By", link alla sessione, "Generated with ..."). L'unico segno del lavoro fatto con Claude è la **c** nel nome del ramo.
- Modelli 3D: nomi in minuscolo con trattini, scala 1 unità = 1 metro, pochi poligoni.
- Nessun token, chiave o password nel repository.

## Come lavora Claude su questo progetto
- Claude si occupa del **codice C#** (gameplay, combattimento, rete co-op). Blender e i modelli sono di Giuseppe e Nazar.
- Claude carica il codice su un **ramo a parte** con il nome della persona più la c (per Giuseppe: `giuseppec/nome-funzione`), mai direttamente su `main`.
- Dopo ogni caricamento Claude scrive cosa controllare. Chi lo prova in Unity dice se va bene; solo allora il ramo si unisce a `main`.
- Le prove in Unity e la ricerca dei problemi le fa Giuseppe (o chi lavora), per non consumare crediti: Claude non prende il controllo del PC di Giuseppe per fare le prove, salvo richiesta esplicita. Dopo ogni caricamento Claude scrive esattamente cosa provare.
- Ogni script C# ha un commento iniziale che dice a cosa serve e come montarlo su un oggetto della scena.
- Claude crea gli script senza file `.meta` (li genera Unity). La prima volta che un ramo di Claude si apre in Unity, i `.meta` nuovi vanno salvati e caricati sullo stesso ramo prima dell'unione, altrimenti ogni PC ne genera di diversi e i collegamenti nelle scene si rompono.

## Cartelle
- `Assets/Art/` modelli, texture, animazioni (Nazar e Giuseppe)
- `Assets/Scripts/Gameplay/` personaggio, combattimento, nemici
- `Assets/Scripts/Rete/` co-op e integrazione Steam
- `Assets/Scripts/Ambiente/` ciclo giorno e notte, torce, effetto retro PS2
- `Assets/Scripts/Livelli/` script degli elementi delle zone (porte, leve, trappole, checkpoint)
- `Assets/Scripts/Interfaccia/` menu iniziale, scelta della classe, opzioni
- `Assets/Segnaposto/` materiali e forme provvisorie della zona di prova, da sostituire con l'arte vera
- Menu dell'editor **magic-gnl**: "Crea scena di prova" (combattimento) e "Crea zona di prova (villaggio in rovina)" (prima zona giocabile)
- `Docs/` documenti di design
- Il progetto Unity sta nella radice della repository. Non committare `Library/`, `Temp/`, `Logs/`.
