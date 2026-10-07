# Mana: regola di design (decisa da Giuseppe il 7 ottobre)

Vale per lo Stregone e per ogni classe che usa il mana. La barra del mana è quella blu dell'HUD (vedi `interfaccia.md`).

## Principio

- **Il mana si ricarica da solo**, a differenza dei PF di Elden Ring (lì tornano solo con le fiale). Non si possono lanciare incantesimi all'infinito, ma non si resta nemmeno a secco per sempre.
- **Lo Stregone deve poter uccidere i nemici più o meno nello stesso tempo di Guerriero e Ladro.** Non deve essere OP, ma la ricarica deve essere abbastanza veloce da non fermarlo a metà combattimento.
- La differenza fra le classi sta nello **stile di gioco**, non nella forza: lo Stregone è fragile (poca vita) e per ricaricarsi deve stare lontano dai nemici; il Guerriero incassa e continua a colpire.

## Regole

1. **Ogni incantesimo costa mana.** Quelli base costano poco, quelli forti e le evocazioni costano molto.
2. **La ricarica parte dopo una breve pausa** dall'ultimo lancio (come la resistenza), poi riempie la barra senza che serva fare altro.
3. **A mana finito non si lancia niente** finché non ne è tornato abbastanza.
4. **Lo Stregone ricarica più in fretta delle altre classi che usano il mana** (deciso da Giuseppe il 7 ottobre).
5. Pozioni di mana: eventuale extra per recuperarlo più in fretta, non necessarie. Da decidere più avanti.

## Valori di partenza (da affinare giocando)

Sono solo un punto di partenza, e vanno messi come **valori modificabili in Unity** (non scritti fissi nel codice), così Giuseppe li regola provando:

- Mana pieno: basta per circa 20-30 secondi di fuoco continuo con gli incantesimi base.
- Ricarica completa: circa 10-15 secondi dopo la pausa iniziale (valore dello Stregone; le altre classi più lente).
- Se lo Stregone uccide più lentamente degli altri due, si alza la ricarica; se è troppo forte, si abbassa.
