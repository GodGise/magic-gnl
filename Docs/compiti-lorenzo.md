# Compiti di Lorenzo: istruzioni per il suo Claude

Scritto dal Claude di Giuseppe il 4 ottobre 2026. Questo file è per te, Claude di Lorenzo: Lorenzo non deve leggerlo. Serve a farti guidare Lorenzo nei suoi compiti dal 5 al 9 ottobre 2026.

Prima di questo file leggi, se li hai, il `CLAUDE.md` della repository `GodGise/magic-gnl` e la guida `Docs/guida-claude.md`: contengono il progetto, le regole e la configurazione. Se qualcosa qui non coincide con il `CLAUDE.md`, vale il `CLAUDE.md`.

## 1. Chi è Lorenzo e come aiutarlo

- Lorenzo è **alle prime armi** con Unity e Git. Ha già configurato tutto: GitHub Desktop, Unity 6000.3.25f1 e il progetto aperto, e ha già visto la zona di prova.
- Il suo ruolo nel team è **level design e bilanciamento**. Costruisce le zone del gioco nell'editor di Unity e regola i numeri del combattimento. **Non scrive codice.**
- **Un passo alla volta.** Dai un'istruzione, aspetta "fatto" o una foto, poi la successiva. Con Giuseppe, cinque istruzioni insieme hanno prodotto un "non sto capendo nulla".
- Usa i nomi esatti dei pulsanti di Unity in grassetto (**Play**, **Inspector**, **Hierarchy**) e spiega in una riga a cosa servono.
- Chiedi una **foto dello schermo** appena qualcosa non torna.
- Fagli i complimenti quando un compito è finito, e segna lo stato nel documento "Compiti del team" (o ricordagli di farlo).
- Se Lorenzo vuole una cosa che richiede codice (un nuovo tipo di nemico, una porta che si apre, una trappola), **non scriverla tu**. Aiutalo a descriverla bene e a mandarla a Giuseppe: il codice lo fa il Claude di Giuseppe. Questo evita due Claude che modificano gli stessi script.

## 2. Regole da far rispettare

1. **La scena è sua.** `Assets/Scenes/ZonaProva.unity` appartiene a Lorenzo: è l'unico che la modifica.
2. **Mai usare il menu *magic-gnl → Crea zona di prova (villaggio in rovina)*.** Ricrea la scena da zero e cancella tutto il suo lavoro. Se Unity mostra la domanda "Ricrearla da capo?", la risposta è **Annulla**.
3. **Mai lavorare su `main`.** Tutto il suo lavoro va sul ramo `lorenzo/zona-1`.
4. **Non toccare** `Assets/Scripts`, `Assets/Editor` e `Assets/Art`: sono di Claude di Giuseppe e di Nazar.
5. **Salvare spesso** in Unity con **Ctrl+S**.
6. **Ogni sera** salva e carica il ramo da GitHub Desktop (sezione 4) e scrive nel gruppo cosa ha fatto.

## 3. Le basi di Unity da insegnargli il primo giorno

Insegnale solo quando servono, una alla volta, facendogliele provare subito.

| Cosa | Come |
| --- | --- |
| Aprire la zona | Pannello **Project** in basso, cartella **Scenes**, doppio clic su **ZonaProva** |
| Guardarsi intorno nella vista **Scene** | Tenere premuto il **tasto destro** del mouse e muoversi con **W A S D**; rotellina per avvicinarsi |
| Andare su un oggetto | Clic sull'oggetto nella **Hierarchy** (elenco a sinistra), poi **F** con il mouse sopra la vista Scene |
| Spostare, ruotare, ingrandire | Selezionare l'oggetto, poi **W** (sposta), **E** (ruota), **R** (scala) e trascinare le frecce colorate |
| Duplicare | **Ctrl+D** |
| Cancellare | **Canc** |
| Annullare | **Ctrl+Z** |
| Salvare | **Ctrl+S** |
| Provare il gioco | **Play** in alto al centro; di nuovo **Play** (o **Ctrl+P**) per fermare |

**Trappola importante:** le modifiche fatte mentre il gioco è in **Play** vengono **perse** quando si ferma il gioco. Si spostano oggetti e si salvano valori solo con il gioco fermo. Durante il Play si può sperimentare, ma i valori buoni vanno scritti su un foglio e reinseriti a gioco fermo.

Com'è fatta la zona nella Hierarchy: un oggetto **Zona** contiene i gruppi *Suolo*, *Sentiero*, *Piazza* (pozzo, torce, case in rovina), *Cimitero*, *Cripta*, *Bosco*, *Rocce*, *Confini* (muri invisibili sul bordo). Fuori da Zona ci sono *Giocatore*, *Cielo* (ciclo giorno e notte), *Sole*, *Luna* e i tre nemici: *Guardiano della piazza*, *Spettro del cimitero*, *Custode della cripta*.

Comandi di gioco: **WASD** movimento, **mouse** camera, **Spazio** schivata, **tasto sinistro** attacco, **tasto destro** parata, **clic della rotellina** aggancio del nemico, **rotellina** cambio nemico, **T** accelera il tempo, **F2** effetto PS2 sì/no, **Esc** libera il mouse.

## 4. Git per Lorenzo: tre gesti e basta

**Una volta sola, il primo giorno di modifiche (mercoledì):**
1. In GitHub Desktop, **Current branch** deve essere **main**. Clic su **Fetch origin**, poi su **Pull origin** se compare.
2. **Current branch → New branch**, nome `lorenzo/zona-1`, clic su **Create branch**.

**Ogni sera:**
1. In Unity **Ctrl+S**.
2. In GitHub Desktop controlla l'elenco dei file cambiati. Devono essere quasi solo `ZonaProva.unity` e qualche `.meta` o materiale. Se compaiono file in `Assets/Scripts` o `Assets/Editor`, fermati e chiedi a Giuseppe.
3. Nel campo **Summary** in basso a sinistra scrive cosa ha fatto (per esempio "Sposta le case della piazza"), poi **Commit to lorenzo/zona-1**.
4. **Publish branch** la prima volta, **Push origin** le volte dopo.
5. Scrive nel gruppo: "Ho caricato lorenzo/zona-1: ho fatto ...".

Se GitHub Desktop segnala un **conflitto**, fermatevi e chiedete a Giuseppe. Non scegliere a caso "usa la mia versione".

## 5. I compiti, giorno per giorno

### Lunedì 5 ottobre: giocare e annotare (30-45 minuti)

Obiettivo: conoscere la zona da giocatore.

1. Aprire **ZonaProva** e premere **Play**.
2. Giocare 30 minuti: seguire il sentiero, arrivare alla piazza, combattere il Guardiano, andare al cimitero e alla cripta, provare di notte e di giorno (tasto **T**).
3. Scrivere **5 problemi** (cose che non vanno, che annoiano o confondono) e **5 idee** (cose che renderebbero la zona più bella o più interessante).
4. Mandarli nel gruppo.

Come aiutarlo: fagli domande concrete ("Ti sei perso? Dove?", "Il combattimento era troppo facile o troppo difficile?", "C'è un posto dove volevi andare ma non potevi?"). Una buona nota dice cosa, dove e perché: "La cripta è vuota dentro, ci entro e non c'è niente" è utile; "brutto" no.

### Martedì 6 ottobre: la mappa su carta (1 ora)

Obiettivo: decidere la zona prima di toccarla.

1. Su un foglio, la zona vista dall'alto. Riferimenti: il giocatore parte a **sud**, la piazza è al **centro**, cimitero e cripta sono a **ovest** della piazza. La zona è un quadrato di 200 metri di lato.
2. Segnare:
    - il **percorso principale** dalla partenza all'obiettivo;
    - **un percorso segreto** o alternativo (per esempio un passaggio tra le case o nel bosco);
    - dove stanno i **nemici**;
    - dove sta l'**obiettivo**. Giuseppe deciderà martedì; la proposta è un frammento d'anima custodito in fondo alla cripta.
3. Foto della mappa nel gruppo, per avere l'ok di Giuseppe.

Consigli di level design da dargli, uno o due alla volta:
- Il giocatore va dove vede **luce**: le torce sono il modo più semplice per guidarlo.
- Un posto importante si vede **da lontano** (la cripta, una torre).
- Prima di un combattimento serve **spazio libero** per schivare.
- Una ricompensa nascosta premia chi esplora: un percorso segreto ha senso se porta a qualcosa.

### Mercoledì 7 ottobre: rifare la zona (2-3 ore)

Obiettivo: la zona come sulla mappa.

1. Creare il ramo `lorenzo/zona-1` (sezione 4).
2. Seguendo la mappa: spostare **case**, **torce**, **alberi**, **rocce** e **nemici** con lo strumento sposta (**W**). Si può duplicare (**Ctrl+D**) e cancellare (**Canc**).
3. Per nuovi muri o rovine: duplicare un muro esistente di una casa, oppure *GameObject → 3D Object → Cube* e trascinarci sopra un materiale da `Assets/Segnaposto/Materiali` (Pietra, PietraScura, Legno...).
4. Per una nuova torcia: duplicare una torcia esistente (ha già luce e fiamma).
5. Per un nuovo nemico: duplicare uno dei tre nemici. Nel componente **Bersaglio** dell'Inspector, la casella *Attacca Il Giocatore* decide se attacca.
6. Ogni tanto **Play** per provare, poi stop e **Ctrl+S**.
7. La sera: salvare e caricare (sezione 4).

Attenzione: non spostare i **Confini** e non cancellare *Giocatore*, *Cielo*, *Sole*, *Luna* e *Main Camera*.

### Giovedì 8 ottobre: l'interno della cripta (2-3 ore)

Obiettivo: la cripta diventa un luogo da esplorare, con un ingresso, un corridoio e una stanza finale dove andrà l'obiettivo.

La cripta oggi è una scatola di 7 x 7 metri con la porta a sud. Proposta semplice, senza scale:
1. Nella Hierarchy, *Zona → Cripta*: cancellare il muro **Retro**, così la cripta si apre verso nord.
2. Costruire dietro un **corridoio** largo 2-3 metri e lungo 10-15, con cubi e materiale **PietraScura**: due muri ai lati e un tetto.
3. In fondo, una **stanza** di 8 x 8 metri con il tetto, buia, con una o due torce duplicate.
4. Mettere un nemico nel corridoio o nella stanza (duplicando il *Custode della cripta*).
5. Lasciare libero il centro della stanza: lì Claude di Giuseppe metterà il frammento d'anima, consegnato come prefab.
6. Provare con **Play**: si entra, si percorre il corridoio, si arriva nella stanza? Il giocatore non deve rimanere incastrato.
7. La sera: salvare e caricare.

### Venerdì 9 ottobre: bilanciamento (1-2 ore)

Obiettivo: un combattimento impegnativo ma non frustrante, "alla Elden Ring ma accessibile".

1. Selezionare **Giocatore** nella Hierarchy. Nell'**Inspector** ci sono i componenti **Giocatore Controllo** e **Resistenza** con i numeri del combattimento.
2. Metodo: premere **Play**, cambiare **un numero alla volta**, provare contro un nemico, annotare l'effetto. Poi **stop** e reinserire a gioco fermo i valori buoni (in Play le modifiche si perdono).
3. Scrivere alla fine una tabella con i valori scelti e mandarla nel gruppo.

Valori di partenza (nell'Inspector i nomi compaiono separati da spazi, per esempio *Costo Schivata*):

| Componente | Valore | Partenza | Cosa cambia |
| --- | --- | --- | --- |
| Giocatore Controllo | Velocita Corsa | 5 | Velocità di corsa |
| Giocatore Controllo | Costo Schivata | 25 | Resistenza spesa per schivare |
| Giocatore Controllo | Durata Schivata | 0,45 s | Quanto dura la schivata |
| Giocatore Controllo | Distanza Schivata | 4 m | Quanto lontano porta |
| Giocatore Controllo | Invulnerabilita Schivata | 0,25 s | Per quanto i colpi non fanno danno |
| Giocatore Controllo | Costo Colpo Parato | 20 | Resistenza persa parando un colpo |
| Giocatore Controllo | Danno Assorbito In Parata | 0,9 | Quota di danno tolta dalla parata (0,9 = 90%) |
| Giocatore Controllo | Costo Attacco | 20 | Resistenza spesa per attaccare |
| Giocatore Controllo | Danno Attacco | 25 | Danno ai nemici (i nemici hanno 100 di vita) |
| Giocatore Controllo | Preparazione Attacco | 0,25 s | Attesa prima che il colpo parta |
| Giocatore Controllo | Recupero Attacco | 0,35 s | Tempo scoperto dopo il colpo |
| Resistenza | Massimo | 100 | Resistenza totale |
| Resistenza | Recupero Al Secondo | 30 | Velocità di ricarica |
| Resistenza | Ritardo Recupero | 0,8 s | Pausa prima della ricarica |

Anche i nemici si regolano dal loro componente **Bersaglio**: *Intervallo Attacchi* (3 s), *Preavviso* (0,7 s, il tempo in cui diventano rossi), *Danno Attacco* (20), *Portata Attacco* (2,5 m), *Vita Massima* (100).

Domande per guidarlo: "Quanti colpi servono per battere un nemico? Ti sembrano troppi?", "Riesci a schivare guardando quando diventa rosso?", "Quando finisci la resistenza, ti senti punito o ti sembra giusto?".

## 6. Quando qualcosa va storto

| Problema | Cosa fare |
| --- | --- |
| Ha perso modifiche fatte durante il Play | Normale in Unity: rifarle a gioco fermo |
| Ha cancellato qualcosa per sbaglio | **Ctrl+Z**. Se è passato tanto tempo, in GitHub Desktop clic destro sul file e *Discard changes* riporta la scena all'ultimo salvataggio caricato (si perde il lavoro non caricato: chiedi conferma prima) |
| Il giocatore cade nel vuoto | Un oggetto ha coperto o spostato il suolo, o è stato toccato un Confine: controllare *Suolo* e *Confini* |
| Errori rossi in Console | Foto del messaggio; probabilmente non dipende da lui: mandarla a Giuseppe |
| GitHub Desktop mostra file di `Assets/Scripts` tra le modifiche | Non caricarli; chiedere a Giuseppe |
| Unity chiede "Ricrearla da capo?" | **Annulla**: qualcuno ha cliccato il menu della zona di prova |
