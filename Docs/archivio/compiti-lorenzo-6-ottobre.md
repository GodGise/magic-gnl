# Compiti di Lorenzo per martedì 6 ottobre

Istruzioni per il Claude di Lorenzo. Le regole generali restano quelle di `CLAUDE.md` e `Docs/compiti-lorenzo.md`; questo file dice cosa fare domani. Accompagna Lorenzo **un passo alla volta**: un'istruzione, aspetta "fatto" o una foto, poi la successiva.

## Cosa è cambiato da ieri

- La **pull request #5** di Lorenzo è stata provata e unita a `main` da Giuseppe: camera, sprint, checkpoint, trappole, leva, porta, muro crepato, chiesetta, chiave, baule e bastone sono nella base comune. Bravo Lorenzo.
- **Il gioco inizia a Villaggio Lago Nero**, non nella gattabuia. Ordine della storia (`Docs/storia.md`):
  1. prologo a Villaggio Lago Nero, la notte della razzia degli orchi: il protagonista combatte e perde contro l'orco enorme;
  2. risveglio nella gattabuia, fuga;
  3. ritorno al villaggio, ormai in rovina.
  Quindi il prossimo lavoro di Lorenzo è **il villaggio**. La gattabuia viene dopo.
- C'è la **mappa vista dall'alto** del villaggio: `Docs/mappe/villaggio-lago-nero.png` (lo script che la genera è accanto). Un quadrato della griglia = 10 metri.
- **Il bastone magico**: resta com'è finché Giuseppe non decide se tenerlo o rimandare le classi. Domani non va toccato.
- `ZonaProva` resta la zona per provare il combattimento: domani non serve toccarla.

## Il lavoro di domani: blockout di Villaggio Lago Nero

**Blockout** vuol dire costruire il livello con cubi, piani e cilindri grigi della misura giusta, per poterci camminare dentro e capire spazi e distanze. I modelli veri di Nazar sostituiranno i cubi più avanti, senza cambiare la pianta.

Domani si costruisce **una sola versione** del villaggio, quella intatta. La versione distrutta e la notte della razzia verranno dopo, partendo da questa scena.

### Passo 0. Prima di iniziare (comandi Git)

1. GitHub Desktop: **Current branch → `main`**.
2. **Fetch origin**, poi **Pull origin**. Così arrivano il lavoro unito ieri e la mappa.
3. Leggere il canale Discord **#darkfantasyaggiornamenti** e scrivere l'annuncio, per esempio:
   > Lorenzo: oggi blockout di Villaggio Lago Nero, scena nuova Assets/Scenes/VillaggioLagoNero.unity e cartella Assets/Segnaposto/Villaggio
4. **Current branch → New branch**, nome `lorenzoc/villaggio-blockout`, creato da `main`. La **c** finale vuol dire "fatto con Claude": è la regola dei nomi dei rami in `CLAUDE.md`.
5. **Publish branch**. La notifica arriva da sola nel canale.

### Passo 1. Scena nuova

1. In Unity: **File → New Scene → Basic (Built-in)**, poi **File → Save As** in `Assets/Scenes/VillaggioLagoNero.unity`.
2. Creare la cartella `Assets/Segnaposto/Villaggio` per i materiali di questa scena.
3. Copiare dalla scena `ZonaProva` gli oggetti che servono per giocare, **senza modificare `ZonaProva`**:
   - aprire `ZonaProva`, selezionare il giocatore, la Main Camera e gli oggetti della luce e del cielo (sole, luna, ciclo giorno e notte), **Ctrl+C**;
   - riaprire `VillaggioLagoNero`, **Ctrl+V**;
   - se Unity chiede di salvare `ZonaProva`, rispondere **Don't Save**.

### Passo 2. Le misure: come passare dalla mappa a Unity

- 1 unità di Unity = 1 metro. La mappa è larga **210 m** (est-ovest) e alta **170 m** (nord-sud).
- Regola di conversione: un punto della mappa con coordinate (x, y), lette sui numeri ai bordi, in Unity va a **X = x, Z = 170 − y**. Il nord della mappa è la direzione **+Z** di Unity.
- Esempi già calcolati:

| Elemento | Sulla mappa (x, y) | In Unity (X, Z) | Misure circa |
| --- | --- | --- | --- |
| Centro della piazzetta (pozzo) | 96, 86 | 96, 84 | piazza larga circa 32 m |
| Taverna | 112, 75 | 112, 95 | 11 × 9 m |
| Mercante | 99, 68 | 99, 102 | 9 × 7 m |
| Fabbro | 79, 78 | 79, 92 | 8 × 7 m |
| Erborista | 80, 98 | 80, 72 | 8 × 7 m |
| Tempio | 60, 26 | 60, 144 | 16 × 10 m |
| Cimitero (recinto) | da 20,12 a 42,32 | da X 20 a 42, Z da 138 a 158 | 22 × 20 m |
| Casa dell'eroe | 140, 122 | 140, 48 | 9 × 7 m |
| Molo | da 163 a 193, y 97 | X da 163 a 193, Z 73 | 30 × 3 m |
| Riva del lago | x circa 160-170 | X circa 160-170 | il lago è tutto a est |

### Passo 3. La mappa come guida sotto i piedi (consigliato)

Mettere l'immagine della mappa sul terreno aiuta a ricalcare tutto nel posto giusto.

1. Trascinare `Docs/mappe/villaggio-lago-nero.png` in `Assets/Segnaposto/Villaggio`, oppure copiarla con Esplora file. In Unity si chiama "texture".
2. Creare un materiale `mappa-guida` (shader **Unlit/Texture**) e assegnargli la texture.
3. L'immagine ha intorno titolo e margini, quindi nel materiale bisogna impostare:
   - **Tiling** X 0.9608, Y 0.8815;
   - **Offset** X 0.0196, Y 0.0630.
4. Creare un **Quad**, chiamarlo `MappaGuida` e assegnargli il materiale. Impostare:
   - Position: 105, 0.02, 85;
   - Rotation: 90, 0, 0;
   - Scale: 210, 170, 1.
5. Guardando dall'alto (vista Scene, asse Y), il lago deve stare a destra (+X) e il tempio in alto (+Z). Se la mappa appare specchiata o capovolta, correggere la rotazione (per esempio Y 180) e ricontrollare. Va verificato insieme a Lorenzo, non dato per scontato.
6. A fine lavoro `MappaGuida` si può spegnere (casella accanto al nome nell'Inspector), ma non va cancellata.

### Passo 4. Costruire, in quest'ordine

Usare oggetti vuoti come cartelle nella Hierarchy: `Terreno`, `Strade`, `Piazza`, `Edifici speciali`, `Case`, `Bosco`, `Lago`.

1. **Terreno**: un piano grande 210 × 170 m (un Plane di Unity misura 10 × 10, quindi Scale 21, 1, 17, Position 105, 0, 85). Materiale verde spento.
2. **Lago**: un piano scuro, quasi nero, a est della riva, leggermente più basso del terreno (Y −0.3). La riva è un po' ondulata: bastano 2-3 piani ruotati.
3. **Strade e piazzetta**: piani sottili color terra (Y 0.01) sopra il terreno. Le strade principali sono larghe circa 3.5 m, i vicoli circa 2 m. La piazzetta è circa un cerchio di 32 m, con il **pozzo** al centro (un cilindro).
4. **Edifici speciali**: cubi alle misure della tabella, alti circa 5-6 m (taverna e tempio più alti, 7-8 m). Nome dell'oggetto uguale al nome sulla mappa.
5. **Case**: un cubo `casa-blocco` da circa 5 × 6 m in pianta e 5 m di altezza, trasformato in **prefab** (trascinarlo dalla Hierarchy alla cartella `Assets/Segnaposto/Villaggio`). Poi si ricopia (Ctrl+D) e si sistema seguendo la mappa. Regole del borgo, decise da Giuseppe:
   - intorno alla piazzetta le case sono attaccate e chiudono tutto il giro, tranne dove escono le strade, con **la porta verso il centro della piazza**. La porta si segna con un cubetto scuro sul lato giusto;
   - lungo le strade le case hanno la porta verso la strada;
   - più ci si allontana dalla piazza, più le case sono rade, fino al bosco.
6. **Bosco**: cilindri alti 6-8 m ai bordi ovest, nord e sud. Non serve metterne tanti domani: bastano a far capire dove inizia il bosco.
7. **Molo**: un cubo lungo e basso sull'acqua, più 2-3 barchette fatte con cubi.
8. **Giocatore**: metterlo davanti alla **casa dell'eroe** (circa X 140, Z 52), dove comincerà il prologo.

### Passo 5. Provare

Premere **Play** e camminare per il villaggio:
- le strade sono larghe abbastanza per combattere, con lo sprint e la camera?
- la piazzetta è troppo grande o troppo piccola?
- quanto ci si mette a piedi dalla casa dell'eroe alla piazza e al bosco?

Scrivere le risposte in `Docs/lavoro-lorenzo-6-ottobre.md`, con le foto dalla vista dall'alto se possibile. Servono a Giuseppe per decidere le modifiche.

### Passo 6. Fine giornata (comandi Git)

1. Unity: **Ctrl+S** per salvare la scena.
2. GitHub Desktop: controllare che fra le modifiche ci siano la scena, i materiali, il prefab e i loro file `.meta`.
3. Riepilogo in basso a sinistra, per esempio "Aggiungi il blockout di Villaggio Lago Nero", poi **Commit to lorenzoc/villaggio-blockout**.
4. **Push origin**.
5. **Create Pull Request** (si apre GitHub nel browser), poi **Create pull request**.
6. Scrivere **"finito"** nel canale Discord con il numero della pull request.

## Se avanza tempo

- Segnare con un cubo rosso il punto dove arrivano gli orchi (sentiero ovest, dal bosco) e con un cubo viola il punto dello scontro con l'orco enorme. Proposta da confermare con Giuseppe: la piazzetta.
- Proporre in `Docs/lavoro-lorenzo-6-ottobre.md` dove mettere checkpoint e torce per la notte della razzia.

## Da non fare domani

- Non modificare `ZonaProva` e non usare il menu "Crea zona di prova".
- Non toccare `Assets/Art` (è di Nazar).
- Non lavorare su `main`: tutto sul ramo `lorenzoc/villaggio-blockout`.
- Non costruire ancora la versione distrutta né la gattabuia.
