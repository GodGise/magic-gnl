# Problemi comuni e come risolverli

Da leggere solo quando qualcosa non funziona (GitHub, Unity, Blender, Claude). I numeri delle sezioni rimandano alla vecchia guida: 5-10 sono in `Docs/configurazione-iniziale.md`, 11-13 in `Docs/guida-claude.md`.

## Problemi comuni e come risolverli

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
