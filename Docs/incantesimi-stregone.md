# Incantesimi dello Stregone

Nuovo sistema dello Stregone, deciso da Lorenzo il 9 ottobre 2026 (bozza, da approvare da Giuseppe: vedi `Docs/da-approvare.md`).
Sostituisce l'idea dei bastoni che lanciano tutti la stessa sfera (`Docs/oggetti-stregone.md`).

## L'idea

- Lo Stregone ha un **inventario di incantesimi**. Gli incantesimi sono oggetti che si trovano nel mondo, come le armi.
- Ne porta con sé un certo numero in delle **caselle** e in combattimento passa dall'uno all'altro.
- **Bastoni** e **libri** non fanno danno da soli: aiutano gli incantesimi, ognuno con un pro e un contro (per esempio un bastone che rinforza una scuola e ne indebolisce un'altra). Il libro decide anche quante caselle si hanno.
- **Amuleti** come per le altre classi: magici e arcani, ognuno con un bonus e un malus.
- Ogni incantesimo costa mana. Il mana si ricarica da solo (`Docs/mana.md`).

## Parole usate nelle tabelle

- **Carica**: tempo dal tasto premuto al lancio. In questo tempo l'incantesimo non è ancora partito.
- **Recupero**: tempo dopo il lancio in cui non si lancia altro (si può schivare).
- **Attesa**: tempo prima di poter rilanciare **lo stesso** incantesimo. Parte nel momento del lancio. Gli altri incantesimi si possono usare.

## Regola dei moltiplicatori di danno

Quando più moltiplicatori si sommano, quelli da ×2 in su perdono la parte dopo la virgola, poi si moltiplicano tra loro. Quelli sotto ×2 restano come sono.
Esempio: Dardo d'ombra (×2 se non visto) lanciato dal Passo d'ombra (×2,5) = ×2 per ×2 = **×4**. Da solo, il ×2,5 resta ×2,5.

## Brace (fuoco): tanto danno, anche ad area

| Incantesimo | Mana | Danno | Carica | Recupero | Attesa | Effetto |
| --- | --- | --- | --- | --- | --- | --- |
| Scintilla (base) | 5 | 21 | 0,3 s | 0,3 s | 0,5 s | Piccola sfera di fuoco veloce verso il nemico |
| Palla di fuoco | 18 | 32 | 0,7 s | 0,5 s | 1,3 s | Sfera lenta che esplode: colpisce tutti i nemici entro 3 m |
| Scia di brace | 22 | 11 al secondo | 0,7 s | 0,6 s | 10 s | Striscia di fuoco a terra davanti, lunga 6 m, accesa per 6 s: brucia chi ci passa |

## Lago Nero (acqua e ghiaccio): controllo

| Incantesimo | Mana | Danno | Carica | Recupero | Attesa | Effetto |
| --- | --- | --- | --- | --- | --- | --- |
| Scheggia di ghiaccio (base) | 7 | 15 | 0,3 s | 0,3 s | 1,5 s | Proiettile veloce; il nemico colpito rallenta del 35% per 2,7 s |
| Onda del lago | 20 | 20 | 0,5 s | 0,6 s | 10 s | Onda alta 4 m che arriva fino a 6 m davanti. I nemici più bassi di 4 m vengono spinti indietro di 5 m e storditi per 1,5 s. Quelli più alti rallentano del 30% per 2 s e prendono il 20% di danno in meno per ogni metro sopra i 4 m (da 9 m in su nessun danno) |
| Pozza nera | 27 | 3 (una volta) | 0,3 s | 0,5 s | 20 s | Pozza larga 4 m dove si mira: chi è dentro resta bloccato per 3,5 s, poi rallenta del 20% per altri 3 s |

## Ombra: colpire senza farsi vedere, scappare

| Incantesimo | Mana | Danno | Carica | Recupero | Attesa | Effetto |
| --- | --- | --- | --- | --- | --- | --- |
| Dardo d'ombra (base) | 8 | 20 | 0,2 s | 0,3 s | 1 s | Proiettile rapidissimo; danno ×2 se il nemico non ti vede |
| Passo d'ombra | 40 | — | 0,1 s | 0,2 s | 30 s | Invisibile per 4 s e +40% di velocità (già dal primo secondo, verso la fine scende piano alla normale). Il primo incantesimo lanciato da invisibile fa danno ×2,5 e fa finire l'effetto: si torna visibili e si va il 50% più piano per 2 s. Se si lasciano finire i 4 s senza lanciare, nessun malus |
| Velo di nebbia | 22 | — | 0,4 s | 0,4 s | 20 s | Nebbia larga 5 m attorno a te per 6 s: i nemici ti vedono solo da vicinissimo e chi ti inseguiva ti perde |

## Evocazione: aiutanti che combattono per te

La durata delle evocazioni si allunga con libri e amuleti che hanno "Durata evocazioni".

| Incantesimo | Mana | Carica | Recupero | Attesa | Effetto |
| --- | --- | --- | --- | --- | --- |
| Fuoco fatuo (base) | 15 | 0,7 s | 0,4 s | 7 s | Luce che ti segue per 20 s e ogni 1,5 s lancia una scintilla da 8 al nemico più vicino. Al massimo 3 insieme |
| Spirito del lupo | 35 | 2 s | 0,6 s | 1 min | Lupo spettrale per 45 s: morso da 14 ogni 1,2 s, 60 di vita, +10 vita per ogni nemico ucciso da lui (solo da lui). Lascia dietro di sé una scia lunga 1 m che fa 5 di danno al secondo ai nemici che ci passano. Uno solo per giocatore |
| Bambola di ossa | 40 | 0,5 s | 0,5 s | 1 min | 150 di vita, non attacca: i nemici entro 8 m attaccano lei. Se viene distrutta scoppia in una nube di gas larga 1,5 m: i nemici dentro rallentano del 35% e prendono 6 di danno al secondo, per 3 s. Se nessuno la distrugge in 20 s si anima in uno scheletro (45 di vita, 11 di danno ogni 1,6 s al nemico più vicino) che dura finché non lo uccidono, al massimo 30 s; lo scheletro morendo non fa gas. Una sola per giocatore |

## Caselle degli incantesimi

- **4 caselle** di base. Alcuni libri ne danno una o due in più, con un contro.
- Si sceglie l'incantesimo con i tasti numerici, uno per casella (da **1** fino a **6** con le caselle in più); si lancia con il **tasto sinistro**.
- In basso a destra si vede l'incantesimo scelto, con l'attesa che si svuota come una clessidra.
- Oggi i tasti 1 e 2 cambiano fra spada e bastone: per lo Stregone diventano le caselle degli incantesimi, per Guerriero e Ladro restano come sono.

## Bastoni

Non fanno danno da soli: cambiano gli incantesimi, con un pro e un contro.

| Bastone | Pro | Contro |
| --- | --- | --- |
| Bastone della vecchia vita (di partenza) | Nessuno | Carica di ogni incantesimo +0,5 s |
| Bastone di quercia nera | Brace: +15% di danno | Lago Nero: effetti (rallentamenti, blocchi, stordimenti) il 25% più corti |
| Bastone del lago (a due mani: niente libro) | Lago Nero: rallentamenti e blocchi il 30% più lunghi, +15% di danno | Niente libro; tutti gli incantesimi costano il 17% di mana in più |
| Bastone d'ossidiana | Ombra: ignora il 25% dell'armatura nemica | Tutti gli incantesimi costano il 15% di mana in più |
| Bastone del cimitero | Evocazioni: +25% di vita e di durata | Incantesimi che fanno danno: -20% |
| Verga d'osso | Tutti gli incantesimi costano il 15% di mana in meno | -5% di danno; carica e recupero il 10% più lunghi |

## Libri

Stanno nella seconda casella, al posto dello scudo. Con il Bastone del lago (a due mani) non si usano.

| Libro | Pro | Contro |
| --- | --- | --- |
| Libro della vecchia vita (di partenza) | +30 mana massimo | Nessuno |
| Libro dei sussurri | +2 caselle per gli incantesimi (6 in tutto, tasti da 1 a 6) | Il mana si ricarica il 25% più piano |
| Libro delle braci | Attesa di tutti gli incantesimi il 30% più corta | -20 mana massimo |
| Libro dell'evocatore | Evocazioni: durano il 30% in più | Attesa di tutti gli incantesimi il 10% più lunga; il mana si ricarica il 7% più piano |
| Libro del lago nero | +1 casella (5 in tutto) e +20 mana massimo | Ogni schivata costa 10 di resistenza in più; tutti gli incantesimi costano l'8% di mana in più |

## Vesti

L'armatura dello Stregone: protegge poco, il pro sta nella magia o nella schivata.

| Veste | Armatura | Pro | Contro |
| --- | --- | --- | --- |
| Tunica stracciata (leggera, di partenza) | 3 | Ogni schivata costa 5 di resistenza in meno; il mana si ricarica il 5% più in fretta | Ogni colpo che fa danno (anche parato) toglie il 10% del mana massimo |
| Veste dell'evocatore (media) | 6 | +10% di mana massimo; carica degli incantesimi di evocazione il 15% più corta | Corsa il 3% più lenta; incantesimi -5% di danno |
| Manto di cenere (pesante) | 10 | +20 mana massimo; incantesimi +10% di danno | Corsa il 5% più lenta; ogni schivata costa anche 5 di mana |

## Amuleti

Come per le altre classi: magici (bonus semplici) e arcani (effetti speciali), ognuno con un malus.

| Amuleto | Tipo | Bonus | Malus |
| --- | --- | --- | --- |
| Osso inciso | Magico | +15 mana massimo | -7% di vita massima |
| Cristallo opaco | Magico | Il mana si ricarica il 10% più in fretta | Carica degli incantesimi il 10% più lunga |
| Cenere benedetta | Magico | Incantesimi +12% di danno | -10% di mana massimo |
| Sigillo del focolare | Arcano | +8 mana per ogni nemico ucciso | -5% di danno |
| Occhio del lago | Arcano | Si possono avere due Spiriti del lupo e due Bambole di ossa insieme | -25% di vita massima; -15% di resistenza massima |
| Cuore del lago nero | Arcano | Il 5% del danno degli incantesimi torna come mana; +6 vita per ogni nemico ucciso | -30% di vita massima; il mana si ricarica il 10% più piano |

## Ancora da decidere

- Velocità dei proiettili e portata di ogni incantesimo.
- Il danno base è più lento della spada del Guerriero (Scintilla: circa 7,5 s per l'orco sgherro contro 4,8 s): da recuperare con bastoni e amuleti, o da rivedere giocando.
