# Lavoro di Lorenzo, 4 ottobre 2026

Riepilogo per Giuseppe e Nazar di quello che Lorenzo ha fatto con il suo Claude. Tutto è sui rami `lorenzo/...`: niente è ancora in `main`.

## Pull request e ordine di unione

| PR | Ramo | Contenuto |
| --- | --- | --- |
| #3 | `lorenzo/camera-sprint` | Camera che non attraversa muri e pavimento, sprint con Shift, aspetto provvisorio da figura umana |
| #4 | `lorenzo/checkpoint-trappola` | Tutto quello della #3, più checkpoint, trappole, leva e porta, muro crepato, animazioni, suoni e le modifiche alla zona di prova |
| #5 | `lorenzo/chiesa-chiave` | Tutto quello della #4, più chiesetta con porta a chiave, chiave nella cripta, baule, bastone magico con mana e questo riepilogo aggiornato |

Ogni ramo parte dal precedente e lo contiene. Il modo più semplice è unire direttamente la **#5** e poi chiudere la #3 e la #4. La #5 non va in conflitto con `main` (controllato la sera del 4 ottobre, dopo la storia e la lista dei modelli di Nazar).

## Cosa c'è

**Camera e movimento**
- Camera: non entra più in muri, strutture, pavimento e nemici. Vicino a un muro si avvicina in modo graduale, si alza un poco e guarda davanti al personaggio; se resta incastrata, il personaggio si nasconde invece di mostrarne l'interno.
- Sprint con Shift (o pressione della levetta sinistra): velocità 8, consuma 15 di resistenza al secondo, riparte con almeno 10.

**Morte e checkpoint**
- Con la vita a zero o cadendo nel vuoto compare "SEI MORTO" e dopo 2 secondi si rinasce all'ultimo checkpoint acceso (o alla partenza).
- Checkpoint (`Livelli/Checkpoint.cs`): si accende con E, cambia colore e fa luce; accenderne un altro spegne il precedente.

**Elementi della zona** (`Assets/Scripts/Livelli/`)
- `TrappolaSpuntoni`: invisibile, scatta dopo che il giocatore ci passa sopra (mezzo secondo di avviso con le punte appena fuori), 35 di danno; non si para ma si schiva. Nella vista Scene è un riquadro rosso.
- `Leva` (tasto E) e `Porta`: la porta è un cubo che scende sotto terra. Se la leva non ha porte collegate, apre la più vicina.
- `MuroFragile`: muro con crepe, trema a ogni colpo e crolla in blocchi dopo 4 colpi. Per questo l'attacco del giocatore ora colpisce anche questi muri.

**Figura provvisoria e animazioni** (segnaposto finché non arrivano i modelli di Nazar)
- `AspettoUmanoide`: durante il Play capsula e cilindri diventano figure a blocchi con spada (giocatore) o mazza (nemici). Agisce solo su capsule e cilindri di Unity, quindi si spegne da solo con i modelli veri.
- `AnimazioneUmanoide`: camminata, combo di tre colpi diversi (orizzontale, rovescio, dall'alto), schivata come scatto inclinato, parata, colpito, caduta. I nemici caricano durante il preavviso rosso e colpiscono alternando i tre movimenti.

**Suoni provvisori** (`Assets/Scripts/Ambiente/`)
- `Suoni`: tutti i suoni sono calcolati dal codice, senza file audio, quindi originali e senza problemi di licenza. Passi, fendenti, schivata, impatto, parata, guardia rotta, colpito, morte, rinascita, leva, porta, checkpoint, trappola, muro.
- `PassiSonori`: passi diversi su erba, terra del sentiero, pietra e legno, riconosciuti dal materiale del pavimento. `SuperficieSonora` serve a forzare il tipo su un pavimento o su una zona.

**Chiesetta, chiave e baule** (PR #5)
- `Livelli/Chiave`: chiave dorata che gira, si raccoglie con E. `Gameplay/Inventario` ricorda le chiavi raccolte.
- `Livelli/Serratura`: va sulla stessa porta di `Porta`. Con la chiave giusta la porta si apre con E; senza, compare "Serve: Chiave della chiesa".
- `Livelli/Baule`: si apre con E (il coperchio si alza) e dà il bastone magico.
- `Gameplay/MessaggiSchermo`: scritte brevi al centro dello schermo (oggetto raccolto, chiave mancante).
- Menu **magic-gnl → Aggiungi chiesetta (alla scena aperta)** (`Editor/AggiungiChiesetta.cs`): aggiunge chiesetta e chiave senza ricreare la scena e si annulla con Ctrl+Z. Lorenzo l'ha già usato su `ZonaProva`: non serve rifarlo.

**Bastone magico** (proposta da discutere: nella storia è l'arma dello Stregone)
- Tasti **1** (spada) e **2** (bastone), o le frecce sinistra e destra del pad. Non si cambia durante un attacco.
- Con il bastone l'attacco lancia una sfera luminosa (`Gameplay/SferaMagica`) che insegue il nemico agganciato, o il più vicino entro 20 metri: 20 di danno, rompe anche i muri crepati.
- Compare una terza barra, il **mana** (100): ogni sfera costa 20, ogni nemico ucciso ne ridà il 15%.
- Per ora si sblocca solo dal baule della chiesetta, per provarlo. Quando ci saranno le classi, si potrà legare allo Stregone.

## Modifiche alla scena `ZonaProva`
- Due trappole: appena dentro la porta della cripta e nella casa in rovina a sud-est della piazza.
- Checkpoint "Checkpoint altare" subito dopo il pozzo.
- "Porta cripta" nel vano d'ingresso e "Leva cripta" accanto, a sinistra.
- Il muro sinistro della cripta (`Lato sinistro`) è diviso in due pezzi più un architrave; nel varco c'è il "Muro crepato": un passaggio segreto per entrare senza leva.
- "Chiesetta" a est della piazza (posizione 28, 0, 10, porta verso la piazza), con baule davanti all'altare; "Chiave della chiesa" in fondo alla cripta.

## Script esistenti toccati (area del Claude di Giuseppe)
`GiocatoreControllo`, `CameraTerzaPersona`, `Resistenza`, `Bersaglio`. Le aggiunte sono valori pubblici in sola lettura per l'animazione, il danno dall'ambiente (`RiceviDannoAmbiente`), la rinascita al checkpoint, lo sprint, la combo e gli agganci dei suoni. Il combattimento di base non è cambiato.

## Come provare
1. GitHub Desktop: Fetch origin, poi Current branch → `lorenzo/chiesa-chiave`.
2. In Unity aprire `ZonaProva` e premere Play.
3. Percorso consigliato: accendere l'altare dopo il pozzo con E, combattere il Guardiano (combo da tre, schivata, parata), andare alla cripta, aprire la porta con la leva (E) ed entrare nella trappola; prendere la chiave in fondo con E; poi girare sul lato sinistro della cripta e rompere il muro crepato. Infine andare alla chiesetta a est della piazza, aprire la porta con E, aprire il baule e provare il bastone (tasto 2) sul Guardiano, guardando la barra del mana.

## Da sapere
- Tutti i numeri (camera, sprint, trappole, muro, animazioni) si regolano dall'Inspector.
- Lorenzo ha provato in Play camera, sprint, checkpoint, leva, porta, trappole e il giro chiave, chiesetta, baule e bastone. Animazioni d'attacco e suoni sono stati scritti per ultimi: vanno ascoltati e guardati con attenzione.
- Con la nuova storia, il "frammento d'anima" in fondo alla cripta previsto dai compiti non vale più: l'obiettivo della zona è da decidere.
- Il vecchio ramo di prova `lorenzo/prova` (pull request #2, chiusa) si può cancellare da GitHub: dalla sessione di Claude la cancellazione dei rami è bloccata.

## Da domani: proposte per andare avanti

Proposte di Lorenzo e del suo Claude, da confermare con Giuseppe (anche nel documento "Compiti del team").

**Per tutti**
1. Giuseppe prova e unisce la #5 (che contiene #3 e #4). Poi tutti fanno Fetch origin e Pull origin su `main`, così si riparte dalla stessa base.
2. In riunione si decide: il bastone resta come arma dello Stregone? Le classi si scelgono già nella vertical slice o si parte solo con il Guerriero?

**Lorenzo (level design e bilanciamento)**
1. **Bozza della prima regione, la gattabuia** (`Docs/storia.md`, regione 0): celle, corridoi con guardie che dormono, sotterranei, grotte e uscita in superficie, fatta con forme segnaposto in una scena nuova sua (per esempio `Assets/Scenes/Gattabuia.unity`). Prima di iniziare va annunciata e concordata con Giuseppe, che ha scritto la regione.
2. Riutilizzare quello che c'è già: cella che si apre (Porta + Leva o Serratura), mucchio del bottino degli orchi come baule con l'arma della classe, trappole e muri crepati come segreti, checkpoint.
3. **Nemico che si muove**: oggi il Guardiano sta fermo, non para e non schiva. Serve un orco di prova che pattuglia, insegue e attacca, per rendere il combattimento meno facile.
4. **Arma di partenza**: l'ascia da legna (o mani nude) prima di trovare l'arma nel bottino, come dice la storia.
5. Bilanciamento: con un nemico che si muove, rivedere dall'Inspector danni, costi di resistenza e mana.

**Nazar**
- I modelli della lista `Docs/lista-modelli-nazar.txt`: quando arrivano, le figure a blocchi spariscono da sole e si sostituiscono i segnaposto della zona.

**Da tenere d'occhio**
- `ZonaProva` è cresciuta molto: chi tocca la scena lo annuncia prima, come sempre.
- Le figure, le animazioni e i suoni sono tutti provvisori, fatti dal codice: vanno sostituiti con modelli, animazioni e suoni veri.
