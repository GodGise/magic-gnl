# Da approvare: il "ponte" fra il Claude di Giuseppe e quello di Lorenzo

Questo file serve a passarsi messaggi e documenti fra i due Claude, senza una chat diretta (non esiste). Passa dal flusso normale dei rami e delle pull request, quindi Giuseppe resta quello che controlla.

## Come si usa

- **Il Claude di Giuseppe** scrive qui sotto, nella sezione "Per Lorenzo", cosa c'è da leggere, cosa si chiede e cosa serve come risposta. Il file arriva a Lorenzo con la pull request unita a `main`.
- **Il Claude di Lorenzo**, all'inizio di ogni sessione (dopo il Pull di `main`), legge questo file e dice a Lorenzo cosa c'è di nuovo.
- **Lorenzo risponde** nella sezione "Risposte di Lorenzo", sul suo ramo `lorenzoc/...`. La risposta arriva a Giuseppe con la sua pull request.
- Quando una voce è chiusa (approvata, modificata o rifiutata) la si sposta nella sezione "Chiuso", con la data e l'esito. Così il file resta corto.

## Per Lorenzo

### 7 ottobre: due documenti nuovi, serve approvazione o modifiche
1. `Docs/mana.md`: regola del mana (si ricarica da solo, lo Stregone più in fretta; deve uccidere quanto Guerriero e Ladro). **Serve a:** barre dell'interfaccia e combattimento dello Stregone.
2. `Docs/oggetti-ladro-stregone.md`: 19 oggetti per Ladro e 19 per Stregone, nello stesso schema del Guerriero. **Da fare:** confrontare con `Docs/oggetti-guerriero.md` (quando è unito) e dire se nomi dei campi, ordine e tipi non combaciano; controllare che i numeri abbiano senso per il bilanciamento.

**Cosa chiede Giuseppe:** leggere i due file e rispondere, per ciascuno, "approvato", oppure cosa cambiare. Non modificare i file direttamente: le modifiche si scrivono nelle risposte e le applica Giuseppe.

## Risposte di Lorenzo

(vuoto)

## Chiuso

(vuoto)
