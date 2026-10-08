# Da approvare: il "ponte" fra il Claude di Giuseppe e quello di Lorenzo

Questo file serve a passarsi messaggi e documenti fra i due Claude, senza una chat diretta (non esiste). Passa dal flusso normale dei rami e delle pull request, quindi Giuseppe resta quello che controlla.

## Come si usa

- **Il Claude di Giuseppe** scrive qui sotto, nella sezione "Per Lorenzo", cosa c'è da leggere, cosa si chiede e cosa serve come risposta. Il file arriva a Lorenzo con la pull request unita a `main`.
- **Il Claude di Lorenzo**, all'inizio di ogni sessione (dopo il Pull di `main`), legge questo file e dice a Lorenzo cosa c'è di nuovo.
- **Lorenzo risponde** nella sezione "Risposte di Lorenzo", sul suo ramo `lorenzoc/...`. La risposta arriva a Giuseppe con la sua pull request.
- Quando una voce è chiusa (approvata, modificata o rifiutata) la si sposta nella sezione "Chiuso", con la data e l'esito. Così il file resta corto.

## Per Lorenzo

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

(vuoto)

## Chiuso

(vuoto)
