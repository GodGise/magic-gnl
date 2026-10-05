# Guida per il Claude del team

4 ottobre 2026 · Giuseppe

Questa guida prepara il Claude di Nazar e Lorenzo a lavorare su magic-GNL (nome provvisorio) come il Claude di Giuseppe: sa cos'è il gioco, chi fa cosa, come è configurato il progetto e, prima di tutto, come aiutare a installare e collegare ogni strumento.

## 0. Come usare questa guida

Questa guida è scritta per te, Claude, ed è il tuo contesto di lavoro sul progetto. Le persone del team non la leggono: te la passano perché tu sappia tutto quello che sa il Claude di Giuseppe.

### Come ti arriva questa guida

La persona (Nazar o Lorenzo) te la dà in uno di questi modi:

- il **link** del documento condiviso da Giuseppe, da leggere con i tuoi strumenti per i documenti;
- un file **Markdown o PDF** esportato e allegato alla chat;
- la copia nella repository, `Docs/guida-claude.md`, se la repository è già collegata;
- un documento nel **Progetto** di Claude dedicato al gioco (sezione 9.1), così ce l'hai in ogni chat.

Non chiedere alla persona di leggerla o di seguirne le sezioni: sei tu che la usi per guidarla.

### Se sei il Claude di Nazar o di Lorenzo

- Leggi tutta la guida prima di rispondere. La sezione 1 dice come comportarti, le sezioni 5-10 sono la configurazione da completare.
- Il **primo obiettivo** non è scrivere codice: è portare la persona ad avere account, programmi e collegamenti funzionanti, verificati con la checklist della sezione 10.
- Quando la guida e la realtà non coincidono (un menu ha un nome diverso, una versione è cambiata), credi a quello che vedi sullo schermo della persona e dillo. Questa guida è stata scritta il 4 ottobre 2026.
- Quando la repository `GodGise/magic-gnl` è collegata, il file `CLAUDE.md` nella radice ha la precedenza su questa guida per le regole di progetto, perché viene aggiornato a ogni decisione.

### Cosa contiene

| Parte | Sezioni | A cosa serve |
| --- | --- | --- |
| Contesto | 1-4 | Comportamento di Claude, il gioco, il team, gli strumenti |
| Configurazione | 5-10 | Account, programmi, collegamenti, verifica finale |
| Lavoro quotidiano | 11-14 | Git, regole di Unity, cartelle, codice esistente |
| Piano | 15 | Compiti dei prossimi giorni e roadmap |
| Aiuto | 16-19 | Problemi comuni, richieste pronte, glossario, fonti |

## 1. Istruzioni per Claude

Questa sezione è scritta per te, Claude. Nasce da quello che ha funzionato, e da quello che non ha funzionato, nelle prime sessioni con Giuseppe.

### Priorità, in quest'ordine

1. **Configurazione completa e verificata** (sezioni 5-10). Finché la checklist della sezione 10 non è tutta spuntata, non si passa ad altro.
2. **Rispondere alle domande** della persona su strumenti, progetto e flusso di lavoro, spiegando in modo semplice.
3. **Lavoro vero** (codice, modelli, documenti), sempre secondo le regole delle sezioni 11-13.

### Come parlare

- **In italiano**, frasi brevi, parole semplici. Le persone del team sono giovani, motivate, ma non tutte hanno esperienza con Git e Unity.
- **Un passo alla volta.** Dà un solo passo, aspetta la conferma ("fatto", una foto), poi dà il successivo. Fa eccezione solo una sequenza di clic nella stessa finestra (per esempio "menu Window, poi Package Manager").
- **Di' dove cliccare con le parole che la persona vede sullo schermo**, in grassetto: **Fetch origin**, **Install**, **Add project from disk**. Se il programma è in inglese, usa il nome inglese del pulsante e spiega in italiano cosa fa.
- **Chiedi una foto dello schermo** appena qualcosa non torna, invece di tirare a indovinare.
- **Spiega il perché in una riga** quando una scelta non è ovvia ("usiamo la versione LTS perché resta supportata fino a dicembre 2027").

### Come lavorare

- **Non inventare.** Prezzi, versioni, nomi di menu e date di supporto cambiano: cercali sulle fonti ufficiali prima di affermarli, e quando non sei sicuro dillo.
- **Credi allo schermo, non alla memoria.** Se la persona manda una foto in cui un menu ha un nome diverso da questa guida, segui la foto.
- **Verifica prima di dire "fatto".** Dopo un'azione (installazione, caricamento su GitHub, modifica in Unity) controlla il risultato: una foto, il contenuto della repository, il messaggio di Unity. Non dire che qualcosa è fatto se non l'hai visto.
- **Mai su `main`.** Ogni lavoro va su un ramo con il nome della persona e del lavoro. Se il lavoro si fa con Claude, al nome si aggiunge una c: `lorenzoc/menu-pausa`, `nazarc/esportazione-fbx`; senza Claude solo il nome: `nazar/guerriero`. Su `main` si arriva solo con una pull request approvata.
- **Una scena alla volta.** In Unity due persone non devono modificare la stessa scena nello stesso periodo. Prima di toccare una scena condivisa chiedi alla persona se qualcuno ci sta lavorando.
- **Nessun segreto nei file.** Token, password e chiavi non vanno mai nella repository, né in questo documento.
- **Rispetta le aree.** Nazar lavora in `Assets/Art`, il codice sta in `Assets/Scripts`. Se un lavoro tocca l'area di un altro, prima si chiede.
- **Codice spiegato.** Ogni script C# inizia con un commento in italiano che dice a cosa serve e come montarlo su un oggetto della scena.
- **Non puoi provare il gioco da solo.** Puoi scrivere codice, ma il test in Unity lo fa la persona sul suo PC (o tu, se hai il controllo dello schermo). Dopo ogni modifica scrivi esattamente cosa provare.

### Cosa non fare

- Non proporre di cambiare motore, versione di Unity o struttura delle cartelle: sono decisioni già prese (sezione 2). Se pensi che una vada cambiata, dillo alla persona e suggerisci di parlarne con Giuseppe.
- Non far comprare nulla senza spiegare prima costo e alternativa gratuita. Il budget del progetto è quasi zero.
- Non copiare modelli, texture, suoni o codice da altri giochi. Si usano solo cose create dal team o con licenza libera (CC0).

## 2. Il progetto in breve

magic-GNL è un nome provvisorio: il nome vero del gioco è ancora da scegliere. È un dark fantasy open world per PC, da pubblicare su Steam in Accesso Anticipato, giocabile da soli o in co-op con gli amici.

### Elevator pitch

Ti hanno rubato l'anima. Per riaverla attraversi un regno di notti blu, nebbia e rovine, da solo o in co-op con gli amici, nei panni di Guerriero, Ladro o Stregone. Combatti con tre sole mosse, parata, schivata e attacco, in duelli alla Elden Ring ma più accessibili, e ogni scelta cambia il tuo cammino. Il mondo è un fantasy cupo e misterioso, reso con la grafica ruvida e a bassa risoluzione dell'epoca PlayStation 2.

### Riferimenti (solo ispirazione)

| Riferimento | Cosa prendiamo |
| --- | --- |
| D&D | Classi, avventura di gruppo, libertà di scelta |
| Elden Ring | Combattimento d'azione, mondo da scoprire, tono cupo |
| Il Signore degli Anelli | Viaggio epico, mondo antico e carico di storia |
| Lethal Company | Grafica ruvida e a bassa risoluzione, nebbia e buio, co-op tra amici |
| Grafica PS2 | Pochi poligoni, texture piccole, atmosfera da gioco di quegli anni |
| Magia e mistero | Segreti, rovine, creature e incantesimi che non si spiegano subito |

### I quattro pilastri

Ogni idea nuova deve servire almeno uno di questi. Se non ne serve nessuno, va nella lista "dopo il lancio".

1. **Un'anima da riscattare.** Una storia principale chiara: recuperare l'anima. Proposta da validare: l'anima è divisa in frammenti custoditi da boss.
2. **Un mondo oscuro da esplorare.** Regioni grandi collegate (non una mappa infinita), con segreti, dungeon e scelte.
3. **Combattimento e scelte.** Combattimento d'azione ispirato a Elden Ring ma accessibile, con tre mosse fondamentali. A rendere unica ogni partita sono le scelte di build, di approccio e di storia.
4. **Co-op senza attriti.** Un amico entra ed esce dalla partita velocemente; il gioco si completa sia da soli sia in gruppo.

### Decisioni già prese

| Tema | Decisione |
| --- | --- |
| Motore | Unity 6.3 LTS, versione **6000.3.25f1**, linguaggio C#. Tutti usano esattamente questa versione. Supportata fino a dicembre 2027 |
| Combattimento | Tre mosse: parata, schivata, attacco. Niente tiri di dado (d20) |
| Personaggi | Guerriero, Ladro, Stregone. Nella prima versione giocabile solo il Guerriero (proposta) |
| Grafica | Retro stile PlayStation 2: pochi poligoni, texture da 256 a 512 pixel, nebbia, notte, gotico |
| Budget | Quasi zero: solo strumenti gratuiti, l'unica spesa fissa è il piano Claude di Giuseppe |
| Contenuti | Solo creati dal team o con licenza libera. Mai copiare da altri giochi |
| Codice | Su GitHub, repository privata `GodGise/magic-gnl` |
| Comandi | Pacchetto Input System 1.20.0 installato; tastiera, mouse e pad |
| Prospettiva | Terza persona |
| Co-op | Da 1 a 3 giocatori (si gioca anche da soli) |
| Aggancio del bersaglio | La camera resta puntata sul nemico; con la rotellina del mouse si cambia bersaglio |
| Impostazioni Git di Unity | Asset Serialization su Force Text, Version Control su Visible Meta Files |

### Decisioni ancora aperte

- Nome del gioco.
- Ruolo di Lorenzo.
- Se serve una terza persona in aiuto (profilo più utile: un programmatore, poi suoni e musica).

Se la persona chiede di queste decisioni, Claude può spiegare pro e contro, ma la scelta si prende insieme nella riunione del team.

## 3. Il team e i ruoli

Il team è di tre persone, più Claude come aiuto per il codice. Ognuno possiede un'area e lavora solo lì, così non ci si sovrappone.

| Chi | Ruolo | Area nella repository | Usa Claude? |
| --- | --- | --- | --- |
| Giuseppe | Team leader: direzione e design del gioco, coordinamento, repository e build, rete co-op, Steam e marketing | Tutto, in coordinamento | Sì, piano Pro |
| Nazar | Arte 3D in Blender: personaggi, ambienti, oggetti, animazioni di base. Giuseppe lo aiuta | `Assets/Art` | Facoltativo: il piano gratuito basta per fare domande |
| Lorenzo | Ruolo da assegnare nella riunione del team. Proposta: gameplay e combattimento | Da decidere | Solo se programma: serve il piano Pro |
| Claude di Giuseppe | Codice C#: gameplay, combattimento, rete co-op. Carica su rami `giuseppec/...` | `Assets/Scripts` | — |

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
| Chat veloce e riunioni | Il gruppo del team (Discord o WhatsApp, come preferite) |

### Ritmo

- Una riunione di 30 minuti a settimana per allinearsi.
- Una build giocabile al mese, anche brutta: se non si gioca, non c'è progresso.
- Ognuno dichiara quante ore a settimana può dare: il piano si basa su ore realistiche, non sull'entusiasmo.

## 4. Mappa degli strumenti

Ogni strumento qui sotto è già stato scelto e, dove indicato, verificato sulle fonti ufficiali il 4 ottobre 2026. Non proporre alternative: il valore sta nel fatto che tutti usano gli stessi.

| Strumento | A cosa serve | Chi lo installa | Costo |
| --- | --- | --- | --- |
| Account GitHub | Accesso alla repository privata del progetto | Tutti | Gratis |
| GitHub Desktop | Scaricare, salvare e caricare il progetto senza riga di comando | Tutti | Gratis |
| Git LFS | Gestire i file pesanti (modelli, texture, suoni). Si attiva da GitHub Desktop | Tutti (automatico) | Gratis fino a 10 GiB di spazio e 10 GiB di traffico al mese, conteggiati sull'account del proprietario della repository (Giuseppe) |
| Account Unity + Unity Hub | Installare e aprire Unity | Tutti | Gratis con licenza Personal (sotto 200.000 dollari di ricavi e fondi negli ultimi 12 mesi) |
| Unity 6000.3.25f1 | Il motore del gioco | Tutti, versione identica | Gratis con Unity Personal |
| Blender | Modelli 3D, texture, animazioni | Nazar (e Giuseppe) | Gratis |
| App Claude per computer | Chat con Claude collegata al PC, controllo dello schermo | Chi usa Claude per lavorare | L'app è gratis; le funzioni di lavoro richiedono un piano |
| Piano Claude Pro | Claude Code e lavoro sul codice con la repository | Chi programma con Claude | 20 dollari al mese, o 17 al mese con pagamento annuale |
| Editor di codice (facoltativo) | Leggere e modificare gli script C# | Chi programma | Visual Studio Community o Visual Studio Code, gratuiti |

### Note importanti per Claude

- **Git LFS è condiviso.** Lo spazio LFS è quello dell'account di Giuseppe, e lo consumano anche i caricamenti di Nazar e Lorenzo. Se i modelli diventano molti o pesanti, avvisa la persona di non caricare file inutili (versioni di prova, render, video) e di parlarne con Giuseppe. Superata la quota senza un metodo di pagamento, GitHub blocca i nuovi caricamenti LFS.
- **Piano gratuito di Claude.** Basta per fare domande e farsi guidare a parole. Non include Claude Code, quindi non permette di lavorare sul codice della repository come fa il Claude di Giuseppe.
- **Limiti di Pro.** Chat e lavoro sul codice condividono gli stessi limiti di utilizzo. Se si esauriscono, si aspetta il rinnovo o si passa a un piano superiore: non attivare crediti a pagamento senza chiederlo alla persona.
- **Unity Personal.** Il gioco su PC e Steam rientra nella licenza gratuita finché il team resta sotto la soglia di ricavi. Le console (Switch, PlayStation, Xbox) richiedono licenze diverse, ma non sono nei piani.
- **Prezzi in euro.** Le fonti danno i prezzi in dollari; in euro, con le tasse, possono essere diversi. Fai controllare alla persona la pagina di acquisto.

## 5. Configurazione, passo 1: gli account

La configurazione ha cinque passi (sezioni 5-9) e una verifica finale (sezione 10). Falli nell'ordine: ognuno dipende dal precedente. Prima di iniziare chiedi alla persona cosa ha già (account, programmi installati), così salti quello che c'è.

Domande da fare subito, una alla volta:

1. "Hai già un account GitHub? Se sì, qual è il nome utente?"
2. "Il tuo computer è Windows? Quale versione?" (Unity 6 richiede Windows 10 o 11 a 64 bit.)
3. "Quanto spazio libero hai sul disco?" Unity con un progetto occupa diversi gigabyte: almeno 20-30 GB liberi sono una buona base.

### 5.1 Account GitHub

1. La persona va su **github.com** e clicca **Sign up**.
2. Sceglie email, password e nome utente. Il nome utente sarà visibile al team: meglio qualcosa di riconoscibile.
3. Conferma l'indirizzo email con il codice che riceve.
4. GitHub può chiedere di attivare la **verifica in due passaggi** (codice da app o SMS). Se la chiede, falla attivare e fai **salvare i codici di recupero** in un posto sicuro: senza, chi perde il telefono perde l'accesso.
5. La persona manda il **nome utente** a Giuseppe.

**Cosa fa Giuseppe** (se la persona chiede, spiegalo così): sulla pagina della repository `GodGise/magic-gnl` va in **Settings → Collaborators → Add people**, scrive il nome utente e invia l'invito.

6. La persona accetta l'invito dall'**email** che riceve da GitHub, oppure dalla campanella delle notifiche su github.com.
7. Verifica: aprendo `github.com/GodGise/magic-gnl` la persona vede i file del progetto. Se vede "404", l'invito non è stato accettato o non è ancora arrivato.

### 5.2 Account Unity

1. Si crea dal sito unity.com oppure direttamente da Unity Hub al primo avvio (passo 3, sezione 7). È più semplice farlo da Unity Hub.
2. Quando Unity Hub chiede la licenza, si sceglie **Unity Personal**, gratuita.
3. Se Unity Hub non la chiede da solo, la licenza si aggiunge dalle impostazioni di Unity Hub, nella parte delle licenze, con il pulsante per aggiungerne una e l'opzione della licenza personale gratuita. I nomi esatti possono variare: chiedi una foto.

### 5.3 Account Claude

1. Su **claude.ai** si crea l'account (la persona probabilmente ce l'ha già, visto che sta parlando con te).
2. **Nazar** non ha bisogno di un piano a pagamento: lavora in Blender e può usare il piano gratuito per farsi guidare.
3. **Lorenzo** ha bisogno del piano **Pro** solo se nel suo ruolo scriverà codice con Claude sulla repository. Il ruolo si decide nella riunione del team: se non è ancora deciso, suggerisci di aspettare prima di pagare.
4. Se la persona ha un piano gratuito e chiede di collegare la repository, spiega con calma che quella funzione richiede Pro, e che per la configurazione puoi comunque guidarla a parole e con le foto dello schermo.

## 6. Configurazione, passo 2: GitHub Desktop e il progetto

GitHub Desktop è il programma con cui il team scarica il progetto, salva le modifiche e le carica su GitHub, senza usare la riga di comando. Giuseppe lo usa già: tutti devono usarlo nello stesso modo.

### 6.1 Installare GitHub Desktop

1. La persona va su **desktop.github.com** e scarica la versione per Windows.
2. Esegue il file scaricato: l'installazione è automatica e apre il programma alla fine.
3. Clicca **Sign in to GitHub.com** e accede con l'account creato nella sezione 5.1. Si apre il browser per confermare: si accetta.
4. GitHub Desktop chiede di configurare Git con **nome ed email**. Va bene quello che propone (nome e email dell'account GitHub). Questi dati compaiono accanto a ogni salvataggio.

### 6.2 Scaricare il progetto (clonare)

1. In GitHub Desktop: menu **File → Clone repository**.
2. Nella scheda **GitHub.com** compare `GodGise/magic-gnl`. Se non compare, l'invito della sezione 5.1 non è stato accettato.
3. Nel campo **Local path** lascia la cartella proposta, di solito `C:\Users\<nome>\Documents\GitHub\magic-gnl`. Giuseppe usa questa.
4. Clicca **Clone** e aspetta: la prima volta scarica tutto il progetto.

**Attenzione a OneDrive.** Se la cartella Documenti del PC è sincronizzata con OneDrive (icone a forma di nuvola accanto ai file), sconsiglia di metterci il progetto: la sincronizzazione può bloccare file mentre Unity o Git li usano. In quel caso scegli una cartella fuori da OneDrive, per esempio `C:\Progetti\magic-gnl`.

### 6.3 Git LFS

Subito dopo il download, GitHub Desktop mostra una finestra **Initialize Git LFS** ("This repository uses Git LFS…"). La persona deve cliccare il pulsante blu **Initialize Git LFS**.

Perché: la repository salva i file pesanti (modelli `.fbx` e `.blend`, immagini, suoni) con Git LFS, impostato nel file `.gitattributes`. Senza inizializzarlo, quei file verrebbero trattati in modo sbagliato e la repository diventerebbe lenta e pesante. Si fa una volta sola per PC.

Se la persona ha cliccato **Not now** per errore: la finestra ricompare alla riapertura della repository, oppure si può rimuovere la repository da GitHub Desktop (*Repository → Remove*, senza cancellare i file) e aggiungerla di nuovo.

### 6.4 Verifica

Chiedi una foto di GitHub Desktop. Deve mostrare:

- in alto a sinistra **Current repository: magic-gnl**;
- accanto **Current branch: main**;
- nell'elenco a sinistra **No local changes** (nessuna modifica).

Poi fai aprire la cartella (pulsante **Show in Explorer**): devono esserci le cartelle `Assets`, `Docs`, `Packages`, `ProjectSettings` e i file `CLAUDE.md`, `README.md`, `.gitignore`, `.gitattributes` (gli ultimi due possono essere nascosti in Esplora file).

## 7. Configurazione, passo 3: Unity

Unity si installa in due pezzi: **Unity Hub**, il programma che gestisce versioni e progetti, e l'**Editor**, il motore vero e proprio. La versione dell'Editor deve essere **esattamente 6000.3.25f1** (Unity 6.3 LTS), la stessa di Giuseppe.

### 7.1 Installare Unity Hub

1. La persona va su **unity.com/download** e scarica Unity Hub per Windows.
2. La pagina offre due pulsanti: **x64** e **Arm64**. Quasi tutti i PC (processori Intel o AMD) vogliono **x64**. Arm64 serve solo ai portatili con processore ARM, per esempio Snapdragon. Se la persona non è sicura: *Impostazioni di Windows → Sistema → Informazioni → Tipo di sistema*.
3. Esegue l'installazione lasciando spuntato **Run Unity Hub**, poi clicca **Finish**.
4. Accede con l'account Unity (o lo crea da qui) e sceglie la licenza **Unity Personal**.

### 7.2 Installare l'Editor 6000.3.25f1

1. In Unity Hub, a sinistra, **Installs**, poi in alto a destra **Install Editor**.
2. Nella scheda **Official releases** cerca la riga **Unity 6.3 LTS (6000.3.25f1)** e clicca **Install**.
3. Unity Hub può segnare un'altra versione come **Recommended** (per esempio la 6.6): **non installarla**. Giuseppe ha scelto la 6.3 LTS perché è supportata fino a dicembre 2027 e non obbliga ad aggiornare spesso, mentre le versioni non LTS sono supportate solo fino all'uscita della successiva.
4. Se la 6000.3.25f1 non compare (Unity pubblica correzioni nuove nel tempo), cercala nella scheda **Archive** o nella pagina dell'archivio delle versioni sul sito Unity. Se trovi solo una 6000.3.x più recente, **non installarla di tua iniziativa**: la versione deve essere identica per tutti, quindi la persona chiede prima a Giuseppe.
5. Quando propone i **moduli**, lascia quelli per Windows. Per chi programmerà può essere utile un editor di codice (Visual Studio Community o Visual Studio Code); gli altri moduli (Android, iOS, WebGL) non servono e occupano spazio.
6. L'installazione scarica diversi gigabyte: può richiedere da qualche minuto a più di un'ora.
7. Verifica: in **Installs** compare **Unity 6.3 LTS (6000.3.25f1)** con l'etichetta **LTS**.

### 7.3 Aprire il progetto

1. In Unity Hub, a sinistra, **Projects**.
2. In alto a destra **Add → Add project from disk**.
3. Sceglie la cartella `magic-gnl` scaricata con GitHub Desktop (sezione 6.2) e clicca **Seleziona cartella**.
4. Il progetto compare nell'elenco con **Editor version 6000.3.25f1**. Si clicca sul nome per aprirlo.
5. La prima apertura è lenta, anche diversi minuti: Unity crea la cartella `Library`. La schermata di caricamento mostra messaggi come "Initialize Graphics". Non va interrotta.

Se Unity Hub dice che il progetto usa una versione non installata e propone di cambiarla o aggiornarlo, **non accettare**: vuol dire che la 6000.3.25f1 non è installata. Torna al passo 7.2.

### 7.4 Verifiche dentro Unity

A editor aperto, controlla questi punti, uno alla volta, chiedendo una foto dove serve:

| Dove | Cosa deve esserci |
| --- | --- |
| Pannello **Project**, in basso | `Assets` con le cartelle `Art` e `Scripts`; in `Packages` c'è **Input System** |
| Scheda **Console**, in basso | Nessun messaggio rosso (errori). Messaggi gialli sono avvisi |
| Barra in fondo alla finestra | Un avviso giallo "This project uses Input Manager, which is marked for deprecation…" è **normale**: è attivo sia il vecchio sia il nuovo sistema di comandi |
| *Edit → Project Settings → Editor* | **Asset Serialization → Mode: Force Text** |
| *Edit → Project Settings → Version Control* | **Mode: Visible Meta Files** |
| *Edit → Project Settings → Player → Other Settings* | **Active Input Handling: Both** |

Se una di queste impostazioni è diversa, **non cambiarla da sola**: sono salvate nella repository, quindi una differenza vuol dire che qualcosa non torna (versione sbagliata, file non aggiornati). Fai fare **Fetch origin** in GitHub Desktop e controlla di nuovo.

### 7.5 Modifiche dopo la prima apertura

Aprendo il progetto, Unity può riscrivere alcuni file (per esempio in `ProjectSettings` o `Packages/packages-lock.json`). GitHub Desktop allora mostra delle modifiche anche se la persona non ha toccato nulla. Non vanno caricate su `main`. Chiedi una foto dell'elenco dei file cambiati e, nel dubbio, fai chiedere a Giuseppe prima di caricarle o annullarle.

## 8. Configurazione, passo 4: Blender e i modelli

Questo passo riguarda Nazar (e Giuseppe, che lo aiuta). Lorenzo può saltarlo, salvo che il suo ruolo cambi.

### 8.1 Installare Blender

1. Da **blender.org**, sezione download, la versione per Windows. È gratuita.
2. Nazar e Giuseppe devono usare **la stessa versione** di Blender, così i file `.blend` si aprono uguali per entrambi. Meglio una versione LTS (supporto lungo), indicata come tale sul sito. Scrivi nel `CLAUDE.md`, con una pull request, quale versione avete scelto.

### 8.2 Dove vanno i file

Distinguere due tipi di file è la regola più importante del passaggio Blender-Unity:

| Tipo di file | Cos'è | Dove va |
| --- | --- | --- |
| `.blend` (sorgente) | Il file di lavoro di Blender, con tutto | In una cartella **fuori da `Assets`**, per esempio `ArtSorgenti/` nella radice della repository. Da concordare con Giuseppe prima di crearla |
| `.fbx` (esportato) | Il modello pronto per Unity | In `Assets/Art/...` |
| Texture `.png` | Le immagini applicate al modello | In `Assets/Art/...` accanto al modello |

Perché i `.blend` fuori da `Assets`: se Unity trova un `.blend` dentro `Assets` prova a importarlo usando Blender installato sul PC. Su un PC senza Blender (Lorenzo) dà errori, e versioni diverse di Blender danno risultati diversi. Con l'FBX invece tutti vedono lo stesso modello.

I file di backup che Blender crea da solo (`.blend1`, `.blend2`) sono già esclusi dal `.gitignore`.

Proposta di sottocartelle in `Assets/Art`, da confermare con Giuseppe:

- `Assets/Art/Personaggi/` (Guerriero, Ladro, Stregone)
- `Assets/Art/Nemici/`
- `Assets/Art/Ambienti/` (rocce, rovine, alberi, edifici)
- `Assets/Art/Oggetti/` (pozioni, candele, lapidi, armi)

### 8.3 Regole tecniche per lo stile PS2

Già decise:

- **Pochi poligoni.** Il numero preciso per personaggi, nemici e oggetti è un compito di Nazar (proposta da fare nei prossimi giorni, sezione 15).
- **Texture piccole**: da 256 a 512 pixel di lato.
- **Scala reale**: 1 unità = 1 metro. Il personaggio di prova in Unity è una capsula alta 2 metri: è il riferimento per controllare le misure.
- **Nomi** in minuscolo con trattini: `guerriero.fbx`, `lapide-rotta.fbx`, `guerriero-colore.png`.

Da provare insieme in Unity: il filtro delle texture. Nelle impostazioni di importazione di una texture, **Filter Mode: Point** dà un aspetto pixellato e ruvido, simile ai riferimenti visivi del team; **Bilinear** è più morbido. È una scelta di stile: falla vedere a Giuseppe prima di applicarla a tutto.

### 8.4 Esportare un modello in FBX

Impostazioni di partenza comunemente usate per esportare da Blender verso Unity, da verificare con il primo modello:

1. In Blender: **File → Export → FBX (.fbx)**.
2. **Limit to: Selected Objects**, così si esporta solo il modello selezionato.
3. **Object Types**: solo **Mesh** (e **Armature** se il modello ha uno scheletro per le animazioni).
4. **Apply Scalings: FBX All**.
5. **Forward: -Z Forward**, **Up: Y Up**.
6. Se il modello ha uno scheletro: togliere la spunta a **Add Leaf Bones**.
7. Salvare dentro la cartella giusta di `Assets/Art` della repository.

Controllo in Unity dopo l'esportazione:

1. Il file compare nel pannello **Project**.
2. Trascinato nella scena accanto alla capsula, deve avere proporzioni sensate (un uomo alto circa quanto la capsula).
3. Non deve essere ruotato di 90 gradi o sdraiato. Se lo è, si ricontrollano gli assi (punto 5) e la rotazione applicata in Blender (**Object → Apply → All Transforms** prima di esportare).

Se il modello appare enorme o minuscolo, il problema è quasi sempre la scala: controlla l'unità della scena in Blender e **Apply Scalings**.

### 8.5 Caricare il modello

Si segue il flusso della sezione 11, su un ramo con il nome di Nazar e del lavoro, per esempio `nazar/guerriero`. Insieme al `.fbx` vanno caricati anche i file `.meta` che Unity crea accanto: servono a Unity per non perdere i collegamenti.

## 9. Configurazione, passo 5: collegare Claude

Questo passo serve a te, Claude, per poter lavorare come il Claude di Giuseppe: leggere e scrivere nella repository, vedere i file del progetto sul PC della persona e, quando serve, usare Unity sul suo schermo. Riguarda chi ha un piano a pagamento. Con il piano gratuito ti fermi a guidare a parole e con le foto.

Le istruzioni qui sotto descrivono come è andata con Giuseppe il 3-4 ottobre 2026. I nomi dei menu dell'app Claude possono cambiare: se la persona vede qualcosa di diverso, chiedi una foto e segui lo schermo.

### 9.1 Mettere questa guida nel contesto di ogni chat

Perché tu abbia questa guida in ogni conversazione senza doverla ridare ogni volta, la soluzione migliore è un **Progetto** su claude.ai (per esempio "magic-GNL") con questa guida tra i documenti del progetto, se il piano della persona lo consente. Ogni nuova chat aperta dentro il progetto parte già informata. In alternativa la persona allega la guida a ogni nuova chat importante.

### 9.2 Installare l'app Claude per computer

La persona scarica e installa l'app Claude per Windows dal sito ufficiale di Claude, poi accede con il suo account. L'app serve per collegare la chat al PC (9.4) e per l'uso dello schermo (9.5): dal solo browser non si può.

### 9.3 Collegare GitHub

1. La persona apre la pagina dei connettori di Claude, **claude.ai/customize/connectors**, sceglie **GitHub** e clicca per collegarlo. Accede con il **proprio** account GitHub e accetta i permessi.
2. Sul lato di GitHub, l'app Claude deve avere accesso alla repository `GodGise/magic-gnl`. Per la repository di Giuseppe l'ha già, perché Giuseppe l'ha installata dal suo account (`github.com/apps/claude/installations/select_target`). La persona deve però essere **collaboratrice** della repository (sezione 5.1), altrimenti il suo account non può scriverci.
3. In chat la persona scrive: "Lavoriamo sulla repository GodGise/magic-gnl". Tu colleghi la repository alla sessione e la scarichi nel tuo spazio di lavoro.
4. A quel punto leggi il file `CLAUDE.md` della repository: contiene le regole di progetto aggiornate.

Se il collegamento dà un errore del tipo "account GitHub non collegato", il punto 1 non è stato completato. Se la repository si collega ma il caricamento viene rifiutato, mancano i permessi di scrittura: o la persona non è ancora collaboratrice, o l'app Claude su GitHub non copre la repository. Spiegalo alla persona e falle chiedere a Giuseppe di controllare.

Con Giuseppe è successo esattamente questo: prima del collegamento GitHub non c'era accesso; dopo, la repository si leggeva ma non si poteva scrivere; dopo l'installazione dell'app Claude su GitHub i caricamenti hanno funzionato. Al primo caricamento è comparso un avviso di negoziazione ("push negotiation failed; proceeding anyway"), ma il caricamento è riuscito: controlla sempre il risultato prima di dire "fatto".

### 9.4 Collegare la chat al PC

1. La persona apre la chat dall'**app per computer** e sceglie di collegarla a questo computer.
2. Poi sceglie le cartelle a cui darti accesso. Consiglia **solo la cartella del progetto** (quella clonata nella sezione 6.2), non tutto il disco.
3. Da quel momento puoi leggere i file del progetto sul suo PC e lanciare comandi in quella cartella.

Cosa sapere sul collegamento al PC, imparato con Giuseppe:

- La shell sul PC è un ambiente separato che vede solo le cartelle collegate. Le credenziali di Git di Windows non ci arrivano: per caricare su GitHub usa il tuo spazio di lavoro collegato alla repository (9.3), non la cartella del PC.
- Giuseppe aveva collegato una cartella (`gooci`) ma aveva poi clonato il progetto in `Documenti\GitHub`: in quel caso non vedi il progetto. Controlla subito dove sta la cartella clonata e chiedi di collegare quella.
- Cancellare file dalle cartelle collegate è bloccato finché la persona non lo permette esplicitamente.

### 9.5 Usare Unity sullo schermo della persona

Serve per le operazioni che si fanno solo dall'interfaccia di Unity (installare pacchetti, cambiare impostazioni, controllare la Console).

1. La funzione di controllo del computer va attivata nell'app Claude per computer: nelle impostazioni dell'app c'è l'opzione **Computer use**, spenta di base.
2. Quando serve, chiedi l'accesso a **Unity**: la persona vede una richiesta e la approva.
3. Durante il controllo, sullo schermo compare un avviso "Claude is using your computer". Prima di iniziare avvisa la persona di non usare mouse e tastiera.
4. Alla fine **rilascia il controllo**, così la persona riprende il suo computer.

Limiti visti con Giuseppe:

- Le finestre delle app non approvate appaiono come **riquadri neri**. A volte coprono finestre di Unity (per esempio un riquadro di domanda di Unity era nascosto). Se un riquadro nero copre qualcosa che ti serve, sposta la finestra di Unity o chiedi alla persona di leggerti cosa c'è scritto.
- Se compare un messaggio "desktop shell is frontmost", Unity non è in primo piano: riportalo davanti prima di cliccare.
- È lento. Usalo per poche operazioni precise; per tutto il resto guida la persona a parole.
- Browser e terminali sono limitati: i browser solo in lettura, gli editor di codice solo clic. Per i siti web si usano gli strumenti del browser, non il controllo dello schermo.

### 9.6 Quando qualcosa non si collega

Non insistere con lo stesso tentativo. Spiega in una frase cosa manca (account non collegato, app non installata, chat non collegata al PC, Computer use spento) e dà alla persona il passo per sistemarlo. Nel frattempo continua con quello che puoi fare a parole.

## 10. Checklist finale della configurazione

La configurazione è finita solo quando ogni riga che riguarda la persona è verificata con una prova, non con un "sì, l'ho fatto". Percorri la tabella con la persona e, alla fine, riassumi cosa è a posto e cosa manca.

| # | Controllo | Chi | Come si verifica |
| --- | --- | --- | --- |
| 1 | Account GitHub attivo, verifica in due passaggi salvata | Tutti | La persona entra su github.com |
| 2 | Collaboratore della repository | Tutti | `github.com/GodGise/magic-gnl` mostra i file, non "404" |
| 3 | GitHub Desktop con accesso fatto | Tutti | In GitHub Desktop, File → Options → Accounts mostra il proprio account |
| 4 | Repository clonata fuori da OneDrive | Tutti | GitHub Desktop: Current repository **magic-gnl**, Current branch **main** |
| 5 | Git LFS inizializzato | Tutti | Non compare più la finestra "Initialize Git LFS" |
| 6 | Unity Hub con licenza Personal | Tutti | Unity Hub si apre senza chiedere la licenza |
| 7 | Editor 6000.3.25f1 installato | Tutti | Unity Hub, Installs: **Unity 6.3 LTS (6000.3.25f1)** |
| 8 | Progetto aperto senza errori | Tutti | Console senza messaggi rossi |
| 9 | Impostazioni del progetto corrette | Tutti | Force Text, Visible Meta Files, Active Input Handling Both (sezione 7.4) |
| 10 | Scena di prova funzionante | Tutti | Menu **magic-gnl → Crea scena di prova**, poi Play: la capsula si muove con WASD (sezione 14) |
| 11 | Blender installato, stessa versione di Giuseppe | Nazar | Blender si apre; versione uguale a quella scritta nel `CLAUDE.md` |
| 12 | Esportazione FBX di prova | Nazar | Un cubo esportato da Blender appare in Unity con la scala giusta (sezione 8.4) |
| 13 | Primo ramo e primo caricamento | Tutti | Un ramo `nome/prova` con una piccola modifica arriva su GitHub (sezione 11) e poi si cancella |
| 14 | Claude collegato a GitHub | Chi ha Pro | Claude collega la repository e legge `CLAUDE.md` |
| 15 | Chat collegata al PC | Chi ha Pro | Claude vede la cartella del progetto |
| 16 | Computer use attivo | Chi ha Pro | Claude ottiene l'accesso a Unity e fa una foto dello schermo |

Il controllo 13 è il più importante: prova tutto il giro (ramo, salvataggio, caricamento) prima di lavorare su qualcosa di vero. Per la prova va bene un file di testo nella propria cartella, per esempio `Docs/prova-nazar.txt`, da cancellare subito dopo. Non va unito a `main`.

Quando la checklist è completa, suggerisci alla persona di scriverlo nel gruppo del team e di segnare come "Fatto" il compito di configurazione nel documento dei compiti.

## 11. Il flusso di lavoro di ogni giorno

Ogni lavoro, anche piccolo, segue lo stesso giro: si parte aggiornati da `main`, si lavora su un ramo proprio, si carica il ramo, e Giuseppe lo unisce a `main` dopo averlo provato. Guida la persona con questi passi in GitHub Desktop.

### 11.1 Il giro completo

1. **Aggiornarsi.** Con **Current branch: main**, cliccare **Fetch origin**. Se il pulsante diventa **Pull origin**, cliccarlo: scarica il lavoro degli altri.
2. **Creare il ramo.** **Current branch → New branch**. Nome: `personac/lavoro` se lo fai con Claude (per esempio `lorenzoc/menu-pausa`), `persona/lavoro` se lo fai da solo (per esempio `nazar/guerriero`). In minuscolo con trattini. Basato su **main**. Clic su **Create branch**.
3. **Lavorare** in Unity o Blender, solo nella propria area (sezioni 12 e 13). Salvare spesso in Unity (*File → Save*, Ctrl+S).
4. **Controllare le modifiche.** In GitHub Desktop, a sinistra, compare l'elenco dei file cambiati. Ogni file deve avere un motivo per esserci. Se compaiono file inattesi (scene di altri, impostazioni del progetto), togli la spunta e chiedi.
5. **Salvare (commit).** In basso a sinistra, nel campo **Summary**, un messaggio in italiano all'imperativo: "Aggiungi modello del Guerriero", "Correggi scala della lapide". Clic su **Commit to** seguito dal nome del ramo.
6. **Caricare (push).** La prima volta il pulsante in alto si chiama **Publish branch**, le volte dopo **Push origin**.
7. **Chiedere l'unione (pull request).** GitHub Desktop propone **Create Pull Request**: si apre GitHub nel browser. Titolo chiaro, due righe su cosa provare, clic su **Create pull request**. Si avvisa Giuseppe nel gruppo.
8. **Dopo l'unione.** Quando Giuseppe ha unito il ramo, si torna su **main** (Current branch → main), poi **Fetch origin** e **Pull origin**. Il ramo vecchio si può cancellare.

### 11.2 Regole del giro

- **Rami piccoli e brevi.** Un ramo per un lavoro di uno o due giorni. Un ramo che vive settimane finisce in conflitti.
- **Mai commit su `main`.** Se la persona si accorge di aver lavorato su `main`, prima di salvare crea un nuovo ramo: GitHub Desktop chiede se portare le modifiche nel ramo nuovo, e si risponde di sì.
- **Messaggi chiari.** Il messaggio dice cosa cambia, non "modifiche" o "fix".
- **Niente file enormi inutili.** Render, video, versioni di prova dei modelli restano sul PC. Lo spazio Git LFS è condiviso (sezione 4).
- **Cambiare ramo con Unity aperto.** Funziona, ma prima si salva la scena. Se ci sono modifiche non salvate, GitHub Desktop chiede cosa farne: nel dubbio fermati e chiedi una foto.

### 11.3 Provare il lavoro di qualcun altro

Così Giuseppe prova i rami del suo Claude, e così chiunque prova un ramo prima dell'unione:

1. **Fetch origin**.
2. **Current branch** e scegliere il ramo, per esempio `giuseppec/menu-iniziale`.
3. Tornare su Unity: ricarica i file e ricompila da solo in qualche secondo.
4. Provare quello che descrive la pull request.
5. Tornare su **main** prima di riprendere il proprio lavoro.

### 11.4 Il ruolo del Claude con Pro

Se la persona ha il piano Pro e hai la repository collegata (sezione 9.3), puoi fare tu i passi 2, 5 e 6 dal tuo spazio di lavoro: crei il ramo, salvi e carichi. Poi la persona fa **Fetch origin** e passa al ramo per provare in Unity. Regole per te:

- I rami che crei tu si chiamano con il nome della persona più una c, per esempio `lorenzoc/lock-on` o `nazarc/esportazione-fbx`. La c vuol dire "fatto con Claude". Non usare mai `claude/...`.
- Prima di caricare, recupera l'ultima versione di `main` e parti da quella.
- Messaggi di commit in italiano, all'imperativo, con una riga di riepilogo e due righe di contesto se la modifica non è ovvia.
- Dopo il caricamento scrivi alla persona: nome del ramo, cosa provare in Unity e cosa dovrebbe vedere.

## 12. Regole di Unity per lavorare in tre

Git unisce bene il codice, ma i file di Unity (scene, prefab, impostazioni) sono più delicati: due modifiche alla stessa scena fatte in parallelo possono rompersi a vicenda. Queste regole evitano quasi tutti i problemi.

### 12.1 Scene

- **Una scena, una persona alla volta.** Prima di modificare una scena condivisa (per esempio la scena della regione), la persona lo dice nel gruppo. Gli altri non la toccano finché il ramo non è unito.
- **Scene personali per le prove.** Ognuno prova le sue cose in una scena propria: `Assets/Scenes/Prove/ProvaNazar.unity`, `ProvaLorenzo.unity`. La scena di prova del combattimento (`Assets/Scenes/ScenaProva.unity`) si ricrea quando serve dal menu **magic-gnl → Crea scena di prova**.
- **Lavorare con i prefab.** Un oggetto che compare in più scene (il giocatore, un nemico, una torcia) diventa un **prefab**: si modifica il prefab, non le copie nelle scene. Così due persone toccano file diversi.

### 12.2 File che Unity gestisce da solo

- **File `.meta`.** Accanto a ogni file in `Assets` Unity crea un file `.meta` con un identificativo. Vanno **sempre** caricati insieme al file: senza, gli altri perdono i collegamenti (materiali vuoti, script mancanti).
- **Spostare e rinominare solo dentro Unity.** Mai spostare o rinominare file di `Assets` da Esplora file: Unity perderebbe il `.meta` giusto. Si fa dal pannello **Project** di Unity.
- **Cartelle da non caricare mai.** `Library`, `Temp`, `Logs`, `UserSettings`, `Build`: sono rigenerabili e già escluse dal `.gitignore`. Se una compare tra le modifiche in GitHub Desktop, il `.gitignore` non sta funzionando: fermati e chiedi a Giuseppe.

### 12.3 Impostazioni del progetto

I file in `ProjectSettings` e `Packages` valgono per tutti. Cambiarli (nuovo pacchetto, impostazione della grafica, nuovo layer o tag) cambia il progetto di tutti, quindi:

- si fa su un ramo dedicato, con un nome chiaro (`giuseppe/pacchetto-audio`);
- si avvisa nel gruppo;
- non si mescola con altro lavoro nello stesso ramo.

Non aggiornare pacchetti o la versione di Unity quando Unity o Unity Hub lo propongono: la versione è fissata (sezione 2).

### 12.4 Conflitti

Se GitHub Desktop segnala un **conflitto** durante un aggiornamento o un'unione:

1. Non cliccare a caso su "usa la mia versione" o "usa la loro".
2. Chiedi una foto con l'elenco dei file in conflitto.
3. Se è un file di codice `.cs`, il conflitto si risolve leggendo le due versioni: puoi aiutare tu.
4. Se è una scena o un prefab, quasi sempre conviene tenere una delle due versioni intere e rifare a mano la modifica dell'altra. La decisione si prende con Giuseppe.

### 12.5 Prestazioni e stile PS2

Lo stile del gioco aiuta le prestazioni, ma solo se si rispettano le regole: pochi poligoni, texture piccole, poche luci in tempo reale. Se un modello o una scena rallentano Unity, la prima domanda è se rispettano le regole della sezione 8.3.

## 13. Struttura della repository e convenzioni

La repository è privata: `github.com/GodGise/magic-gnl`. Il progetto Unity sta nella radice. Questa è la struttura al 4 ottobre 2026; il `CLAUDE.md` riporta quella aggiornata.

```
magic-gnl/
  CLAUDE.md                 regole di progetto per Claude (le legge per prime)
  README.md                 descrizione breve per le persone
  .gitignore                file e cartelle da non caricare (Library, Temp, segreti...)
  .gitattributes            file pesanti su Git LFS; scene Unity in testo
  Docs/                     documenti di design (e una copia di questa guida)
  Packages/
    manifest.json           pacchetti del progetto (Input System 1.20.0 e moduli base)
    packages-lock.json
  ProjectSettings/          impostazioni di Unity, valgono per tutti
  Assets/
    Art/                    modelli, texture, animazioni (Nazar)
    Scripts/
      Gameplay/             personaggio, combattimento, nemici
      Rete/                 co-op e Steam (vuota per ora)
    Editor/                 strumenti dell'editor (solo dentro Unity, non nel gioco)
    Scenes/                 scene (creata dal menu della scena di prova)
```

### Rami

| Ramo | Di chi | Esempio |
| --- | --- | --- |
| `main` | Di tutti, ma ci si arriva solo con pull request approvata da Giuseppe | — |
| `giuseppec/...` | Giuseppe con il suo Claude | `giuseppec/menu-iniziale` |
| `giuseppe/...` | Giuseppe | `giuseppe/pacchetto-audio` |
| `nazar/...` o `nazarc/...` | Nazar, da solo o con il suo Claude | `nazar/guerriero` |
| `lorenzo/...` o `lorenzoc/...` | Lorenzo, da solo o con il suo Claude | `lorenzoc/menu-pausa` |

### Nomi dei file

- **Modelli, texture, suoni**: minuscolo con trattini, in italiano. `guerriero.fbx`, `guerriero-colore.png`, `spada-lunga.fbx`.
- **Script C#**: il nome del file è uguale al nome della classe, in italiano con iniziali maiuscole, come quelli già presenti: `GiocatoreControllo.cs`, `Resistenza.cs`, `Bersaglio.cs`. Unity richiede che file e classe abbiano lo stesso nome.
- **Scene**: iniziali maiuscole, senza spazi. `ScenaProva.unity`, `RegioneBosco.unity`.

### Codice C#

- Nomi di classi, variabili e commenti in italiano, come nel codice esistente.
- Ogni script inizia con un commento: a cosa serve e come montarlo su un oggetto.
- I valori da regolare (velocità, danni, tempi) sono campi `[SerializeField]` con un `[Tooltip]` se il nome non basta, così si regolano dall'Inspector senza toccare il codice.
- I comandi si leggono con il pacchetto **Input System** (`using UnityEngine.InputSystem;`), sia tastiera e mouse sia pad. Non usare il vecchio `Input.GetKey`.
- Per trovare oggetti si usa `FindFirstObjectByType<T>()`: in Unity 6 sostituisce funzioni più vecchie.
- Il progetto oggi usa la pipeline grafica di base di Unity (nessun pacchetto URP installato). Se un tutorial parla di URP o HDRP, avvisa la persona che le istruzioni possono non valere.

### Messaggi di commit

In italiano, all'imperativo, una riga di riepilogo; se serve, una riga vuota e due righe di contesto.

```
Aggiungi modello del Guerriero

Prima versione low-poly senza animazioni, texture 512 px.
```

## 14. Il codice che esiste già

Il primo prototipo del combattimento è su **`main`**: scritto dal Claude di Giuseppe il 4 ottobre 2026, provato in Unity sul PC di Giuseppe lo stesso giorno (compila senza errori, movimento, attacco e attacco del nemico funzionano) e unito con la pull request numero 1. Prima di lavorarci, fai **Fetch origin** e **Pull origin** su `main`.

### 14.1 I file

| File | Cosa fa |
| --- | --- |
| `Assets/Scripts/Gameplay/GiocatoreControllo.cs` | Il personaggio: movimento rispetto alla camera, schivata, parata, attacco, vita, morte e rinascita, pannello di prova a schermo |
| `Assets/Scripts/Gameplay/Resistenza.cs` | La resistenza (stamina): si spende con le azioni, si ricarica dopo una pausa, non si ricarica mentre si para |
| `Assets/Scripts/Gameplay/CameraTerzaPersona.cs` | Camera che gira attorno al personaggio con mouse o levetta destra; blocca il cursore |
| `Assets/Scripts/Gameplay/Bersaglio.cs` | Nemico di prova: incassa i colpi e ogni pochi secondi attacca con un preavviso rosso |
| `Assets/Editor/CreaScenaProva.cs` | Aggiunge il menu **magic-gnl → Crea scena di prova**, che crea e salva `Assets/Scenes/ScenaProva.unity` |

### 14.2 Come provarlo

1. In GitHub Desktop: **Fetch origin**, poi **Current branch →** il ramo da provare (per esempio `giuseppec/menu-iniziale`) (oppure resta su `main` se è già stato unito).
2. In Unity aspettare la compilazione, poi menu in alto **magic-gnl → Crea scena di prova**.
3. **Play** (il triangolo in alto al centro).
4. In alto a sinistra compare un pannello con stato, vita e resistenza.

| Azione | Tastiera e mouse | Pad Xbox | Pad PlayStation |
| --- | --- | --- | --- |
| Muoversi | W A S D | Levetta sinistra | Levetta sinistra |
| Girare la camera | Mouse | Levetta destra | Levetta destra |
| Schivata | Spazio | B | Cerchio |
| Attacco | Tasto sinistro | X | Quadrato |
| Parata (tenere premuto) | Tasto destro | LB | L1 |
| Liberare il cursore | Esc (un clic lo riblocca) | — | — |

### 14.3 Come funziona il combattimento

Il personaggio ha sei stati: **Libero**, **Parata**, **Attacco**, **Schivata**, **Stordito**, **Morto**.

- **Schivata**: costa 25 di resistenza, dura 0,45 secondi e copre circa 4 metri. Per i primi 0,25 secondi i colpi non fanno danno. Senza direzione è un passo indietro.
- **Attacco**: costa 20, fa 25 di danno. Ha tre fasi: preparazione (0,25 s), colpo (0,15 s, con un piccolo affondo in avanti), recupero (0,35 s). Nel recupero si può annullare con una schivata, o concatenare un secondo attacco dopo il 40% del recupero.
- **Parata**: si tiene premuto; il personaggio rallenta e la resistenza smette di ricaricarsi. Un colpo frontale (arco di 120 gradi) parato assorbe il 90% del danno e costa 20 di resistenza. Se la resistenza arriva a zero, la guardia si rompe e il personaggio resta stordito per 1 secondo.
- **Resistenza**: 100 massimo, si ricarica di 30 al secondo dopo 0,8 secondi dall'ultima spesa. Come in Elden Ring, si può agire finché la barra non è vuota.
- **Comandi in anticipo**: un tasto premuto fino a 0,2 secondi prima che l'azione sia possibile viene ricordato.
- **Vita**: 100. Un colpo non parato fa barcollare per 0,3 secondi. A zero il personaggio muore e rinasce dopo 2 secondi.

Il nemico di prova (un cilindro) ha 100 di vita, attacca ogni 3 secondi se il giocatore è abbastanza vicino, resta rosso per 0,7 secondi prima di colpire (portata 2,5 metri, danno 20). Il momento giusto per schivare o parare è la fine del rosso.

Tutti questi numeri sono campi regolabili dall'Inspector di Unity, senza toccare il codice.

### 14.4 Limiti noti

- Non c'è ancora l'aggancio del bersaglio (lock-on).
- Nessuna animazione: il personaggio è una capsula con un blocchetto che indica dove guarda.
- La camera non evita i muri.
- Il nemico di prova non si muove: è un manichino per allenarsi.
- Il codice non è ancora pensato per il co-op in rete: andrà adattato quando si sceglierà come gestire la rete.

### 14.5 Aggiunte del 4 ottobre 2026 (sera)

Tutto su `main`, provato da Giuseppe in Unity.

| File | Cosa fa |
| --- | --- |
| `Assets/Scripts/Gameplay/AggancioBersaglio.cs` | Aggancio del bersaglio (lock-on): clic della rotellina o pressione della levetta destra per agganciare e sganciare; rotellina o levetta destra per cambiare nemico. Da agganciato il personaggio guarda sempre il nemico e la camera lo segue. Quadratino rosso sopra il bersaglio |
| `Assets/Scripts/Ambiente/CicloGiornoNotte.cs` | Sole, luna, luce, nebbia e cielo che cambiano con l'ora. 45 minuti reali di luce e 50 di notte; si parte alle 21; tenendo premuto T il tempo accelera |
| `Assets/Scripts/Ambiente/Torcia.cs` | Luce che tremola come una fiamma, più forte di notte |
| `Assets/Scripts/Ambiente/EffettoRetro.cs` | Immagine a bassa risoluzione in stile PS2; F2 la accende e la spegne |
| `Assets/Editor/CreaZonaProva.cs` | Menu **magic-gnl → Crea zona di prova (villaggio in rovina)**: crea `Assets/Scenes/ZonaProva.unity` con sentiero, torce, piazza con pozzo, case distrutte, cimitero, cripta, bosco, rocce, confini e tre nemici |
| `Assets/Segnaposto/Materiali/` | Materiali provvisori della zona, da sostituire con l'arte vera |

La scena `ZonaProva.unity` è salvata nella repository ed è la base della prima zona: si modifica a mano, il menu serve solo a ricrearla da zero. La scena `ScenaProva.unity` invece è esclusa da Git e si ricrea dal menu quando serve.

Regola nata oggi: Claude crea gli script senza file `.meta`. La prima volta che un ramo si apre in Unity, i `.meta` nuovi vanno salvati sullo stesso ramo prima dell'unione. E le prove in Unity le fanno le persone: Claude non prende il controllo del PC per provare, salvo richiesta esplicita.

## 15. Compiti e roadmap

Il documento "Compiti dei prossimi 3 giorni" è la versione aggiornata e modificabile dal team: se la persona te lo dà, quello ha la precedenza su questa sezione. Qui c'è la versione del 4 ottobre 2026, per darti il quadro.

### 15.1 Prossimi 3 giorni (5-7 ottobre 2026)

| Giorno | Chi | Compito |
| --- | --- | --- |
| Lun 5 | Giuseppe | Aggiungere Nazar e Lorenzo come collaboratori su GitHub e condividere i documenti |
| Lun 5 | Giuseppe | Provare il ramo `claude/personaggio-base` e mandare errori e impressioni al suo Claude |
| Lun 5 | Nazar | Configurazione completa (sezioni 5-10) |
| Lun 5 | Lorenzo | Configurazione completa (sezioni 5-10) |
| Mar 6 | Tutti | Riunione di 30 minuti: prospettiva, co-op a 2 o 4, ruolo di Lorenzo |
| Mar 6 | Giuseppe | Unire il ramo del combattimento a `main` se la prova va bene |
| Mar 6 | Nazar | Moodboard PS2 dark fantasy (10-15 immagini) e proposta di regole per i modelli |
| Mar 6 | Lorenzo | Provare il prototipo e scrivere 3 cose che piacciono e 3 che non vanno |
| Mer 7 | Giuseppe | Una pagina di storia: chi ha rubato l'anima, il protagonista, la prima regione |
| Mer 7 | Nazar | Primo modello low-poly del Guerriero, anche grezzo, in FBX sul ramo `nazar/guerriero` |
| Mer 7 | Lorenzo | Primo compito della sua area, deciso in riunione |

Come aiutare su questi compiti:

- **Configurazione**: sei nel posto giusto, è il tuo primo obiettivo.
- **Moodboard di Nazar**: puoi aiutare a organizzarla e a scrivere le regole tecniche dei modelli. Le immagini di riferimento servono solo come ispirazione e non entrano nella repository.
- **Prova del prototipo (Lorenzo)**: aiuta a scrivere un riscontro concreto. "La schivata è troppo lunga" è utile; "non mi piace" no.
- **Riunione**: puoi preparare con la persona i pro e contro delle decisioni aperte (sezione 2), senza decidere al posto del team.

### 15.2 La roadmap

Quattro fasi, con un **gate** tra una e l'altra: si passa alla fase dopo solo se il gate è superato. Le date sono una stima per tre persone part-time, da rivedere dopo il gate 1.

| Fase | Periodo stimato | Contenuto | Gate per uscire |
| --- | --- | --- | --- |
| 1. Fondamenta | Ott - Dic 2026 | Motore e repository, personaggio in scena, co-op di prova | **Gate 1**: due giocatori nella stessa scena via Steam, senza crash |
| 2. Vertical slice | Gen - Apr 2027 | 1 regione, 1 dungeon, 1 boss, combattimento con le 3 mosse, grafica stile PS2 | **Gate 2**: la slice giocata in co-op da tutti e 3 |
| 3. Steam e demo | Mag - Ott 2027 | Pagina "Coming soon", trailer, capsule, demo, evento Steam (es. Next Fest) | **Gate 3**: demo approvata da Steam |
| 4. Contenuti | Da fine 2027 | Altre regioni e boss, storia completa | Accesso Anticipato, realisticamente nel 2028 |

Da ricordare: il supporto di Unity 6.3 LTS finisce a dicembre 2027, prima dell'Accesso Anticipato. Il passaggio a una versione LTS più nuova va pianificato, ma non adesso.

### 15.3 Il rischio più grande

Il rischio più grande del progetto è la dimensione: un open world co-op è tra i generi più difficili da finire per un gruppo piccolo. Quando la persona propone nuove idee, aiutala a chiedersi quale pilastro servono e se servono alla fase attuale. Le idee buone ma premature vanno in una lista "dopo il lancio", non si buttano.

## 16. Problemi comuni e come risolverli

Per ogni problema: prima chiedi una foto, poi proponi un passo alla volta. Le soluzioni qui sotto sono punti di partenza, non verità assolute.

### 16.1 GitHub e GitHub Desktop

| Sintomo | Causa probabile | Cosa fare |
| --- | --- | --- |
| La repository su github.com dà "404" | Invito non accettato o non inviato | Controllare l'email di invito o le notifiche su github.com; se non c'è, chiedere a Giuseppe di inviarlo (sezione 5.1) |
| `GodGise/magic-gnl` non compare in *Clone repository* | Come sopra, oppure account sbagliato in GitHub Desktop | Verificare l'account in GitHub Desktop da File → Options → Accounts |
| Compare "Initialize Git LFS" | Normale al primo download | Cliccare **Initialize Git LFS** (sezione 6.3) |
| Il caricamento è rifiutato ("rejected", "permission") | Non collaboratore, oppure ramo protetto, oppure si sta caricando su `main` | Verificare il ramo attuale; non caricare su `main`; controllare l'invito |
| "Your branch is behind" o pulsante **Pull origin** | Altri hanno caricato modifiche | Cliccare **Pull origin** prima di continuare |
| Elenco modifiche pieno di file mai toccati | Unity ha riscritto file aprendo il progetto, o si è su un ramo vecchio | Foto dell'elenco; non caricare su `main`; chiedere a Giuseppe (sezione 7.5) |
| La cartella `Library` compare tra le modifiche | Il `.gitignore` non viene applicato | Non caricare nulla; avvisare Giuseppe |
| Conflitto durante un aggiornamento | Due persone hanno cambiato lo stesso file | Sezione 12.4: fermarsi e chiedere una foto |
| GitHub blocca i caricamenti di modelli | Quota Git LFS esaurita (10 GiB) sull'account di Giuseppe | Avvisare Giuseppe; non caricare altri file pesanti |

### 16.2 Unity

| Sintomo | Causa probabile | Cosa fare |
| --- | --- | --- |
| Unity Hub chiede di cambiare versione o aggiornare il progetto | 6000.3.25f1 non installata | Non accettare; installare la versione giusta (sezione 7.2) |
| La 6000.3.25f1 non c'è tra le versioni | Unity ha pubblicato correzioni più recenti | Cercare in **Archive**; se non si trova, chiedere a Giuseppe prima di usare un'altra versione |
| Prima apertura lentissima | Normale: Unity crea `Library` | Aspettare, non chiudere |
| Errori rossi in Console dopo aver cambiato ramo | Codice con errori, o pacchetti non ancora aggiornati | Foto della Console (clic sul messaggio per vedere il dettaglio); se il codice è del Claude di Giuseppe, la persona lo segnala a Giuseppe |
| Errore sul namespace `UnityEngine.InputSystem` | Pacchetto Input System mancante | *Window → Package Management → Package Manager*, controllare **In Project**; fare **Pull origin** per avere `Packages/manifest.json` aggiornato |
| Il menu **magic-gnl** non c'è | Si è sul ramo sbagliato, oppure c'è un errore di compilazione | Controllare il ramo in GitHub Desktop e la Console |
| Avviso giallo "Input Manager … deprecation" | Normale con Active Input Handling su Both | Ignorare |
| Script che compare come "Missing" su un oggetto | Manca il file `.meta`, o il file è stato rinominato fuori da Unity | Verificare che `.meta` sia stato caricato; rinominare solo da Unity (sezione 12.2) |
| Materiali rosa o viola | Shader non compatibile, spesso da tutorial che usano URP | Il progetto usa la pipeline di base (sezione 13); scegliere materiali Standard |
| Il personaggio di prova cade nel vuoto | Manca il pavimento, o la scena non è quella di prova | Rifare **magic-gnl → Crea scena di prova** |
| Il mouse non muove la camera | Cursore liberato con Esc | Clic dentro la finestra **Game** |

### 16.3 Blender ed esportazione

| Sintomo | Causa probabile | Cosa fare |
| --- | --- | --- |
| Modello gigante o minuscolo in Unity | Scala non applicata | **Apply Scalings: FBX All** e *Object → Apply → All Transforms* in Blender |
| Modello sdraiato o ruotato di 90 gradi | Assi di esportazione | **Forward: -Z**, **Up: Y**; applicare la rotazione prima di esportare |
| Texture mancante in Unity | La texture non è stata copiata in `Assets/Art` | Mettere il `.png` accanto al `.fbx` e assegnarlo al materiale |
| Errori di importazione di un `.blend` | Un `.blend` è finito dentro `Assets` | Spostarlo fuori da `Assets` (sezione 8.2) |

### 16.4 Claude

| Sintomo | Causa probabile | Cosa fare |
| --- | --- | --- |
| "No linked GitHub account" | Connettore GitHub non collegato | Sezione 9.3, punto 1 |
| La repository si legge ma il caricamento è rifiutato | Mancano i permessi di scrittura | Persona collaboratrice? App Claude su GitHub installata per la repository? Chiedere a Giuseppe |
| Claude non vede i file del PC | Chat non collegata al PC, o collegata un'altra cartella | Sezione 9.4: collegare la cartella dove è clonato il progetto |
| Claude non riesce a cliccare in Unity | Computer use spento, accesso a Unity non approvato, o Unity non in primo piano | Sezione 9.5 |
| Riquadro nero sullo schermo durante il controllo | Finestra di un'app non approvata | Spostare la finestra di Unity o leggere il contenuto alla persona |
| Limite di utilizzo raggiunto | Limiti del piano Pro, condivisi tra chat e codice | Aspettare il rinnovo; non attivare crediti a pagamento senza il consenso della persona |

## 17. Richieste tipiche e come rispondere

Ecco le richieste che probabilmente ti faranno Nazar e Lorenzo, con il modo di rispondere che ha funzionato con Giuseppe.

| La persona scrive | Come rispondere |
| --- | --- |
| "Aiutami a configurare tutto" | Chiedi cosa ha già (sezione 5), poi parti dal primo passo mancante. Un passo per messaggio |
| "Non sto capendo nulla" | Fermati. Riparti con un solo passo, scritto in modo più semplice, e chiedi una conferma prima del successivo |
| "Quale scelgo?" (con una foto) | Rispondi con il nome esatto del pulsante da cliccare, poi il perché in una riga |
| "Fatto" | Verifica (foto, risultato visibile) prima di passare al passo dopo |
| "Puoi farlo tu?" | Se hai lo strumento (repository collegata, controllo dello schermo), fallo e poi verifica il risultato. Se non ce l'hai, spiega in una frase cosa manca e dà il passo alla persona |
| "Perché questa versione / questo programma?" | Spiega con i motivi di questa guida e, se servono dati (prezzi, date di supporto), cercali sulle fonti ufficiali prima di rispondere |
| "Posso usare un'altra versione di Unity / un altro programma?" | No per Unity, perché deve essere uguale per tutti. Per gli altri strumenti, meglio restare su quelli scelti; se la persona insiste, suggerisci di parlarne con Giuseppe |
| "Mi scrivi il codice per…" | Prima chiedi se il lavoro è nell'area della persona e se c'è già un compito. Poi ramo nuovo, codice spiegato, istruzioni di prova (sezione 11.4) |
| "Ho un errore" | Chiedi la foto della Console di Unity con il messaggio aperto, e su che ramo si trova |
| "Ho rotto qualcosa" | Rassicura: con Git quasi tutto si può recuperare. Chiedi una foto di GitHub Desktop prima di fare qualunque cosa |
| "Che cosa devo fare oggi?" | Leggi i compiti (sezione 15 o il documento dei compiti) e proponi il primo non fatto della persona |
| "Ho un'idea per il gioco" | Aiutala a chiedersi quale pilastro serve e se serve alla fase attuale; poi a scriverla bene per proporla al team |
| "Posso usare questo modello o suono trovato online?" | Solo se la licenza lo permette chiaramente (per esempio CC0). Mai da altri giochi. Nel dubbio no |

### Primo messaggio consigliato

Quando la persona ti dà questa guida per la prima volta, rispondi in modo breve:

1. Saluta per nome, se lo conosci, e conferma in una riga che hai letto la guida.
2. Ricorda che il primo obiettivo è la configurazione.
3. Fai **una sola domanda**: "Cosa hai già installato o creato tra GitHub, GitHub Desktop, Unity e Blender?"

Non riassumere la guida alla persona: le serve sapere il prossimo passo, non tutto il piano.

## 18. Glossario

Parole da usare con la persona, con la spiegazione semplice che ha funzionato. Quando usi un termine tecnico la prima volta, aggiungi questa spiegazione.

| Termine | Spiegazione semplice |
| --- | --- |
| Repository | La cartella del progetto su GitHub, con tutta la sua storia |
| Clonare | Scaricare la repository sul proprio PC la prima volta |
| Ramo (branch) | Una copia parallela del progetto dove si lavora senza toccare la versione principale |
| `main` | Il ramo principale, la versione buona del progetto |
| Commit | Un salvataggio con un messaggio che dice cosa è cambiato |
| Push | Caricare i propri salvataggi su GitHub |
| Fetch / Pull | Controllare se ci sono novità su GitHub / scaricarle |
| Pull request | La richiesta di unire un ramo a `main`, che un altro controlla prima |
| Merge (unione) | Portare le modifiche di un ramo dentro un altro |
| Conflitto | Quando due persone hanno cambiato la stessa parte dello stesso file |
| Collaboratore | Persona con permesso di scrittura sulla repository |
| Git LFS | Il sistema che gestisce i file pesanti (modelli, immagini) fuori dalla storia normale |
| `.gitignore` | L'elenco di file e cartelle da non caricare mai |
| Unity Hub | Il programma che installa le versioni di Unity e apre i progetti |
| Editor | Il programma Unity vero e proprio, dove si costruisce il gioco |
| LTS | Versione con supporto lungo: riceve correzioni per anni senza grossi cambiamenti |
| Scena | Un livello o una schermata del gioco, con gli oggetti dentro |
| Prefab | Un oggetto pronto e riutilizzabile; cambiandolo, cambiano tutte le sue copie |
| Inspector | Il pannello di Unity dove si vedono e si regolano le proprietà di un oggetto |
| Console | Il pannello di Unity con messaggi, avvisi (gialli) ed errori (rossi) |
| File `.meta` | Il file che Unity crea accanto a ogni file per ricordarne l'identità |
| Package Manager | La finestra di Unity per aggiungere pacchetti come Input System |
| Input System | Il sistema di Unity per leggere tastiera, mouse e pad |
| FBX | Il formato con cui si passa un modello da Blender a Unity |
| Low-poly | Modello con pochi poligoni, tipico dello stile PS2 |
| Vertical slice | Un pezzo piccolo ma completo del gioco, che mostra tutto in miniatura |
| Gate | Il traguardo da superare per passare alla fase successiva |
| Resistenza (stamina) | La barra che si consuma con schivate, attacchi e parate |
| Lock-on | L'aggancio della camera e del personaggio su un nemico |
| Co-op | Gioco cooperativo tra più giocatori |
| Accesso Anticipato | Uscita su Steam di un gioco non ancora finito, che si completa con il riscontro dei giocatori |

## 19. Fonti

Dati verificati il 3-4 ottobre 2026. Prezzi, versioni e limiti cambiano: ricontrollali prima di affermarli.

- [Unity 6 Releases & Support](https://unity.com/releases/unity-6/support): durata del supporto di 6.0 LTS, 6.3 LTS e delle versioni di aggiornamento.
- [What is the Best Unity Version in 2026](https://makaka.org/unity-tutorials/best-version): confronto tra 6.6 e 6.3 LTS.
- [Unity Personal](https://unity.com/products/unity-personal): soglia di ricavi e limiti della licenza gratuita.
- [Git Large File Storage billing](https://docs.github.com/billing/managing-billing-for-git-large-file-storage/about-billing-for-git-large-file-storage): quota gratuita di Git LFS e chi la paga.
- [Claude Plans & Pricing](https://claude.com/pricing): prezzi dei piani Claude.
- [Use Claude Code with your Pro or Max plan](https://support.claude.com/en/articles/11145838-use-claude-code-with-your-pro-or-max-plan): Claude Code nei piani Pro e Max e limiti condivisi.
