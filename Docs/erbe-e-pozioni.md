# Erbe e pozioni: la bottega di Ilse

Decisione di Lorenzo, 13 ottobre 2026. Proposta di fiori e ricette del Claude di Lorenzo, da rivedere giocando.

## L'idea

L'erboristeria di Ilse è un **negozio di baratto**: si portano fiori e si ricevono pozioni. **Ogni fiore ha uno scopo** e ogni fiore, o combinazione di fiori, dà una pozione precisa. Nel prologo serve solo la pozione della vita (quest 4); le altre ricette si vedono già nella bottega, spente finché non si hanno i fiori.

Le **monete** non ci sono ancora. Quando arriveranno saranno un oggetto come gli altri: basterà metterle fra gli ingredienti di una ricetta ("5 Monete → 1 Pozione di Ilse", per comprare) o come risultato ("1 Cardo nero → 2 Monete", per vendere). Il negozio non va rifatto.

## I fiori

| Fiore | Dove cresce (proposta) | A cosa serve |
|---|---|---|
| **Fiore di riva** | Acqua bassa della riva del lago (già nel villaggio, quest 4) | Vita |
| **Cardo nero** | Campi incolti, fra le pietre (per esempio dietro le fattorie) | Resistenza |
| **Lunaria** | Si apre solo di notte, vicino a tombe e rovine (cimitero, tempio) | Mana (Stregone) |

Per ora solo i Fiori di riva sono nel mondo. Cardo nero e Lunaria vanno messi da Lorenzo (level design): oggetto vuoto con il componente `OggettoRaccoglibile` e, come "Oggetto", `Assets/Dati/Oggetti/Missioni/cardo-nero` o `lunaria`.

## Le ricette della bottega di Ilse

| Ricetta | Serve | Ricevi | Effetto |
|---|---|---|---|
| Pozione di Ilse | 3 Fiori di riva | 2 pozioni | +60 vita in 1 s |
| Tonico di cardo | 2 Cardi neri | 1 | +100 resistenza subito, ricarica della resistenza +50% per 60 s |
| Infuso di lunaria | 2 Lunarie | 1 | +60 mana |
| Balsamo di riva | 2 Fiori di riva + 1 Cardo nero | 1 | +100 vita in 4 s, ricarica della resistenza +30% per 30 s |
| Elisir del crepuscolo | 1 Fiore di riva + 1 Cardo nero + 1 Lunaria | 1 | +40 vita, +40 mana, +40 resistenza |

Tutti i numeri si cambiano dall'Inspector: le pozioni in `Assets/Dati/Oggetti/Consumabili/`, le ricette in `Assets/Dati/Ricette/`.

## Idee per dopo (utilità, non ancora programmate)

- **Antidoto** (quando ci saranno veleni, per esempio i ragni delle grotte della gattabuia).
- **Unguento d'ombra**: passi più silenziosi per qualche minuto (furtività del Ladro).
- **Olio di brace**: l'arma brucia per un minuto (più danno ai ragni e ai non morti).
- **Vista notturna**: la nebbia si dirada per un po'.
- **Monete**: comprare e vendere da Ilse e da altri mercanti (fabbro, oste).

## Come funziona nel codice

- `DatiRicetta` (`Assets/Scripts/Gameplay/Oggetti/`): cosa si dà e cosa si riceve.
- `Negozio` (`Assets/Scripts/Livelli/`): il negoziante e la finestra della bottega (elenco delle ricette a sinistra, dettaglio e pulsante "Scambia" a destra). Serve a qualsiasi mercante.
- `DatiConsumabile`: oltre alla vita ora può ridare mana e resistenza e far ricaricare più in fretta la resistenza per un po'.
- Menu "magic-gnl > Erbe: crea fiori, pozioni e ricette" (`Assets/Editor/CreaErbe.cs`): crea quelli che mancano.
- Quest 4: Ilse, quando ha i fiori, apre la bottega; il primo scambio fa passare a "Bevi una pozione".
