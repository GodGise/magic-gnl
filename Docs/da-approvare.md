# Da approvare: il "ponte" fra il Claude di Giuseppe e quello di Lorenzo

Questo file serve a passarsi messaggi e documenti fra i due Claude, senza una chat diretta (non esiste). Passa dal flusso normale dei rami e delle pull request, quindi Giuseppe resta quello che controlla.

## Come si usa

- **Il Claude di Giuseppe** scrive qui sotto, nella sezione "Per Lorenzo", cosa c'è da leggere, cosa si chiede e cosa serve come risposta. Il file arriva a Lorenzo con la pull request unita a `main`.
- **Il Claude di Lorenzo**, all'inizio di ogni sessione (dopo il Pull di `main`), legge questo file e dice a Lorenzo cosa c'è di nuovo.
- **Lorenzo risponde** nella sezione "Risposte di Lorenzo", sul suo ramo `lorenzoc/...`. La risposta arriva a Giuseppe con la sua pull request.
- Quando una voce è chiusa (approvata, modificata o rifiutata) la si sposta nella sezione "Chiuso", con la data e l'esito. Così il file resta corto.

## Per Lorenzo

### 8 ottobre: co-op, prima fase (solo informazione, non serve rispondere)
Giuseppe sta installando il co-op (ramo `giuseppec/rete-coop`, Netcode for GameObjects, vedi `Docs/rete-coop.md`). Nella prima fase **non si tocca nessuno script del giocatore, dei nemici o del combattimento**: gli altri giocatori compaiono come sagome di rete (`Assets/Scripts/Rete/`). Nelle fasi successive (vita, colpi, nemici decisi dall'host) i file `GiocatoreControllo.cs`, `Bersaglio.cs` e `InseguimentoNemico.cs` dovranno cambiare: **prima di toccarli chiederemo qui**. Se Lorenzo ci sta già lavorando, lo scriva nelle risposte così ci si accorda.
Per ora una regola da tenere a mente scrivendo codice nuovo: i valori del combattimento (danni, vita, tempi) restano nell'Inspector, e il danno lo deve decidere un solo punto del codice (non in tanti posti), perché in co-op lo deciderà l'host.

### 7 ottobre: due documenti nuovi, serve approvazione o modifiche
1. `Docs/mana.md`: regola del mana (si ricarica da solo, lo Stregone più in fretta; deve uccidere quanto Guerriero e Ladro). **Serve a:** barre dell'interfaccia e combattimento dello Stregone.
2. `Docs/oggetti-stregone.md`: 19 oggetti dello Stregone (5 armi, 5 libri, 3 vesti, 6 amuleti), scritti sullo stesso schema di `Docs/oggetti-guerriero.md` e `Docs/oggetti-ladro.md`. **Da fare:** rivedere i numeri oggetto per oggetto come per le altre due classi (danno, costo mana, tempi, portata) e dire se la regola del libro nella casella dello Scudo va bene. In fondo al file c'è l'elenco di cosa decidere.

**Cosa chiede Giuseppe:** leggere i due file e rispondere, per ciascuno, "approvato", oppure cosa cambiare. Non modificare i file direttamente: le modifiche si scrivono nelle risposte e le applica Giuseppe.

## Risposte di Lorenzo

(vuoto)

## Chiuso

(vuoto)
