# Erbe e pozioni: la bottega di Ilse

Decisione di Lorenzo, 13 ottobre 2026. Proposta di fiori e ricette del Claude di Lorenzo, da rivedere giocando.

## L'idea

L'erboristeria di Ilse è un **negozio di baratto**: si portano fiori e si ricevono pozioni. **Ogni fiore ha uno scopo** e ogni fiore, o combinazione di fiori, dà una pozione precisa. Nel prologo serve solo la pozione della vita (quest 4); le altre ricette si vedono già nella bottega, spente finché non si hanno i fiori.

Si può anche **comprare** con le monete, le **monete d'oro**, quando mancano i fiori (costa molto di più), e **vendere** i fiori a Ilse. Monete, prezzi e mercato: `Docs/monete.md`.

## I fiori

| Fiore | Dove cresce (proposta) | A cosa serve |
|---|---|---|
| **Fiore di riva** | Acqua bassa della riva del lago (già nel villaggio, quest 4) | Vita |
| **Cardo nero** | Fra i sassi dei campi incolti (per esempio dietro le fattorie) | Difesa: danni subiti -10% per 3 minuti |
| **Lunaria** | Si apre solo di notte, vicino a tombe e rovine (cimitero, tempio) | Mana massimo +5% per 3 minuti (Stregone) |

Fiori, pozioni e voci della bottega **esistono già** come file (`Assets/Dati/Oggetti/Missioni/`, `Assets/Dati/Oggetti/Consumabili/`, `Assets/Dati/Ricette/`), pronti da usare. Per ora solo i Fiori di riva sono nel mondo. Cardo nero e Lunaria vanno messi da Lorenzo (level design): oggetto vuoto con il componente `OggettoRaccoglibile` e, come "Oggetto", `Assets/Dati/Oggetti/Missioni/cardo-nero` o `lunaria`.

## Le ricette della bottega di Ilse

| Ricetta | Serve | Ricevi | Effetto |
|---|---|---|---|
| Pozione di Ilse | 3 Fiori di riva | 2 pozioni | +60 vita in 1 s |
| Tonico di cardo | 2 Cardi neri | 1 | Danni subiti -10% per 3 minuti |
| Infuso di lunaria | 2 Lunarie | 1 | Mana massimo +5% per 3 minuti |
| Balsamo di riva | 2 Fiori di riva + 1 Cardo nero | 1 | +100 vita in 7 s, danni subiti -10% per 3 minuti |
| Elisir del crepuscolo | 1 Fiore di riva + 1 Cardo nero + 1 Lunaria | 1 | +40 vita, danni subiti -10% e mana massimo +5% per 3 minuti |

**Regole (Lorenzo, 13 ottobre):**
- **Solo Tonico e Infuso sono cumulabili, e solo negli effetti, non nella durata**: tre Tonici = danni subiti -30%; ogni pozione bevuta fa ripartire i 3 minuti di tutta la somma, che poi finisce tutta insieme (al massimo -90%).
- Balsamo ed Elisir non si sommano a niente: conta il più forte fra il loro effetto e la somma dei Tonici (o degli Infusi).
- Gli effetti si vedono in basso a sinistra, sopra il rombo della pozione, con il tempo che resta. Morendo finiscono.
- **Cambiare consumabile**: tasto **C** (si cambia in Opzioni > Comandi, voce "Cambia oggetto"; Y / Triangolo sul pad) passa al consumabile dopo; F lo usa. Con più consumabili diversi, sotto il rombo compare "C  cambia".
- Il mana non si ridà con le pozioni (si ricarica da solo): la Lunaria alza il **mana massimo**.
- Nello zaino si portano **al massimo 3 pozioni** all'inizio del gioco (`Zaino`, "Massimo Consumabili", si potrà alzare più avanti). Con 3 pozioni la bottega non ne dà altre e quelle a terra non si raccolgono.

Tutti i numeri si cambiano dall'Inspector: le pozioni in `Assets/Dati/Oggetti/Consumabili/`, le ricette in `Assets/Dati/Ricette/`.

## Idee per dopo (utilità, non ancora programmate)

- **Antidoto** (quando ci saranno veleni, per esempio i ragni delle grotte della gattabuia).
- **Unguento d'ombra**: passi più silenziosi per qualche minuto (furtività del Ladro).
- **Olio di brace**: l'arma brucia per un minuto (più danno ai ragni e ai non morti).
- **Vista notturna**: la nebbia si dirada per un po'.

## Come funziona nel codice

- `DatiRicetta` (`Assets/Scripts/Gameplay/Oggetti/`): cosa si dà e cosa si riceve.
- `Negozio` (`Assets/Scripts/Livelli/`): il negoziante e la finestra della bottega (elenco delle ricette a sinistra, dettaglio e pulsante "Scambia" a destra). Serve a qualsiasi mercante.
- `DatiConsumabile`: oltre alla vita ora può ridare mana e resistenza e far ricaricare più in fretta la resistenza per un po'.
- Menu "magic-gnl > Erbe: crea fiori, pozioni e ricette" (`Assets/Editor/CreaErbe.cs`): crea quelli che mancano.
- Quest 4: Ilse, quando ha i fiori, apre la bottega; il primo scambio fa passare a "Bevi una pozione".
