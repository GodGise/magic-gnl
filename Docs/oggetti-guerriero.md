# Oggetti del Guerriero: stato del lavoro

Riepilogo per Giuseppe (e per il Claude di Lorenzo alla prossima sessione). Ramo `lorenzoc/oggetti-guerriero`, 6 ottobre 2026 sera. Aggiornato il 7 ottobre: **revisione finita** per tutti i 19 oggetti e prima prova in Unity andata bene. Mancano i file `.asset`/`.meta` creati da Unity e la pull request.

## Cosa c'è

Il Guerriero ha quattro categorie di oggetti, decise da Lorenzo:

| Categoria | Tipi |
| --- | --- |
| Arma | spada, spadone (a due mani: niente scudo), ascia, mazza |
| Scudo | grande, medio, piccolo |
| Armatura | pesante, media, leggera |
| Amuleto | magico (bonus semplici), arcano (effetti speciali); ogni amuleto ha anche un malus |

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
  - Statistiche nuove per bonus e malus: *Velocita Parata*, *Velocita Attacco*, *Vita Massima Percento* e, nei modificatori, *Armatura Percento*. Ogni amuleto ha un bonus e un malus.
  - Tempo per alzare lo scudo (`GiocatoreControllo`, *Tempo Alzata Scudo*, 0,1 s): prima la parata non ferma i colpi. Le Statistiche hanno *Velocita Parata* per accelerarlo o rallentarlo (malus degli amuleti).
  - `CalcoloDanno` accetta la penetrazione dell'armatura (mazze).

### Per l'inventario di Giuseppe

- Equipaggiare: `GetComponent<Equipaggiamento>().Equipaggia(oggetto)`. Mette l'oggetto nella casella giusta in base al tipo.
- Togliere: `TogliArma()`, `TogliScudo()`, `TogliArmatura()`, `TogliAmuleto()`.
- Cosa c'è addosso: le proprietà `Arma`, `Scudo`, `Armatura` e `Amuleto`; l'evento `Cambiato` avvisa per ridisegnare il menu.
- Testi: `oggetto.Nome` e `oggetto.Descrizione` passano da `Lingua.T`. Le chiavi `oggetto.<nome>.nome` e `oggetto.<nome>.descrizione` di tutti i 19 oggetti le ha già aggiunte Giuseppe in `Lingua.cs`, sul ramo `giuseppec/traduzioni-oggetti`.

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
| 4 | Ascia del boia (`ascia_boia`) | 38 | 30 | 0,50 / 0,52 | 1,9 m / 150° | +10%, totale ×2 | — | 35% (30) | ✔ fatta |
| 5 | Mazza ferrata (`mazza_ferrata`) | 28 | 25 | 0,36 / 0,45 | 1,6 m / 100° | — | 45% | 30% (26) | ✔ fatta |
| 6 | Martello di ossa (`martello_ossa`) | 34 | 28 | 0,42 / 0,50 | 1,7 m / 100° | +1,5% | 45% | 30% (28) | ✔ fatta |
| 7 | Spadone del cavaliere (`spadone_cavaliere`), due mani | 46 | 32 | 0,50 / 0,60 | 2,4 m / 160° | +5% | — | 30% (35) | ✔ fatta |


### Scudi

| # | Oggetto (chiave) | Taglia | Para | Costo parata | Arco | Parata perfetta | Peso | Revisione |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 8 | Scudo della vecchia vita (`scudo_vecchia_vita`) | Medio | 60% | 25 | 120° | — | schivata +5, corsa -3% | ✔ fatta |
| 9 | Pavese di quercia (`pavese_quercia`) | Grande | 85% | 22 | 160° | — | schivata +15, corsa -10% | ✔ fatta |
| 10 | Brocchiere di ferro (`brocchiere_ferro`) | Piccolo | 30% | 25 | 100° | entro 0,15 s: nemico sbilanciato 0,8 s, e intanto i colpi che riceve fanno ×1,5 | schivata +3, corsa -2% | ✔ fatta |

### Armature

| # | Oggetto (chiave) | Peso | Armatura | Peso sulla schivata / corsa | Revisione |
| --- | --- | --- | --- | --- | --- |
| 11 | Giubba di cuoio imbottito (`giubba_cuoio`) | Leggera | 10 | +3,5 / -2% | ✔ fatta |
| 12 | Cotta di maglia rattoppata (`cotta_maglia`) | Media | 25 | +6,5 / -5% | ✔ fatta |
| 13 | Corazza di piastre annerite (`corazza_piastre`) | Pesante | 45 | +12 / -12% | ✔ fatta |

### Amuleti

| # | Oggetto (chiave) | Tipo | Effetto | Revisione |
| --- | --- | --- | --- | --- |
| 14 | Zanna di lupo (`zanna_lupo`) | Magico | +10% danno; malus: -10% di resistenza massima | ✔ fatta |
| 15 | Occhio di corvo (`occhio_corvo`) | Magico | +10% critico; malus: -7% di vita massima | ✔ fatta |
| 16 | Pietra del focolare (`pietra_focolare`) | Magico | +15 armatura; malus: attacchi il 7,5% più lenti | ✔ fatta |
| 17 | Cuore di brace (`cuore_brace`) | Arcano | +9 vita per nemico ucciso; malus: -5% danno | ✔ fatta |
| 18 | Respiro del lago (`respiro_lago`) | Arcano | resistenza +30% più veloce; malus: -8 armatura e -7% di vita massima | ✔ fatta |
| 19 | Sangue antico (`sangue_antico`) | Arcano | 6% del danno inflitto torna come vita; malus: -15% di vita massima | ✔ fatta |

## Bilanciamento del 9 ottobre (Lorenzo)

Dopo i conti su tutte le combinazioni (armi con ogni amuleto, scudi con ogni armatura) contro l'orco sgherro:

| Oggetto | Prima | Ora | Perché |
| --- | --- | --- | --- |
| Ascia del boia | danno 42, carica 0,42 s | danno 38, carica 0,50 s | Uccideva l'orco in 3,1 s, più dello Spadone a due mani; ora 3,8 s (Spadone 3,7 s, Martello di ossa 3,9 s, Ascia da legna 4,2 s) |
| Mazza ferrata | ignora 30% armatura | ignora 45% | Non serviva a niente: più lenta della Spada del capitano e non migliore contro i corazzati |
| Brocchiere di ferro | parata perfetta entro 0,2 s | entro 0,15 s | Con l'avviso rosso dell'orco la parata perfetta era troppo facile |
| Respiro del lago | malus -20% armatura | malus -8 armatura e -7% vita massima | Con un'armatura leggera il malus non pesava |
| Sangue antico | 10% del danno torna come vita | 6% | Colpendo tre orchi insieme si recuperava più vita di quanta se ne perdeva |
| Zanna di lupo | malus: scudo il 5% più lento | malus: -10% di resistenza massima | Il malus pesava solo a chi para con lo scudo |
| Occhio di corvo | malus: scudo il 5% più lento | malus: -7% di vita massima | Come sopra |
| Cuore di brace | +12 vita per nemico ucciso | +9 | Sullo Stregone (amuleti in comune) curava troppo con le uccisioni delle evocazioni |

Regole decise per tutte le classi:
- **Esecuzione furtiva**: boss e miniboss non si eseguono mai. I nemici normali sì, finché sono del nostro livello o inferiore (quando ci saranno i livelli; per ora tutti i nemici normali).
- **Moltiplicatori di danno**: quando se ne sommano più di uno, quelli da ×2 in su perdono la parte dopo la virgola, poi si moltiplicano; quelli sotto ×2 restano come sono (stessa regola di `Docs/incantesimi-stregone.md`).
- **Boss**: avranno regole loro per stordimento, sbilanciamento e simili.

## Prossimi passi

1. ~~Revisione con Lorenzo~~: fatta per tutti i 19 oggetti (7 ottobre).
2. Prova in Unity: menu *Crea oggetti del Guerriero*, poi trascinare gli oggetti nelle caselle di **Equipaggiamento** del Giocatore e combattere l'orco sgherro nel villaggio.
3. Salvare e caricare i `.meta` e i file `.asset` creati da Unity, poi aprire la pull request.
4. Più avanti: inventario e raccolta con E (Giuseppe), modelli e icone (Nazar), traduzioni in `Lingua.cs`.
