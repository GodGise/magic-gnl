# Interfaccia di gioco: barre e inventario (approvate da Giuseppe il 7 ottobre)

Disegni approvati: `anteprima-barre-hud.png` e `anteprima-inventario.png` (Giuseppe li carica in `Docs/interfaccia/`).
Stile: lo stesso dei menu (`Assets/Scripts/Interfaccia/GraficaMenu.cs`): riquadri scuri con cornice di bronzo, rombi agli angoli, scritte Cinzel / IM Fell (Forum e Cormorant per il russo, ZCOOL XiaoWei per il cinese), colore fiamma per ciò che è attivo o scelto. Ogni scritta passa da `Lingua.T`.

**Il codice si scrive dopo che Lorenzo ha unito `lorenzoc/oggetti-guerriero`**: barre e inventario leggono vita, resistenza, mana ed equipaggiamento dai suoi script.

## Barre in partita (HUD)

- **In alto a sinistra**, una sotto l'altra: **Vita** (rossa, la più lunga, con "62 / 100" a destra), **Resistenza** (ocra), **Mana** (blu, solo se la classe lo usa). Cornice di bronzo, rombo all'inizio e rombino alla fine. La lunghezza può crescere quando aumenta il valore massimo.
- **Danno appena subito**: il pezzo di vita perso resta chiaro per circa mezzo secondo, poi scende (come in Elden Ring).
- **In basso a sinistra**: arma e scudo in mano dentro due rombi grandi, con il tasto (1, 2) sotto; quello in uso è acceso color fiamma. Accanto, un rombo più piccolo con l'amuleto.
- **In basso al centro**: nome e barra della vita del **nemico agganciato** (solo mentre è agganciato; per i boss sempre visibile durante lo scontro).
- **Bersaglio agganciato**: rombo vuoto color fiamma sopra il nemico (sostituisce il quadrato rosso di adesso).
- **Messaggi di azione**: riquadro in fondo al centro con il tasto in un rombo e l'azione ("E · Raccogli: Chiave della chiesa"). Sostituisce le scritte di `MessaggiSchermo`.
- **In alto a destra**: momento del giorno e ora del gioco ("Notte · 21:40"), al posto della scritta di prova del ciclo giorno e notte.
- Il pannello di prova del giocatore (stato, comandi) resta solo per lo sviluppo: si accende e spegne con un tasto (da decidere, per esempio F1).

- **Ladro** (8 ottobre): al posto dello scudo, l'**arma a distanza** nel rombo con il tasto 2 (spenta finché non c'è il tiro con l'arco). **Abilità Q** dell'amuleto (Ultimo respiro): rombo con la Q accanto all'amuleto, si riempie mentre si ricarica, color fiamma quando è pronta, viola mentre si è invisibili.
- **Morte**: "Sei morto" grande al centro, rosso cupo. L'avviso dell'esecuzione furtiva usa il riquadro delle azioni ("SX · Esecuzione furtiva"). I messaggi temporanei (`MessaggiSchermo`) hanno lo stile dei menu.

## Inventario (Tab apre e chiude)

- Titolo "Equipaggiamento" in alto con il divisore. Il gioco sotto si scurisce.
- **Pannello sinistro**: sagoma del personaggio con le **4 caselle** attorno (Arma e Scudo in alto, Armatura e Amuleto in basso), etichetta sopra e nome dell'oggetto sotto ogni casella; casella vuota con un trattino e "vuoto". In fondo le statistiche totali: Vita, Armatura, Danno, Critico.
- **Pannello destro, "Zaino"**: griglia di caselle (6 per riga) con le icone degli oggetti raccolti. La casella scelta ha il bordo color fiamma e un alone.
- **Schede dello zaino** (8 ottobre, come Elden Ring): Tutto, Armi (anche archi e balestre), Scudi, Armature, Amuleti, con il numero di oggetti accanto. Si cambiano con Q e R (LB e RB sul pad) o con un clic. Lo zaino non ha limite: si scorre con la rotellina.
- **Dettaglio** sotto lo zaino: nome dell'oggetto (color fiamma), tipo e classe a destra, descrizione in corsivo, statistiche con il **confronto** con l'oggetto equipaggiato nella stessa casella: verde se migliora, rosso se peggiora (per i costi, più alto = rosso).
- **Comandi**: frecce o levetta per scegliere, E (o A) per equipaggiare, Tab (o B) per chiudere; mouse: clic per scegliere, doppio clic per equipaggiare.
- Con lo spadone a due mani la casella dello scudo si svuota (regola già in `Equipaggiamento`).
- **Ladro**: la seconda casella è **Arma a distanza** al posto di Scudo (proposta di Lorenzo). Il Ladro non può equipaggiare scudi e le altre classi non possono equipaggiare archi e balestre: compare un avviso in basso.
- Nomi e descrizioni degli oggetti di Guerriero e Ladro sono tradotti nelle 8 lingue in `Lingua.cs`. Lo Stregone (libro nella seconda casella) lo fa Lorenzo con il suo Claude.
- In co-op l'inventario non ferma il gioco.

## Icone (compito per Nazar)

Una icona per ognuno dei 19 oggetti del Guerriero (`Docs/oggetti-guerriero.md`), 256x256 px, sfondo trasparente, colori spenti e caldi (osso, ferro, bronzo), leggibili anche piccole. Vanno nel campo **Icona** di ogni oggetto.
