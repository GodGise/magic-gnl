# Il codice che esiste già

Fotografia del 4 ottobre 2026 del primo prototipo del combattimento. **Il codice nella repository è la fonte**: se qui c'è scritto un numero diverso da quello che vedi nello script, vale lo script. Da leggere solo quando si lavora su questi file.
Dopo il 4 ottobre sono arrivati altri lavori (camera che non attraversa i muri, sprint, checkpoint, trappole, leva e porta, chiesetta, chiave, baule, bastone magico): vedi `Docs/lavoro-lorenzo-6-ottobre.md` e, per il 4 ottobre, `Docs/archivio/lavoro-lorenzo-4-ottobre.md`.

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
| `Assets/Scripts/Gameplay/AggancioBersaglio.cs` | Aggancio del bersaglio (lock-on): clic della rotellina o pressione della levetta destra per agganciare e sganciare; uno scatto del mouse (o della levetta destra) verso un nemico per cambiare bersaglio; la rotellina non cambia più nemico (con lo Stregone sceglie l'incantesimo). Da agganciato il personaggio guarda sempre il nemico e la camera lo segue. Quadratino rosso sopra il bersaglio |
| `Assets/Scripts/Ambiente/CicloGiornoNotte.cs` | Sole, luna, luce, nebbia e cielo che cambiano con l'ora. 45 minuti reali di luce e 50 di notte; si parte alle 21; tenendo premuto T il tempo accelera |
| `Assets/Scripts/Ambiente/Torcia.cs` | Luce che tremola come una fiamma, più forte di notte |
| `Assets/Scripts/Ambiente/EffettoRetro.cs` | Immagine a bassa risoluzione in stile PS2; F2 la accende e la spegne |
| `Assets/Editor/CreaZonaProva.cs` | Menu **magic-gnl → Crea zona di prova (villaggio in rovina)**: crea `Assets/Scenes/ZonaProva.unity` con sentiero, torce, piazza con pozzo, case distrutte, cimitero, cripta, bosco, rocce, confini e tre nemici |
| `Assets/Segnaposto/Materiali/` | Materiali provvisori della zona, da sostituire con l'arte vera |

La scena `ZonaProva.unity` è salvata nella repository ed è la base della prima zona: si modifica a mano, il menu serve solo a ricrearla da zero. La scena `ScenaProva.unity` invece è esclusa da Git e si ricrea dal menu quando serve.

Regola nata oggi: Claude crea gli script senza file `.meta`. La prima volta che un ramo si apre in Unity, i `.meta` nuovi vanno salvati sullo stesso ramo prima dell'unione. E le prove in Unity le fanno le persone: Claude non prende il controllo del PC per provare, salvo richiesta esplicita.
