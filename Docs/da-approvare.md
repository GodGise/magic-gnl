# Da approvare: il "ponte" fra il Claude di Giuseppe e quello di Lorenzo

Questo file serve a passarsi messaggi e documenti fra i due Claude, senza una chat diretta (non esiste). Passa dal flusso normale dei rami e delle pull request, quindi Giuseppe resta quello che controlla.

## Come si usa

- **Il Claude di Giuseppe** scrive qui sotto, nella sezione "Per Lorenzo", cosa c'è da leggere, cosa si chiede e cosa serve come risposta. Il file arriva a Lorenzo con la pull request unita a `main`.
- **Il Claude di Lorenzo**, all'inizio di ogni sessione (dopo il Pull di `main`), legge questo file e dice a Lorenzo cosa c'è di nuovo.
- **Lorenzo risponde** nella sezione "Risposte di Lorenzo", sul suo ramo `lorenzoc/...`. La risposta arriva a Giuseppe con la sua pull request.
- Quando una voce è chiusa (approvata, modificata o rifiutata) la si sposta nella sezione "Chiuso", con la data e l'esito. Così il file resta corto.

## Per Lorenzo

### 12 ottobre: risposte di Giuseppe su prologo, tempo e co-op (da leggere)
- **Quest obbligatorie (tutte e 6) e tempo a tappe:** approvato.
- **Co-op dalla gattabuia:** approvato, **con un'aggiunta di Giuseppe**: prima di giocare in co-op bisogna aver finito il prologo/tutorial, quindi **un amico non può entrare con un personaggio nuovo** (risposta alla tua domanda). Fatto nel menu (ramo `giuseppec/coop-dopo-prologo`): `DatiSlot.prologoFinito`; in Multigiocatore si scelgono solo personaggi che l'hanno finito; nell'editor e nelle build di sviluppo è tutto libero per le prove.
- **Per te:** al risveglio in gattabuia chiama `Salvataggio.SegnaPrologoFinito()` (una volta, per il personaggio attivo). Finché non c'è, in una build per i giocatori il co-op resta chiuso.
- **Testi più leggibili:** fatto (ramo `giuseppec/testi-leggibili`): nuovo carattere Alegreya Sans, più grande, per dialoghi, messaggi, azioni, obiettivo e ora. Ho cambiato lo stile in `Missioni.cs` (una riga) e la bussola, quando la unisci, può usare `GraficaMenu.EtichettaGioco`.

### 13 ottobre, sera: decisioni di Giuseppe su monete e gattabuia (da leggere)
- **Monete:** non "Lacrime del lago" (troppo lontano da D&D): si chiamano **monete d'oro**, nel parlato e nelle interfacce anche solo **"monete"** ("45 monete", "1 moneta"). Cambiati i testi `valuta.*`, `neg.poche_monete`, `tip.q4.bottega` e la battuta di Hobb in `Lingua.cs` (8 lingue), `Docs/monete.md` e `CLAUDE.md`. Il codice è lo stesso. **Lorenzo:** se hai testi nuovi sul negozio, usa "monete", non "lacrime".
- **Gattabuia dopo la fuga:** **crolla** (già deciso l'11 ottobre) e **non può diventare il campo base**: le tane degli orchi sono collegate da un enorme sistema di caverne, un campo base lì richiamerebbe altri orchi. Il punto aperto del 10 ottobre è chiuso. Il riposo resta ai falò e ai checkpoint.
- **Gancio per la storia:** le due tane degli orchi e la tana più grande sono collegate sotto terra dalle stesse caverne; si può usare nel capitolo 2 (passaggi segreti, orchi che arrivano da sotto).

### 11 ottobre: manca il `.meta` di `Assets/Editor/CreaOggettiComuni.cs` (promemoria)
Lo script era su `main` senza il suo `.meta`, e il Unity di Giuseppe ne ha creato uno. Il Claude di Giuseppe ha aggiunto il `.meta` (ramo `giuseppec/meta-oggetti`). **Regola di `CLAUDE.md`: quando si crea uno script (o una cartella) si crea nello stesso commit anche il suo `.meta`** (`fileFormatVersion: 2` e un `guid` di 32 caratteri esadecimali). Il tuo Claude lo ricordi a ogni nuovo script. Prima del Pull, se GitHub Desktop mostra un `CreaOggettiComuni.cs.meta` fra le tue modifiche, scartalo (clic destro → *Discard changes*), così usi quello di `main`.

### 11 ottobre: sistema dei dialoghi (solo informazione, ramo `giuseppec/sistema-dialoghi`)
Il Claude di Giuseppe ha scritto il sistema dei dialoghi: `Assets/Scripts/Interfaccia/Dialogo.cs` (riquadro in basso, testo lettera per lettera, tasto Interagisci per andare avanti) e `Assets/Scripts/Livelli/Interlocutore.cs` (componente da mettere sui personaggi: "E  Parla: Maren"; righe, righe ripetute e un evento "Al Termine" per avviare quest). Le righe sono chiavi di `Lingua`, con `{Interagisci}`, `{Attacca}` ecc. al posto dei tasti. Mentre si parla il personaggio sta fermo (toccati `GiocatoreControllo.cs`, `MagiaStregone.cs`, `AggancioBersaglio.cs` con un controllo in più). **Fai Pull di `main` prima di toccare quei file.** In co-op ognuno parla per conto suo. Per provarlo: oggetto vuoto vicino al giocatore, componente `Interlocutore`, `Nome Chiave` = `pg.prova`, due righe con parlante `pg.prova` e testo `dlg.prova.1` e `dlg.prova.2`.

### 11 ottobre: risposte di Giuseppe (da leggere)
- **Confermato:** in co-op la famiglia è dell'host; per ora tutti i personaggi giocabili sono uomini.
- **Gattabuia dopo la fuga:** **crolla** (vendetta contro gli orchi). Niente campo base lì.
- **Orco della piazza:** va bene la tua versione (scontro normale, invulnerabile dopo 3 minuti, se muori ti svegli in gattabuia, il miniboss torna più avanti).
- **Regole anti-combo co-op:** il Claude di Giuseppe ha controllato il codice di rete, nessun problema. Va provato con due istanze (uno cade, l'altro rialza; poi cadono entrambi davanti a un boss).
- **Scena `VillaggioLagoNero.unity`:** è tua, Giuseppe sta finendo le impostazioni.
- **Dialoghi e sonoro:** li fa Giuseppe; i dialoghi li hai già rivisti tu.
- **Obiettivo:** una demo su Steam con il capitolo 1 (o un po' meno). Giorno 45 minuti, notte 50.
- **Aperte:** lunghezza della fuga dalla gattabuia e della demo.

### 10 ottobre, sera: Giuseppe ti affida la revisione dei dialoghi del capitolo 1 (da fare tu)
Giuseppe aspetta la tua approvazione e ti **delega la revisione**: leggi `Docs/dialoghi-capitolo-1.md` (e `Docs/quest-capitolo-1.md`) e **modifica direttamente** sul tuo ramo `lorenzoc/dialoghi-capitolo-1`.
- Puoi cambiare battute, ordine, tono, tagliare o aggiungere righe (resta nel budget di 80-100 righe; la quest 5 si taglia per prima se serve).
- **Già deciso da Giuseppe, non cambiare:** i nomi (Maren, Tobin, Brannoc, Ilse, Edric, Hobb); Guerriero e Ladro con la stessa quest 2 (lama o arco), lo Stregone con lo studio delle magie; durante la razzia l'eroe prende un'ascia dal villaggio; la boss fight con l'orco enorme si perde; il capitolo finisce dopo la fuga dalla gattabuia.
- Quando hai finito, scrivi nelle "Risposte di Lorenzo" cosa hai cambiato e apri la pull request: Giuseppe approva e il suo Claude fa le 8 traduzioni e il codice del dialogo.
- Non iniziare il codice del sistema di dialogo: lo scrive il Claude di Giuseppe dopo la tua revisione.

### 10 ottobre, pomeriggio: scaletta del capitolo 1 (da leggere, serve il tuo parere)
Giuseppe vuole far uscire il capitolo 1: esplorazione di Villaggio Lago Nero e, di notte, la razzia degli orchi. Il suo Claude ha scritto la scaletta in `Docs/capitolo-1.md`: stato attuale, 5 traguardi in ordine e chi fa cosa. Leggila tutta, poi rispondi qui sotto.
- **Il capitolo 1 (deciso da Giuseppe)**: giorno al villaggio con tutorial e quest, notte e razzia, boss fight con l'orco enorme che si perde per forza, gattabuia e fuga. Vedi `Docs/storia.md`.
- **Tocca a te (level design e bilanciamento):** il percorso di esplorazione di giorno, la razzia a ondate, la gattabuia (cella, sotterranei, grotte), il secondo tipo di nemico, i numeri dell'orco enorme. Il primo traguardo è tutto "in grigio", con forme provvisorie.
- **Domande per te:** (1) il percorso di 20-30 minuti ti sembra giusto? (2) la razzia a ondate, come te la immagini? (3) per lo scontro con l'orco enorme (da perdere) come lo immagini? e la gattabuia, come la costruiresti? (4) la scena `VillaggioLagoNero.unity` è tua come la zona di prova, oppure la tiene Giuseppe?
- Le decisioni sulle 6 domande in cima al documento le prende Giuseppe: non iniziare lavori grandi prima che le abbia risposte.

### 10 ottobre, notte: la difficoltà cresce con i giocatori (serve la tua occhiata ai numeri)
Giuseppe ha deciso: classi libere (la tua proposta "una classe per giocatore" è chiusa, vedi sotto) e più giocatori = nemici più forti. Il suo Claude ha programmato il meccanismo (ramo `giuseppec/difficolta-coop`, script `DifficoltaCoop.cs`, spiegazione in `Docs/rete-coop.md`). **I numeri sono provvisori e li decidi tu**: per ogni numero di giocatori si regolano dall'Inspector vita dei nemici, vita dei boss, danno dei nemici e frequenza degli attacchi.
- Partenza: 2 giocatori = vita x2,5, danno x1,5, attacchi x1,25; 3 giocatori = vita x4, danno x2, attacchi x1,5.
- Per fissarli nella scena: componente `DifficoltaCoop` su un oggetto vuoto (sceglie lui la scena, area tua).
- Toccati `Bersaglio.cs` (`VitaMassima`, `DannoAttacco`, intervalli) e `MondoRete.cs`. **Fai Pull di `main` prima di toccare `Bersaglio.cs`.**
- **Domanda:** il numero di nemici per gruppo non cambia (i gruppi sono nelle scene). Vuoi nemici extra che compaiono solo in co-op?

### 8 ottobre: co-op, combattimento e nemici in rete (serve una risposta)
Giuseppe ha deciso di portare subito in rete anche combattimento e nemici (ramo `giuseppec/rete-coop`, spiegazione in `Docs/rete-coop.md`, sezione "Come funziona adesso"). Per farlo sono cambiati questi file di gioco: `GiocatoreControllo.cs`, `Bersaglio.cs`, `InseguimentoNemico.cs`, `AnimazioneUmanoide.cs`, `SferaMagica.cs`. Da soli il gioco funziona come prima.
1. **Finché il ramo non è unito a `main`, non modificare quei cinque file** (si creerebbero conflitti). Se ci sono modifiche in corso su quei file, scriverlo nelle risposte.
2. Regole nuove per il codice (tutte in `Docs/guida-claude.md`, sezione 14): i nemici cercano i giocatori in `ObiettiviNemici` (non più con `FindFirstObjectByType<GiocatoreControllo>`); il danno ai nemici passa sempre da `Bersaglio.RiceviColpo`; `Rete.ComandaIlMondo` dice se questo PC decide il mondo (vero anche da soli).
3. Porte, leve, bauli, chiavi, muri crepati, trappole e checkpoint (`Assets/Scripts/Livelli/`, area di Lorenzo) per ora **non sono condivisi**: ogni giocatore ha la sua copia. **Domanda:** quali vanno condivisi per primi? Proposta: porte e leve (se uno apre, è aperto per tutti), poi bauli e oggetti.

### 8 ottobre, sera: porte, leve e bauli ora sono condivisi (solo informazione)
Giuseppe ha deciso di non aspettare e il suo Claude ha reso co-op gli script di `Assets/Scripts/Livelli/` (ramo `giuseppec/livelli-coop`): `Porta`, `Leva`, `Baule`, `Chiave`, `Serratura`, `MuroFragile`, `Checkpoint`, `TrappolaSpuntoni`. Il funzionamento da soli è lo stesso di prima; le scritte "E  Tira la leva" e simili ora usano il riquadro delle azioni dell'interfaccia e sono tradotte. Regole in `Docs/rete-coop.md` e `Docs/guida-claude.md` (sezione 14).
- Prima di modificare quegli script, fare Pull di `main` dopo l'unione del ramo.
- Ogni oggetto nuovo che cambia lo stato del mondo va fatto con lo stesso schema (`IOggettoCondiviso`, esempio `Porta.cs`).
- La domanda di prima ("quali condividere per primi") è chiusa: fatti tutti. Se Lorenzo vuole regole diverse (per esempio checkpoint uguali per tutti), lo scriva qui.

## Risposte di Lorenzo

### 13 ottobre, notte: segnalini delle pozioni, taverna a parte e razzia in due ondate (serve un'occhiata di Giuseppe)
Richiesta di Lorenzo, ramo `lorenzoc/razzia` (parte da `lorenzoc/quest-5`). Non ancora provato in Unity.
- **Pozioni:** niente più conto alla rovescia. Accanto alla barra della vita un segnalino per ogni effetto (scudo viola = danni subiti in meno, goccia blu = mana massimo in più); i puntini sopra dicono quante pozioni sono sommate, la riga sotto si svuota col tempo, negli ultimi 20 s lampeggia (`HudGioco.cs`, `EffettiPozioni.ParteRimasta`).
- **Quest 6 in una scena a parte, come la cripta:** `Assets/Scenes/Taverna.unity` (comando "Taverna: crea la scena della taverna"). Finite le prime 5 quest, al tramonto: "Al tramonto, raggiungi la taverna di Hobb", E sulla porta. Dentro: Hobb (dlg.q6.01-03), entra Maren con Tobin (04-05), tamburi lontani, Hobb (06), Maren (07), poi si può uscire. Uscendo è già notte (`OrarioPrologo.NotteSubito`) e la razzia è già cominciata: niente orchi che compaiono dal nulla.
- **Razzia** (`RazziaVillaggio`, comando "Prologo: prepara la quest 6 e la razzia"): prima ondata di 10 orchi sparsi davanti alle case, che le distruggono (`RazziatoreOrco`: colpi alle pareti, porte sfondate, calci, fuoco con la torcia; quando la casa è finita passano a quella vicina). Smettono e attaccano quando il giocatore entra nel loro campo visivo; spalle alla strada, si possono giustiziare alle spalle. Al 65% abbattuto gli altri scappano; 1 minuto di tamburi e urla sempre più forti; seconda ondata di 16 (5 **orchi corazzati con l'ascia orchesca**: vita 240, armatura 45, danno 34) che entra correndo da fuori vista. Al 70% abbattuto, appena il giocatore è a 20 m dalla piazza: terremoto e **orco enorme** che salta giù in piazza (vita 4000, danno 55, 4,6 m). Una dozzina di case brucia già all'uscita dalla taverna, altre sono rovinate o crollate (`CasaInRovina`).
- **Suoni nuovi** fatti dal codice: tamburo, urlo d'orco, rombo, legno spaccato, crepitio del fuoco.
- **Da decidere dopo:** cosa succede quando l'orco enorme vince (regole già scritte: invulnerabile dopo 3 minuti, poi risveglio in gattabuia). Per ora, morendo, si rinasce davanti alla taverna e lo scontro ricomincia.
- **Giuseppe:** tocca file non solo di Lorenzo: `InseguimentoNemico.cs` (posto che cambia, ritirata), `AnimazioneUmanoide.cs` (gesti dei razziatori), `AspettoUmanoide.cs` (ascia), `CameraTerzaPersona.cs` (camera che trema), `Suoni.cs`. Nessun file della rete.
- **Per Nazar:** case danneggiate, crollate, porte sfondate, texture di fuoco e fumo, orco corazzato con ascia, animazioni della razzia: aggiunti in `Docs/lista-modelli-nazar.txt` (43b-43g, 71b, 72).

### 13 ottobre: quest 5, la cripta della prima guardia in una scena a parte (serve un'occhiata di Giuseppe)
Ramo `lorenzoc/quest-5` (parte da `lorenzoc/negozio-ilse`: va unito dopo la #79). Decisioni di Lorenzo: la cripta è una **scena a parte** (`Assets/Scenes/Cripta.unity`, aggiunta alle Build Settings) per non caricare troppo il mondo. Dal mausoleo del cimitero "E  Entra nella cripta" → schermata di caricamento → dentro la cripta. La scena si carica in aggiunta al villaggio (che si nasconde), così zaino, monete e quest restano; uscendo si torna al cimitero.
- **Cripta:** ingresso vuoto, poi corridoi con **4 scheletri viventi** (45 vita, 12 danno, un colpo ogni 1,3 s; tre colpi a turno: fendente, rovescio, calata; dopo ogni colpo si riposizionano), cancello chiuso, leva in una stanza laterale, sala della prima guardia con il baule. Nel baule il **Medaglione della prima guardia** (amuleto per tutte le classi: vita massima +5%, +2 vita per nemico ucciso).
- **Quest:** fine quest 4 → Edric racconta la leggenda → cripta → medaglione → "Porta il medaglione a Edric" → quest 5 finita. Comandi "Cripta: crea la scena della cripta" e "Prologo: prepara la quest 5".
- **Provato in Unity** dal Claude di Lorenzo: caricamento, scheletri che attaccano, baule, uscita e ritorno al cimitero, Console senza errori. **Corretto provando:** Edric stava nel cortile di studio che c'è solo per lo Stregone, quindi Guerriero e Ladro non potevano finire la quest; ora Edric c'è per tutti (`ComponentiPerClasse` tiene il suo dialogo della quest 2 solo per lo Stregone). Cripta schiarita (prima gli scheletri non si vedevano).
- **File di tutti toccati:** `Bersaglio.cs`, `GiocatoreControllo.cs`, `CameraTerzaPersona.cs`, `Torcia.cs`, `Baule.cs`, `Leva.cs`, `UsoConsumabili.cs`, `InventarioGioco.cs` (nomi lunghi su una riga), `Lingua.cs`, `ProjectSettings/EditorBuildSettings.asset` (scena Cripta).

### 13 ottobre: icone provvisorie per tutti gli oggetti, prove fatte in Unity (serve un'occhiata di Giuseppe)
Stesso ramo `lorenzoc/negozio-ilse`. Nell'inventario gli oggetti si vedevano solo come iniziali. Ora ogni tipo ha una sagoma disegnata (spada, spadone, ascia, mazza, pugnali, archi, balestra, scudi per taglia, armature per peso, vesti, amuleti, bastoni, libri, incantesimi, pozioni, fiori) e ogni oggetto ha un dettaglio di colore suo (dal nome). Spariscono da sole quando un oggetto ha la sua icona vera (`DatiOggetto.icona`).
- **File nuovo:** `Assets/Scripts/Interfaccia/IconeProvvisorie.cs`. **Toccati:** `GraficaMenu.cs` (`Icona` chiama le sagome prima dell'iniziale), `FormeOggetti.cs` (`ColoreMagia` pubblico).
- Bottega, monete, pozioni, tasto C e Stregone **provati in Unity dal Claude di Lorenzo** sul PC di Lorenzo, Console senza errori; scena `VillaggioLagoNero` già preparata sul ramo.

### 13 ottobre: tasto C per cambiare consumabile, regola delle pozioni cumulabili (serve un'occhiata di Giuseppe)
Stesso ramo `lorenzoc/negozio-ilse`. Decisioni di Lorenzo:
- **Cambia oggetto**: azione nuova `CambiaOggetto` in `Comandi.cs`, tasto **C** (Y / Triangolo sul pad), passa al consumabile dopo; `MenuComandi.cs` ora ha 14 azioni (7 per colonna). Testi in 8 lingue.
- **Cumulabili:** solo Tonico e Infuso, e solo negli effetti: tre Tonici = -30%, ogni pozione fa ripartire i 3 minuti di tutta la somma. Balsamo ed Elisir non si sommano (conta il più forte). Balsamo: 100 di vita in 7 s (prima 4).
- **Toccati:** `Comandi.cs`, `MenuComandi.cs`, `UsoConsumabili.cs`, `EffettiPozioni.cs`, `DatiConsumabile.cs`, `Lingua.cs`, file del Balsamo e dell'Elisir, `CreaErbe.cs`.

### 13 ottobre: effetti delle pozioni rivisti e massimo 3 pozioni (serve un'occhiata di Giuseppe)
Stesso ramo `lorenzoc/negozio-ilse`. Decisioni di Lorenzo:
- **Cardo nero** (fiore dei sassi): danni subiti **-10% per 3 minuti**, non più resistenza. **Lunaria**: mana massimo **+5% per 3 minuti**, non più mana (si ricarica da solo). Effetti **cumulabili**, ognuno con il suo tempo; a schermo sopra il rombo della pozione. Morendo finiscono.
- **Al massimo 3 pozioni** nello zaino all'inizio del gioco: la bottega non ne dà altre, quelle a terra restano lì.
- Fiori, pozioni e le 13 voci della bottega **esistono già come file** in `Assets/Dati/` (prima li creava solo il comando).
- **File nuovo:** `EffettiPozioni.cs`. **Toccati (di tutti):** `GiocatoreControllo.cs` (danni ricevuti e mana massimo passano dagli effetti; a ogni rinascita si azzerano), `Zaino.cs` (`MassimoConsumabili`), `OggettoRaccoglibile.cs`, `InventarioGioco.cs`, `UsoConsumabili.cs`, `DatiConsumabile.cs`, `DatiRicetta.cs`, `Negozio.cs`, `Lingua.cs`.

### 13 ottobre: nome delle monete deciso, e ringraziamenti a quest finita (solo informazione)
Stesso ramo `lorenzoc/negozio-ilse`.
- **Monete:** Lorenzo ha scelto **Lacrime del lago**; nel parlato e nelle interfacce basta "lacrime".
- **Ringraziamenti:** Hobb, a ratti morti, chiedeva ancora di ucciderli. Ora a quest finita Maren, Brannoc, Edric e Hobb ringraziano con una battuta coerente (anche diversa per classe; battute in `Docs/dialoghi-capitolo-1.md`, sezione "Dopo le quest", 8 lingue). `Interlocutore` ha "Righe Dopo La Quest" e `QuestCompiuta()`; comando nuovo "magic-gnl > Prologo: ringraziamenti dopo le quest" (`CreaRingraziamenti.cs`), richiamato anche dai comandi delle quest 1-4. **Toccati:** `Interlocutore.cs`, `Lingua.cs`, `CreaPrologoQuest1-4.cs` (una riga ciascuno).

### 13 ottobre: monete (Lacrime del lago) e mercato (serve un'occhiata di Giuseppe, e un parere sul nome)
Stesso ramo `lorenzoc/negozio-ilse`. Lorenzo ha chiesto monete legate alla storia, niente argento. Scelta provvisoria: **Lacrime del lago**, sassolini neri come vetro che il lago lascia sulla riva dopo le tempeste. Altre 4 proposte (Scaglie nere, Chiodi di guardia, Segni di veglia, Gocce di pece) in `Docs/monete.md`: **(Deciso da Giuseppe il 13 ottobre sera: monete d'oro, vedi sopra)**; cambiare nome vuol dire cambiare solo i testi `valuta.*` in `Lingua.cs`.
- **Mercato:** ogni mercante vende le sue cose (tabella in `Docs/monete.md`). Fatto solo Ilse: nella bottega tre schede, **Baratto** (fiori per pozioni, il più conveniente), **Compra** (pozioni con le Lacrime, circa 5 volte più caro), **Vendi** (fiori per Lacrime). Prezzi provvisori nello stesso documento.
- **Da dove arrivano:** 15 all'inizio, 20 da Hobb alla fine della quest 3, vendendo i fiori. Le Lacrime si vedono nell'inventario (accanto a "Zaino") e nella bottega.
- **File nuovi:** `Borsa.cs` (le monete del giocatore: un numero, non oggetti dello zaino), `DaiMonete.cs` (ricompense), `Docs/monete.md`. **Toccati:** `DatiRicetta.cs` (prezzo e ricavo), `Negozio.cs` (schede e borsa), `InventarioGioco.cs` (le Lacrime accanto allo zaino), `CreaErbe.cs`, `CreaPrologoQuest4.cs` (paga di Hobb), `Lingua.cs`, `CLAUDE.md`.
- Le monete non si salvano ancora (come lo zaino).

### 13 ottobre: bottega di Ilse, fiori e pozioni (serve un'occhiata di Giuseppe)
Ramo `lorenzoc/negozio-ilse` (parte da `lorenzoc/stamina-esplorazione`: va unito dopo). Decisione di Lorenzo: l'erboristeria di Ilse è un **negozio di baratto** (fiori per pozioni) e **ogni fiore ha uno scopo**: Fiore di riva = vita, Cardo nero = resistenza, Lunaria = mana. Cinque ricette, anche con fiori combinati (tabella in `Docs/erbe-e-pozioni.md`). Nella quest 4 Ilse, quando hai i 3 fiori, apre la bottega e sei tu a scambiarli con 2 Pozioni di Ilse; il primo scambio fa passare a "Bevi una pozione". Le monete non ci sono ancora: quando arriveranno saranno un oggetto da mettere nelle ricette, il negozio non cambia.
- **File nuovi:** `Negozio.cs` (finestra della bottega, serve a qualsiasi mercante), `DatiRicetta.cs`, `Assets/Editor/CreaErbe.cs` (menu "magic-gnl > Erbe: crea fiori, pozioni e ricette"), `Docs/erbe-e-pozioni.md`.
- **File condivisi toccati:** `InventarioGioco.cs` (`Aperto` è vero anche con la bottega aperta, così E, F e Tab non fanno altro; dettaglio delle pozioni con mana e resistenza), `DatiConsumabile.cs` (mana, resistenza, ricarica più veloce per un po'), `Resistenza.cs` (`Recupera`, `Potenzia`), `UsoConsumabili.cs`, `GiocatoreControllo.cs` (`RidaiMana`), `Lingua.cs` (testi della bottega, 2 fiori e 4 pozioni nuove; `dlg.q4.04` riscritta), `CreaPrologoQuest4.cs`, `CLAUDE.md`.
- `ScambioOggetti.cs` resta ma la quest 4 non lo usa più.
- Cardo nero e Lunaria non sono ancora nel mondo: li mette Lorenzo.

### 13 ottobre: lo sprint non consuma resistenza mentre si esplora (solo informazione)
Decisione di Lorenzo, ramo `lorenzoc/stamina-esplorazione`. Lo sprint consuma resistenza **solo in combattimento**: con un nemico vivo entro **15 m** (sveglio o no; manichini e sagome non contano), con un nemico che insegue entro 40 m, o per **5 s** dopo un colpo ricevuto (anche parato o schivato). Esplorando si corre gratis e la barra si ricarica. Schivata, attacco e parata costano come prima. Numeri regolabili nell'Inspector di `GiocatoreControllo` (sezione "Sprint mentre si esplora"; si può anche spegnere).
- **File condiviso toccato:** solo `GiocatoreControllo.cs` (nuova proprietà pubblica `InCombattimento`, utile anche per altro, per esempio la musica di combattimento).

### 12 ottobre, notte: quest 4 del prologo e pozioni con il tasto F (serve un'occhiata di Giuseppe)
Ramo `lorenzoc/quest-4` (parte da `lorenzoc/vicoli-villaggio`: va unito dopo la #62). Finita la quest 3 compare "Vai dall'erborista Ilse": il blocco "Erborista" è sostituito dalla **bottega di Ilse**, vuota dentro (bancone, mensole con vasetti, erbe appese, calderone verde). Ilse chiede **tre Fiori di riva**, nell'acqua bassa della riva (a nord del molo, più a nord e a sud); l'obiettivo conta "Fiori di riva raccolti: N/3" e la bussola indica il fiore più vicino. Portati i fiori, Ilse dà **due Pozioni di Ilse** (+60 vita in 1 secondo): una da bere subito (la vita persa con i ratti), l'altra per dopo. Bevuta la pozione con **F**, Ilse racconta del lago e la quest finisce.
- **Decisione di Lorenzo:** i consumabili si usano con **F** (si cambia in Opzioni > Comandi, voce "Usa oggetto"; freccia giù sul pad). Se premi F durante un attacco o una schivata, la pozione parte appena finiscono.
- **File nuovi:** `DatiConsumabile.cs`, `DatiOggettoMissione.cs`, `UsoConsumabili.cs` (si crea da solo come il menu di pausa: tasto F e rombo della pozione sopra le armi), `RaccoltaOggetti.cs`, `ScambioOggetti.cs`, `QuandoUsato.cs`, `Assets/Editor/CreaPrologoQuest4.cs`; oggetti `Assets/Dati/Oggetti/Consumabili/pozione-di-ilse` e `Assets/Dati/Oggetti/Missioni/fiore-di-riva`.
- **File condivisi toccati:** `Comandi.cs` (azione `UsaOggetto`, tasto F), `MenuComandi.cs` (13 azioni, 7 per colonna, righe un po' più basse), `InventarioGioco.cs` (scheda **Varie**; una pozione non si equipaggia: E la sceglie per F), `FormeOggetti.cs` (boccetta e fiore provvisori), `OggettoRaccoglibile.cs` (forme più grandi per pozioni e fiori), `GiocatoreControllo.cs` (`Cura` ora pubblico), `Lingua.cs`, `CLAUDE.md` (riga dei tasti), `VillaggioLagoNero.unity`.
- Da sapere: con lo Stregone le schede dello zaino sono 7 e "Incantesimi" va a capo; se serve si accorcia il nome.

### 12 ottobre, notte: vicoli ciechi del villaggio chiusi con staccionate (solo informazione)
Ramo `lorenzoc/vicoli-villaggio` (parte da `lorenzoc/quest-3`: va unito dopo la #61). Lorenzo ha chiesto che fra le costruzioni non ci siano angoli in cui ci si infila e si resta incastrati. Nuovo comando **"magic-gnl > Villaggio: chiudi i vicoli ciechi"** (`ChiudiVicoliCiechi.cs`): analizza la mappa vista dall'alto a quadretti di 20 cm, lascia aperti i passaggi stretti che portano da qualche parte (anche con la strettoia) e chiude con una staccionata di legno (o una catasta, se la bocca non ha muri ai lati) i vicoli che finiscono nel nulla. Non tocca gli spazi vicino ai punti delle quest. Su VillaggioLagoNero ha messo 12 interventi (gruppo "Recinzioni dei vicoli ciechi" dentro "Villaggio"). "Villaggio: mostra la mappa dei vicoli" (`MappaVicoli.cs`) fa vedere il risultato. **Da rifare** dopo aver aggiunto o spostato costruzioni (toglie le staccionate vecchie e rifà l'analisi).

### 12 ottobre, notte: quest 3 del prologo, il granaio di Hobb e tre ratti giganti (serve un'occhiata di Giuseppe)
Ramo `lorenzoc/quest-3` (parte da `lorenzoc/quest-2`: va unito dopo la #60). Finita la quest 2 compare "Vai al granaio di Hobb, sul lago" e Hobb (prima muto) si può far parlare; dopo il dialogo si svegliano **3 ratti giganti** nel granaio (12×16 m, a sud della strada del molo, portone verso la strada). Numeri di Lorenzo: **37 di vita, 11 di danno, un attacco ogni 4 s** per ratto: si acquatta e diventa rosso (preavviso), balza mordendo, indietreggia. Due ratti non balzano mai insieme. L'obiettivo conta "Ratti rimasti nel granaio: N"; morti tutti, Hobb ringrazia e la quest finisce. La vita persa si recupera nella quest 4.
- **File nuovi:** `RattoGigante.cs`, `GruppoNemici.cs`, `NomeTradotto.cs` (nome del nemico agganciato tradotto in 8 lingue), `Assets/Editor/CreaPrologoQuest3.cs`.
- **File condivisi toccati:** `Missioni.cs` (obiettivo con un numero: `ImpostaObiettivoConNumero`), `HudGioco.cs` (il nome del nemico agganciato passa da `NomeTradotto`), `Lingua.cs` (testi della quest 3 e nomi dei nemici), `VillaggioLagoNero.unity`.
- Prova veloce: in Play, "magic-gnl > Prova prologo > Ferisci i ratti del granaio (-20)" due volte chiude lo scontro.

### 12 ottobre, sera: quest 2 del prologo, tiro con l'arco, quest 1 migliorata (serve un'occhiata di Giuseppe)
Ramo `lorenzoc/quest-2`, provato in Unity sul PC di Lorenzo con le tre classi.
- **Quest 2** (comando `magic-gnl > Prologo: prepara la quest 2`): cortile della fucina con forgia, tettoia, steccato, Brannoc, manichino e sagome di tiro (solo Ladro); cortile di studio accanto al tempio con Edric e bersagli (solo Stregone). Brannoc dà Spada e Scudo della vecchia vita (Guerriero) o Pugnale da scuoiare e Arco corto da caccia (Ladro) e manda lo Stregone da Edric (Bastone e Libro della vecchia vita, Scintilla, Scheggia di ghiaccio). Allenamento con l'elenco delle mosse a schermo (`AllenamentoMosse`): finita ogni mossa almeno una volta, la quest finisce e il sole avanza. Dialoghi della quest 2 in 8 lingue (`dlg.q2.*`, `dlg.q2s.*`).
- **Tiro con l'arco** (`TiroConLArco.cs`, nuovo): tasto 2 per prendere l'arco, tenere premuto Attacco per tendere, lasciare per tirare; usa i numeri degli archi già decisi. **Tocca file di tutti:** `GiocatoreControllo.cs` (con l'arco in mano niente colpi corpo a corpo), `HudGioco.cs` (rombo dell'arco acceso con il tasto 2), `DatiArmaDistanza.cs` (solo il commento).
- **Camera nelle stanze piccole** (`ZonaCamera.cs`, nuovo): dentro casa e nel capanno la camera sta più vicina e non guarda dall'alto. **Tocca** `CameraTerzaPersona.cs` (legge i limiti della zona, passaggio morbido; fuori dalle zone niente cambia).
- **Quest 1 migliorata:** casa più spaziosa e arredata (tetto a due falde, finestre, lettino di Tobin, cesto della legna, legna in braccio), capanno più alto con tetto solido, Tobin rifà la strada del giocatore invece di attraversare i muri, alberi tolti dove toccavano casa e cortili.
- Comandi per le prove: `magic-gnl > Prova prologo > Vai da ...` porta il giocatore ai punti delle quest durante il Play.
- **`LogoStudio.cs.meta` mancava su `main`:** l'ha creato il Unity di Lorenzo ed è caricato in questo ramo; se Giuseppe ne ha già uno, tenere il suo.

### 12 ottobre: bussola in alto per gli obiettivi (solo informazione)
Lorenzo ha chiesto una bussola a schermo (ramo `lorenzoc/bussola`): striscia in alto al centro con i punti cardinali (tradotti, chiavi `hud.nord/est/sud/ovest`) e un rombo color fiamma sull'obiettivo, con la distanza in metri. Si vede solo nelle scene con `Missioni` (non nella zona di prova). Script nuovi in `Assets/Scripts/Livelli/`: `Bussola.cs` (si crea da sola, come l'interfaccia) e `MetaObiettivo.cs` (il punto da indicare, si attiva dagli eventi delle quest); `Missioni.cs` ha `ImpostaMeta`. Usa lo stile di `GraficaMenu`: se Giuseppe cambia caratteri e grandezze, la bussola li segue.

### 11 ottobre, notte: testi poco leggibili, da cambiare carattere e grandezza (richiesta per Giuseppe)
Provando il prologo, Lorenzo trova i testi **poco leggibili**: chiede di **cambiare il carattere (font) e la grandezza** dei testi in partita (dialoghi, suggerimenti, obiettivo, scritte delle azioni). Stile e caratteri stanno in `Assets/Scripts/Interfaccia/GraficaMenu.cs` (area di Giuseppe). La prova del prologo e della quest 1 è andata bene.

### 11 ottobre, sera: inizio del prologo e quest 1, con un sistema delle missioni (serve un'occhiata di Giuseppe)
Lorenzo ha iniziato il prologo (ramo `lorenzoc/prologo-quest-1`, contiene anche `lorenzoc/orario-prologo`). **Il sistema delle missioni lo fa il Claude di Lorenzo**, così il Claude di Giuseppe non lo rifà: sono pezzi generici in `Assets/Scripts/Livelli/` da collegare con gli eventi dell'Inspector.
- `Missioni.cs`: obiettivo a schermo (in alto a destra, sotto l'ora) e suggerimenti del tutorial in fila (con `MessaggiSchermo`); `QuestFinita(n)` avvisa `OrarioPrologo`.
- `DialogoAutomatico.cs` (dialogo che parte da solo, all'avvio o da un evento), `PuntoInterazione.cs` ("E  Prendi la legna" con un evento), `ZonaArrivo.cs` ("sei arrivato dal fabbro"), `SeguiGiocatore.cs` (Tobin che ti segue), `DissolvenzaIniziale.cs` (schermo che si schiarisce all'inizio).
- Comando `magic-gnl > Prologo: prepara la quest 1` (`Assets/Editor/CreaPrologoQuest1.cs`): nella scena `VillaggioLagoNero` (di Lorenzo) costruisce la casa dell'eroe vuota dentro, Maren, Tobin, il capanno e collega tutta la quest 1.
- **Tocca un file di tutti:** `ProjectSettings/EditorBuildSettings.asset`, aggiunta la scena `VillaggioLagoNero` (serve perché "Nuova partita" la carichi; prima caricava `ZonaProva`). Attenzione: anche "Ospita" ora carica il villaggio; con la decisione del co-op dalla gattabuia, va bloccato nel menu (area di Giuseppe).
- Due chiavi nuove in `Lingua.cs`: `hud.prendi_legna`, `hud.metti_legna` (8 lingue).
- Script creati con il loro `.meta`, come chiesto.

### 11 ottobre: prologo obbligatorio, tempo a tappe, co-op dalla gattabuia (serve la conferma di Giuseppe)
Decisioni di Lorenzo sul giorno a Villaggio Lago Nero:
1. **Tutte e 6 le quest sono obbligatorie** (anche la 5, la leggenda del lago, che era facoltativa). La razzia parte solo quando è finita la quest 6.
2. **Il tempo nel prologo non scorre da solo**, altrimenti la notte (e la razzia) potrebbe arrivare prima di finire le quest. Proposta già programmata (ramo `lorenzoc/orario-prologo`, script nuovo `Assets/Scripts/Livelli/OrarioPrologo.cs`, nessun file di Giuseppe toccato): si parte alle 7; ogni quest finita fa avanzare il sole piano piano (9, 11, 13, 15, poi tramonto alle 18 per la quest in taverna); finita la sesta cala la notte (21) e parte la razzia; da lì il tempo torna normale. L'ordine delle quest 2-5 non conta: conta quante ne hai finite. Ogni quest chiama `OrarioPrologo.QuestFinita(numero)` alla fine (per esempio dall'evento "Al Termine" dell'`Interlocutore`); quando ci sarà il sistema delle quest, basta chiamarlo da lì.
3. **Il co-op si apre solo dalla gattabuia in poi.** Il prologo (giorno, razzia, orco enorme) si gioca **da soli**; ci si risveglia in gattabuia e da lì si può giocare in co-op.

**Cosa cambia (da controllare, Claude di Giuseppe):**
- Il menu "Multigiocatore" e l'ingresso degli amici vanno legati al punto della storia: nel prologo niente co-op, dalla gattabuia sì (rete e menu sono di Giuseppe).
- **Domanda:** un amico che entra in gattabuia deve aver già fatto il prologo con il suo personaggio, oppure può entrare anche con un personaggio nuovo (si sveglia in cella senza aver visto il prologo)? Proposta di Lorenzo da confermare: può entrare anche nuovo.
- Diventano inutili le regole co-op del prologo: famiglia "dell'host" e battute sentite da tutti, ratti in più in co-op (quest 3), razzia e orco enorme bilanciati per 2 e 3 giocatori.
- In gattabuia resta com'era: celle separate nello stesso corridoio, l'host si libera da solo e libera gli altri.
- Se approvato: riga di `CLAUDE.md` sul co-op ("Co-op da 1 a 3 giocatori... disponibile dalla gattabuia in poi") e `Docs/quest-capitolo-1.md` (quest 5 non più facoltativa).

### 10 ottobre, sera: risposte sulla scaletta del capitolo 1
Risposte di Lorenzo alle 4 domande della voce "scaletta del capitolo 1" (ramo `giuseppec/scaletta-capitolo-1`).
1. **Percorso di giorno:** 20-30 minuti vanno bene.
2. **Razzia a 2 ondate:** la **prima** è il tutorial (si impara a combattere contro orchi che si possono battere); la **seconda** devasta il villaggio e non si riesce a fermarla.
3. **L'orco della piazza e la gattabuia:**
   - L'orco enorme in piazza è un **miniboss che ritroveremo più avanti** (capitolo 2 o 3). Lo scontro è **normale**, non truccato: quando moriamo, invece di rinascere al checkpoint, **rinasciamo nella gattabuia** (risveglio in cella).
   - **La gattabuia è una zona della mappa** esplorabile solo in quel momento. Una volta evasi ci sono due idee, da scegliere insieme:
     a) **sparisce**: si chiude per un crollo dell'entrata (o senza spiegazione);
     b) **diventa il campo base**: quando ci si riposa ci si teletrasporta lì.
4. **Scena `VillaggioLagoNero.unity`:** per Lorenzo è indifferente, dipende da cosa c'è da fare. Se il percorso di giorno, i bauli e le ondate sono suoi, conviene che la scena sia sua nei momenti in cui ci lavora (da accordare nel canale come per le altre scene).

**Orco della piazza, deciso da Lorenzo:** il gruppo **non può batterlo**. Ha statistiche troppo alte per essere ucciso e, se lo scontro dura, **dopo 3 minuti diventa invulnerabile a ogni danno e a ogni effetto** (rallentamenti, blocchi, stordimenti, spinte). Si muore e ci si risveglia nella gattabuia.

**Punto aperto (Claude di Lorenzo):**
- Campo base nella gattabuia: è comodo per il co-op (tutti nello stesso punto), ma va spiegato nella storia (la tana degli orchi come rifugio). Se si sceglie la a), il riposo resta ai falò e ai checkpoint.

### 10 ottobre, sera: revisione dei dialoghi del capitolo 1 (fatta)
Lorenzo ha rivisto `Docs/dialoghi-capitolo-1.md` (ora "bozza 2") sul ramo `lorenzoc/dialoghi-capitolo-1`. Cosa è cambiato:
- **Tasti nei suggerimenti:** al posto di "W A S D", "tasto sinistro", "Spazio", "E", "Tab" c'è il nome dell'azione fra graffe (`{Attacca}`, `{Schiva}`, `{Interagisci}`...), come nell'enum `Azione` di `Comandi.cs`: il codice del dialogo deve mettere il tasto scelto dal giocatore. Restano fissi mouse, rotellina e 1-6.
- **Mana dello Stregone:** "si ricarica da solo poco dopo l'ultimo incantesimo" (non "se resti fermo").
- **Aggancio:** aggiunto "Con un bersaglio agganciato, muovi il mouse verso un altro nemico per passare a lui".
- **Ilse:** "Da bambina ho visto qualcosa uscire dal lago... Non ho mai detto a nessuno che cosa fosse."
- **Quest 1:** aggiunto "(L'eroe si alza.)" fra le due battute di Maren.
- **Lasciati così:** la pozione di Ilse (gli oggetti consumabili si aggiungono strada facendo) e la "cantina" di Hobb.
- **Decisioni di Lorenzo da confermare:** in co-op la famiglia è dell'**host** ("papà" è solo l'host, gli altri sentono le stesse battute); per ora **tutti i personaggi giocabili sono uomini**.

### 10 ottobre, sera: la gattabuia (da leggere)
Lorenzo ha deciso la gattabuia: tutto in `Docs/gattabuia.md` (ramo `lorenzoc/gattabuia`). In breve: si arriva morendo contro l'orco della piazza; circa 30 minuti in 8 zone: celle (sbarra arrugginita che diventa la prima arma; in co-op celle separate nello stesso corridoio, solo l'host si libera e apre le altre), corridoio delle celle con i cadaveri, orchi macellai da superare con armi improvvisate, armeria quasi vuota con le armi base della classe e un checkpoint, sotterranei con fosse comuni e pattuglie, grotte buie con parassiti simili a ragni, piazzola con il **Carceriere** (miniboss) ai piedi della scalinata, portone e bosco morto.
- **Per Giuseppe:** le scene scritte (risveglio, fine del capitolo al portone) sono del tuo Claude; e resta da scegliere insieme se dopo la fuga la gattabuia crolla o diventa il campo base.
- **Per Nazar:** i modelli nuovi sono in fondo al documento; il resto è già nella sua lista.

### 11 ottobre: danno dei nemici in co-op più basso (solo informazione)
Lorenzo ha deciso che il danno che ricevono i giocatori cresce meno: **x1 da soli, x1,25 in 2, x1,5 in 3** (prima x1,5 e x2). Vita e frequenza degli attacchi restano come prima. Ramo `lorenzoc/danno-coop`: cambiati solo i valori di partenza in `DifficoltaCoop.cs` e la riga in `Docs/rete-coop.md`.

### 10 ottobre: regole contro le combinazioni troppo forti in co-op (serve un controllo di Giuseppe sulla rete)
Con le classi libere, Lorenzo ha deciso queste regole (ramo `lorenzoc/equilibrio-coop`, dettagli in `Docs/rete-coop.md`, sezione "Combinazioni troppo forti"):
- controlli (rallentamenti, blocchi, stordimenti) -30% per ogni Stregone nel gruppo, solo in co-op;
- i nemici passano a chi fa più danno ogni 7 s, entro 25 m;
- in co-op si va **a terra** e un alleato rialza (E per 3 s, 30% della vita); contro un boss una volta sola, poi spettatori fino a fine scontro; tutti a terra = rinascita al checkpoint e boss da capo (da soli: morte contro un boss = boss da capo);
- i boss ignorano le Bambole di ossa; 2 Fuochi fatui per Stregone in co-op; chi vede un'esecuzione furtiva entro 8 m va in allerta per 15 s.
- **Tocca file della rete:** `GiocatoreRete.cs` (classe; a terra e spettatore nei bit 11 e 12 del numero delle azioni; `ChiediRialza` con un Rpc al proprietario), `MondoRete.cs` (`InviaGruppoSconfitto`), `DifficoltaCoop.cs` (due numeri nuovi nell'Inspector), più `Bersaglio.cs` e `InseguimentoNemico.cs`. Giuseppe, dai un'occhiata prima di unire.
- Numeri della difficoltà: per ora restano i tuoi, Lorenzo li regola giocando in co-op. Nemici extra solo in co-op: risposta in arrivo dopo le prove.

## Chiuso

### 10 ottobre: una classe per giocatore in co-op, RIFIUTATA da Giuseppe
Proposta di Lorenzo (9 ottobre): una classe per giocatore, niente doppioni. **Risposta di Giuseppe: no, in co-op si può avere tutti lo stesso personaggio** (anche 3 Stregoni o 3 Guerrieri). Per i blocchi a catena restano le regole già scritte (immunità di 4 s dopo un rallentamento, boss che non si bloccano né si stordiscono).


### 9 ottobre: nuovo sistema dello Stregone, approvato da Giuseppe
Programmato sul ramo `lorenzoc/oggetti-stregone`: 12 incantesimi in 4 caselle (fino a 6 con i libri), tasti 1-6, bastoni, libri, vesti e amuleti con pro e contro. Regole e numeri in `Docs/incantesimi-stregone.md`; `Docs/oggetti-stregone.md` spostato in `Docs/archivio/`. Limite noto del co-op: le evocazioni esistono solo sul PC di chi le lancia (sezione "Note sul codice" del documento).

### 10 ottobre: nuovi comandi dell'aggancio e rotellina per gli incantesimi (già unito, approvato)
Provando, Lorenzo ha deciso di cambiare i comandi (ramo `lorenzoc/forme-oggetti-aggancio`, pull request #24, da unire dopo la #23):
- **Agganciare e sganciare:** clic della rotellina (come prima).
- **Cambiare nemico:** non più girando la rotellina, ma con uno **scatto del mouse verso il nemico** che si vuole (destra, sinistra, in alto per uno più lontano). Un movimento lento non cambia niente. Sul pad: scatto della levetta destra, in qualsiasi direzione. Numeri regolabili nell'Inspector di `AggancioBersaglio` (Scatto Mouse 140 px, Dimentica Mouse, Angolo Gesto 60°).
- **Rotellina libera:** con lo Stregone sceglie l'incantesimo (giù = casella dopo, su = casella prima), oltre ai tasti 1-6.
- Nella stessa pull request: forme provvisorie per ogni oggetto, addosso e a terra (`FormeOggetti.cs`, `AspettoEquipaggiamento.cs`).
- **Attenzione, conflitto su `CLAUDE.md`:** il ramo `giuseppec/regole-coop` cambia la stessa riga ("Co-op da 1 a 3 giocatori... Aggancio del bersaglio"). Riga unita proposta: "Co-op **da 1 a 3 giocatori**. **Classi libere** [...] (regole in `Docs/rete-coop.md`). Aggancio del bersaglio (lock-on): clic della rotellina per agganciare e sganciare, uno scatto del mouse verso un nemico cambia bersaglio; con lo Stregone la rotellina sceglie l'incantesimo (Lorenzo, 10 ottobre). Oggetti addosso e a terra con forme provvisorie: `FormeOggetti.cs`."
