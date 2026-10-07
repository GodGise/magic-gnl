# Oggetti del Guerriero: stato del lavoro

Riepilogo per Giuseppe (e per il Claude di Lorenzo alla prossima sessione). Ramo `lorenzoc/oggetti-guerriero`, 6 ottobre 2026 sera. **Lavoro in corso: non ancora da unire.** Mancano la revisione di metà degli oggetti e la prova in Unity.

## Cosa c'è

Il Guerriero ha quattro categorie di oggetti, decise da Lorenzo:

| Categoria | Tipi |
| --- | --- |
| Arma | spada, spadone (a due mani: niente scudo), ascia, mazza |
| Scudo | grande, medio, piccolo |
| Armatura | pesante, media, leggera |
| Amuleto | magico (bonus semplici), arcano (effetti speciali) |

Ogni oggetto è unico, con un nome suo (niente livelli di rarità). Gli oggetti si raccolgono con E e finiscono nell'inventario, da cui si equipaggiano: **l'inventario lo fa Giuseppe**. Per ora c'è la parte sotto: oggetti, statistiche ed equipaggiamento.

### Script (in `Assets/Scripts/Gameplay/Oggetti/`)

- `DatiOggetto`: la base comune. Contiene nome di lavoro, chiave del nome e della descrizione in `Lingua`, classe, **icona** e **modello 3D** (vuoti: li riempiremo con il lavoro di Nazar).
- `DatiArma`, `DatiScudo`, `DatiArmatura` e `DatiAmuleto`: i quattro tipi di oggetto. Sono ScriptableObject e si creano da *Create > magic-gnl > Oggetti*.
- `Equipaggiamento` (sul Giocatore, aggiunto da solo): quattro caselle, cioè arma, scudo, armatura e amuleto. Applica i numeri al personaggio. Rispetta la regola dello spadone: con lo spadone lo scudo si toglie.
- Modifiche agli script esistenti:
  - `GiocatoreControllo.AggiornaEquipaggiamento`: i numeri del colpo vengono dall'arma, quelli della parata dallo scudo (senza scudo si para con l'arma, peggio). Il peso di scudo e armatura rende la schivata più cara e la corsa più lenta.
  - Parata perfetta con lo scudo piccolo, ed effetti degli amuleti arcani.
  - `Bersaglio.Sbilancia`: dopo una parata perfetta il nemico lampeggia di azzurro e non attacca per un po'.
  - `Resistenza.MoltiplicatoreRecupero`.
  - `CalcoloDanno` accetta la penetrazione dell'armatura (mazze).

### Per l'inventario di Giuseppe

- Equipaggiare: `GetComponent<Equipaggiamento>().Equipaggia(oggetto)`. Mette l'oggetto nella casella giusta in base al tipo.
- Togliere: `TogliArma()`, `TogliScudo()`, `TogliArmatura()`, `TogliAmuleto()`.
- Cosa c'è addosso: le proprietà `Arma`, `Scudo`, `Armatura` e `Amuleto`; l'evento `Cambiato` avvisa per ridisegnare il menu.
- Testi: `oggetto.Nome` e `oggetto.Descrizione` passano da `Lingua.T`. **In `Lingua.cs` vanno aggiunte le chiavi** `oggetto.<nome>.nome` e `oggetto.<nome>.descrizione` (l'elenco dei nomi è nella tabella qui sotto). Finché mancano, si vede il nome italiano di lavoro. `Lingua.cs` è di Giuseppe, quindi non l'abbiamo toccato.

### Creare i file degli oggetti

Menu **magic-gnl → Crea oggetti del Guerriero** (`Assets/Editor/CreaOggettiGuerriero.cs`). Crea in `Assets/Dati/Oggetti/Guerriero/` (Armi, Scudi, Armature, Amuleti) solo i file che mancano. Gli oggetti già creati e ritoccati nell'Inspector non vengono toccati.

La cartella `Assets/Dati/` è nuova: se Giuseppe preferisce un altro posto, si sposta da Unity prima dell'unione.

## Gli oggetti

Riferimenti:
- giocatore senza oggetti: critico 10% da ×1,75, armatura 0, vita 100, resistenza 100;
- orco sgherro: vita 160, armatura 25, danno 26.

I critici delle armi si sommano a quelli del giocatore.

### Armi

| # | Oggetto (chiave) | Danno | Costo | Carica / recupero (s) | Portata / arco | Critico | Ignora armatura | Parata senza scudo (costo) | Revisione |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Spada della vecchia vita (`spada_vecchia_vita`) | 25 | 20 | 0,25 / 0,35 | 1,8 m / 120° | — | — | 40% (25) | ✔ fatta |
| 2 | Spada del capitano (`spada_capitano`) | 30 | 20 | 0,24 / 0,33 | 1,9 m / 120° | +5% | — | 30% (20) | ✔ fatta |
| 3 | Ascia da legna (`ascia_legna`) | 34 | 24 | 0,38 / 0,48 | 1,8 m / 150° | +5%, totale ×1,7 | — | 25% (30) | ✔ fatta |
| 4 | Ascia del boia (`ascia_boia`) | 42 | 30 | 0,42 / 0,52 | 1,9 m / 150° | +10%, totale ×2 | — | 35% (30) | ✔ fatta |
| 5 | Mazza ferrata (`mazza_ferrata`) | 28 | 25 | 0,36 / 0,45 | 1,6 m / 100° | — | 30% | 30% (26) | ✔ fatta |
| 6 | Martello di ossa (`martello_ossa`) | 34 | 28 | 0,42 / 0,50 | 1,7 m / 100° | +1,5% | 45% | 30% (28) | ✔ fatta |
| 7 | Spadone del cavaliere (`spadone_cavaliere`), due mani | 46 | 32 | 0,50 / 0,60 | 2,4 m / 160° | +5% | — | 30% (35) | ✔ fatta |


### Scudi

| # | Oggetto (chiave) | Taglia | Para | Costo parata | Arco | Parata perfetta | Peso | Revisione |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 8 | Scudo della vecchia vita (`scudo_vecchia_vita`) | Medio | 60% | 25 | 120° | — | schivata +5, corsa -3% | ✔ fatta |
| 9 | Pavese di quercia (`pavese_quercia`) | Grande | 85% | 22 | 160° | — | schivata +15, corsa -10% | ✔ fatta |
| 10 | Brocchiere di ferro (`brocchiere_ferro`) | Piccolo | 30% | 25 | 100° | entro 0,2 s: nemico sbilanciato 0,8 s, e intanto i colpi che riceve fanno ×1,5 | schivata +3, corsa -2% | ✔ fatta |

### Armature

| # | Oggetto (chiave) | Peso | Armatura | Peso sulla schivata / corsa | Revisione |
| --- | --- | --- | --- | --- | --- |
| 11 | Giubba di cuoio imbottito (`giubba_cuoio`) | Leggera | 10 | +3,5 / -2% | ✔ fatta |
| 12 | Cotta di maglia rattoppata (`cotta_maglia`) | Media | 25 | +6,5 / -5% | ✔ fatta |
| 13 | Corazza di piastre annerite (`corazza_piastre`) | Pesante | 45 | +12 / -12% | ✔ fatta |

### Amuleti

| # | Oggetto (chiave) | Tipo | Effetto | Revisione |
| --- | --- | --- | --- | --- |
| 14 | Zanna di lupo (`zanna_lupo`) | Magico | +10% danno | da fare |
| 15 | Occhio di corvo (`occhio_corvo`) | Magico | +8% critico | da fare |
| 16 | Pietra del focolare (`pietra_focolare`) | Magico | +10 armatura | da fare |
| 17 | Cuore di brace (`cuore_brace`) | Arcano | +12 vita per nemico ucciso | da fare |
| 18 | Respiro del lago (`respiro_lago`) | Arcano | resistenza +30% più veloce | da fare |
| 19 | Sangue antico (`sangue_antico`) | Arcano | 8% del danno inflitto torna come vita | da fare |

## Prossimi passi

1. Finire la revisione con Lorenzo, dagli amuleti in poi.
2. Prova in Unity: menu *Crea oggetti del Guerriero*, poi trascinare gli oggetti nelle caselle di **Equipaggiamento** del Giocatore e combattere l'orco sgherro nel villaggio.
3. Salvare e caricare i `.meta` e i file `.asset` creati da Unity, poi aprire la pull request.
4. Più avanti: inventario e raccolta con E (Giuseppe), modelli e icone (Nazar), traduzioni in `Lingua.cs`.
