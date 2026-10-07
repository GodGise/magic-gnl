# Configurazione iniziale e glossario

Per chi si aggiunge al team (o per chi ha un PC nuovo). **Non serve a ogni sessione**: il Claude di chi ha già tutto installato non lo legge.
Contiene: mappa degli strumenti (4), configurazione passo per passo (5-9), checklist finale (10), primo messaggio consigliato, glossario e fonti.

I numeri delle sezioni sono quelli della vecchia guida unica, così i rimandi nel testo ("sezione 7.4") restano validi. Le sezioni 1, 2, 11, 12, 13 e 17 sono in `Docs/guida-claude.md`; il codice esistente in `Docs/codice-esistente.md`; i problemi comuni in `Docs/problemi-comuni.md`; la roadmap in `Docs/roadmap.md`.
Scritto il 4 ottobre 2026: prezzi, versioni e nomi dei menu possono essere cambiati.

Nota del 7 ottobre: la repository `GodGise/magic-gnl` risulta **pubblica** (si scarica anche senza account), non privata come scritto più sotto. Per scrivere servono comunque l'invito come collaboratore e l'account GitHub.

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
