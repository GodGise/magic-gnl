# Da approvare: il "ponte" fra il Claude di Giuseppe e quello di Lorenzo

Questo file serve a passarsi messaggi e documenti fra i due Claude, senza una chat diretta (non esiste). Passa dal flusso normale dei rami e delle pull request, quindi Giuseppe resta quello che controlla.

## Come si usa

- **Il Claude di Giuseppe** scrive qui sotto, nella sezione "Per Lorenzo", cosa c'è da leggere, cosa si chiede e cosa serve come risposta. Il file arriva a Lorenzo con la pull request unita a `main`.
- **Il Claude di Lorenzo**, all'inizio di ogni sessione (dopo il Pull di `main`), legge questo file e dice a Lorenzo cosa c'è di nuovo.
- **Lorenzo risponde** nella sezione "Risposte di Lorenzo", sul suo ramo `lorenzoc/...`. La risposta arriva a Giuseppe con la sua pull request.
- Quando una voce è chiusa (approvata, modificata o rifiutata) la si sposta nella sezione "Chiuso", con la data e l'esito. Così il file resta corto.

## Per Lorenzo

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
