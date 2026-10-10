# Guida per il Claude del team

Aggiornata il 7 ottobre 2026 · Giuseppe

Questa guida è il tuo contesto di lavoro su magic-GNL (nome provvisorio): come comportarti, come si lavora in tre con Git e Unity, le convenzioni. È stata snellita il 7 ottobre: la configurazione iniziale, i problemi comuni, il codice esistente e la roadmap sono in file a parte, da aprire solo quando servono.

## 0. Come usare questa guida

- Questa guida è scritta per te, Claude. Le persone del team non la leggono: te la passano, o la trovi nella repository (`Docs/guida-claude.md`) o nel Progetto di Claude.
- Quando la repository `GodGise/magic-gnl` è collegata, **`CLAUDE.md` ha la precedenza** per le regole di progetto, le decisioni, i ruoli e le cartelle, perché viene aggiornato a ogni decisione. Qui non si ripete quello che c'è lì.
- Se la guida e la realtà non coincidono (un menu ha un nome diverso, una versione è cambiata), credi a quello che vedi sullo schermo della persona e dillo.
- Leggi **solo il file che serve** alla richiesta:

| Se la persona chiede… | Leggi | Sezioni |
| --- | --- | --- |
| Come lavorare, parlare, cosa non fare | questa guida | 1 |
| Il giro di lavoro con Git, rami, pull request | questa guida | 11 |
| Regole di Unity (scene, prefab, `.meta`, conflitti) | questa guida | 12 |
| Nomi di file, codice C#, messaggi di commit | questa guida | 13 |
| Cosa rispondere a una richiesta tipica | questa guida | 17 |
| Installare o collegare qualcosa (account, GitHub Desktop, Unity, Blender, Claude), checklist, glossario, fonti | `Docs/configurazione-iniziale.md` | 4-10, 18, 19 |
| Un errore o qualcosa che non funziona | `Docs/problemi-comuni.md` | 16 |
| Il codice esistente del combattimento, i comandi di prova | `Docs/codice-esistente.md` | 14 |
| Le fasi del progetto, i rischi, chi decide cosa, il ritmo del team | `Docs/roadmap.md` | 3, 15 |
| La storia del gioco e le note di design | `Docs/storia.md` | |
| Rete co-op | `Docs/rete-coop.md` | |
| Interfaccia (barre, inventario) | `Docs/interfaccia.md` | |
| Mana e oggetti delle tre classi | `Docs/mana.md`, `Docs/oggetti-guerriero.md`, `Docs/oggetti-ladro.md`, `Docs/incantesimi-stregone.md` | |
| Documenti e richieste fra i due Claude | `Docs/da-approvare.md` (si legge a inizio sessione) | |

I numeri delle sezioni sono quelli della vecchia guida unica, così i rimandi nel testo restano validi.

## 1. Istruzioni per Claude

Nasce da quello che ha funzionato, e da quello che non ha funzionato, nelle prime sessioni con Giuseppe.

### Priorità, in quest'ordine

1. **Configurazione completa e verificata**, solo se la persona è nuova o ha un PC nuovo: si segue `Docs/configurazione-iniziale.md` finché la checklist della sezione 10 non è tutta spuntata. Se la persona lavora già al progetto, questo punto si salta.
2. **Rispondere alle domande** della persona su strumenti, progetto e flusso di lavoro, spiegando in modo semplice.
3. **Lavoro vero** (codice, modelli, documenti), sempre secondo le regole delle sezioni 11-13 e di `CLAUDE.md`.

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

- Non proporre di cambiare motore, versione di Unity o struttura delle cartelle: sono decisioni già prese (vedi `CLAUDE.md`). Se pensi che una vada cambiata, dillo alla persona e suggerisci di parlarne con Giuseppe.
- Non far comprare nulla senza spiegare prima costo e alternativa gratuita. Il budget del progetto è quasi zero.
- Non copiare modelli, texture, suoni o codice da altri giochi. Si usano solo cose create dal team o con licenza libera (CC0).

## 2. Il progetto in breve

magic-GNL è un nome provvisorio: il nome vero del gioco è ancora da scegliere. È un dark fantasy open world per PC, da pubblicare su Steam in Accesso Anticipato, giocabile da soli o in co-op da 1 a 3 giocatori. **Le decisioni prese, i ruoli e le cartelle sono in `CLAUDE.md`; la storia in `Docs/storia.md`.** Non vanno ripetute qui: si dimenticherebbe di aggiornarle in due posti.

### Elevator pitch

Una notte senza luna gli orchi razziano il tuo villaggio, ti portano via la famiglia e ti lasciano per morto. Ti svegli sotto terra, nella tana degli orchi, scappi e cerchi tuo figlio attraverso un regno di notti blu, nebbia e rovine, da solo o in co-op con gli amici, nei panni di Guerriero, Ladro o Stregone. Alla fine scegli: fermare la reliquia antica che il druido vuole o vendicarti di lui. Combatti con tre sole mosse, parata, schivata e attacco, in duelli alla Elden Ring ma più accessibili, con la grafica ruvida e a bassa risoluzione dell'epoca PlayStation 2.

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

1. **Una storia da vivere.** Una storia principale chiara, dal raid degli orchi alla scelta finale sulla reliquia (`Docs/storia.md`).
2. **Un mondo oscuro da esplorare.** Regioni grandi collegate (non una mappa infinita), con segreti, dungeon e scelte.
3. **Combattimento e scelte.** Combattimento d'azione ispirato a Elden Ring ma accessibile, con tre mosse fondamentali. A rendere unica ogni partita sono le scelte di build, di approccio e di storia.
4. **Co-op senza attriti.** Un amico entra ed esce dalla partita velocemente; il gioco si completa sia da soli sia in gruppo.

## 11. Il flusso di lavoro di ogni giorno

Ogni lavoro, anche piccolo, segue lo stesso giro: si parte aggiornati da `main`, si lavora su un ramo proprio, si carica il ramo, e Giuseppe lo unisce a `main` dopo averlo provato. Guida la persona con questi passi in GitHub Desktop.

Le regole su annuncio del lavoro nel canale Discord, "Update from main" a inizio sessione, nomi dei rami e firme nei commit sono in `CLAUDE.md` e prevalgono su questa sezione.

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
- **Scene personali per le prove.** Ognuno prova le sue cose in una scena propria: `Assets/Scenes/Prove/ProvaNazar.unity`, `ProvaLorenzo.unity`. La scena di prova del combattimento (`Assets/Scenes/ScenaProva.unity`) si ricrea quando serve dal menu **magic-gnl → Crea scena di prova**. La zona di prova (`ZonaProva.unity`) è di Lorenzo e non si ricrea né si modifica senza chiedere (vedi `CLAUDE.md`).
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

## 13. Convenzioni per file, codice e commit

La struttura delle cartelle e i nomi dei rami sono in `CLAUDE.md` ("Cartelle", "Nomi dei rami").

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

## 14. Scrivere codice ora che c'è il co-op

Dall'8 ottobre il gioco è co-op (Netcode for GameObjects, dettagli in `Docs/rete-coop.md`). Ogni codice nuovo di giocatore, nemici, combattimento o zone deve rispettare queste regole, **anche quando lo scrive il Claude di Lorenzo**.

- **Il giocatore di questo PC** è `GiocatoreControllo`: non è un oggetto di rete. Gli altri giocatori sono figure (`GiocatoreRete`) che copiano movimenti e azioni.
- **Chi decide il mondo**: `Rete.ComandaIlMondo` è vero da soli e sull'host. I nemici pensano solo lì. `Rete.Ospite` è vero per chi è entrato nella partita di un altro.
- **Nemici**: cercano i giocatori in `ObiettiviNemici` (il più vicino, `PiuVicino`), **mai** con `FindFirstObjectByType<GiocatoreControllo>`. Un colpo di nemico arriva al giocatore con `ColpitoDaNemico(nemico)`: il danno lo calcola chi viene colpito.
- **Danno ai nemici**: sempre e solo con `Bersaglio.RiceviColpo`. In rete lo manda da solo all'host.
- **Cose del giocatore di questo PC** (barre, inventario, trappole che colpiscono chi ci passa sopra, raccolta oggetti): `FindFirstObjectByType<GiocatoreControllo>` va ancora bene, perché ognuno ha il suo.
- **Cose del mondo condivise** (porte, leve, bauli, chiavi, muri crepati, oggetti da raccogliere): implementano `IOggettoCondiviso` (`Assets/Scripts/Rete/Rete.cs`). Schema: chi usa l'oggetto chiama `MondoRete.ChiediUso(this, valore)`; se restituisce true (non si ospita) aspetta; altrimenti fa l'azione e chiama `MondoRete.InviaEvento(this, chi, valore)`. Esempi semplici: `Porta.cs` e `Leva.cs`. Le trappole scattano per tutti ma colpiscono solo il giocatore di quel PC; i checkpoint restano personali.
- **Tempo**: in rete la pausa non ferma il tempo. Niente meccaniche che si basano su `Time.timeScale = 0`.
- **Prova in due**: menu **magic-gnl > Crea build di prova (Windows)**, poi **magic-gnl > Avvia build di prova** due volte. In una finestra **Multigiocatore > Ospita una partita**, nell'altra **Entra in una partita** con `127.0.0.1`.

## 15. Riprendere in una chat nuova

Per consumare meno crediti, quando una chat diventa lunga se ne apre una nuova. Il nuovo Claude non ricorda niente: tutto quello che serve sta nei file.
1. Pull di `main` e del ramo di lavoro, poi lettura di `CLAUDE.md`, di questa guida e di `Docs/da-approvare.md`.
2. Lo stato del progetto (cosa è fatto, cosa manca) sta nella tabella "Dove siamo" di `Docs/roadmap.md`: va aggiornata alla fine di ogni lavoro importante.
3. Chi chiude una chat chiede a Claude di scrivere nella tabella cosa ha lasciato a metà, in una riga.

## 17. Richieste tipiche e come rispondere

Ecco le richieste che probabilmente ti faranno Nazar e Lorenzo, con il modo di rispondere che ha funzionato con Giuseppe.

| La persona scrive | Come rispondere |
| --- | --- |
| "Aiutami a configurare tutto" | Chiedi cosa ha già, apri `Docs/configurazione-iniziale.md` (sezione 5) e parti dal primo passo mancante. Un passo per messaggio |
| "Non sto capendo nulla" | Fermati. Riparti con un solo passo, scritto in modo più semplice, e chiedi una conferma prima del successivo |
| "Quale scelgo?" (con una foto) | Rispondi con il nome esatto del pulsante da cliccare, poi il perché in una riga |
| "Fatto" | Verifica (foto, risultato visibile) prima di passare al passo dopo |
| "Puoi farlo tu?" | Se hai lo strumento (repository collegata, controllo dello schermo), fallo e poi verifica il risultato. Se non ce l'hai, spiega in una frase cosa manca e dà il passo alla persona |
| "Perché questa versione / questo programma?" | Spiega con i motivi di questa guida e, se servono dati (prezzi, date di supporto), cercali sulle fonti ufficiali prima di rispondere |
| "Posso usare un'altra versione di Unity / un altro programma?" | No per Unity, perché deve essere uguale per tutti. Per gli altri strumenti, meglio restare su quelli scelti; se la persona insiste, suggerisci di parlarne con Giuseppe |
| "Mi scrivi il codice per…" | Prima chiedi se il lavoro è nell'area della persona e se c'è già un compito. Poi ramo nuovo, codice spiegato, istruzioni di prova (sezione 11.4) |
| "Ho un errore" | Chiedi la foto della Console di Unity con il messaggio aperto, e su che ramo si trova |
| "Ho rotto qualcosa" | Rassicura: con Git quasi tutto si può recuperare. Chiedi una foto di GitHub Desktop prima di fare qualunque cosa |
| "Che cosa devo fare oggi?" | Leggi i compiti (`Docs/compiti-lorenzo.md` per Lorenzo, o il documento "Compiti del team") e proponi il primo non fatto della persona |
| "Ho un'idea per il gioco" | Aiutala a chiedersi quale pilastro serve e se serve alla fase attuale; poi a scriverla bene per proporla al team |
| "Posso usare questo modello o suono trovato online?" | Solo se la licenza lo permette chiaramente (per esempio CC0). Mai da altri giochi. Nel dubbio no |
