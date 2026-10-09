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

- **4 caselle** di base. Alcuni libri danno una casella in più, con un contro.
- Si sceglie l'incantesimo con i tasti **1, 2, 3, 4** (e **5** con la casella in più); si lancia con il **tasto sinistro**.
- In basso a destra si vede l'incantesimo scelto, con l'attesa che si svuota come una clessidra.
- Oggi i tasti 1 e 2 cambiano fra spada e bastone: per lo Stregone diventano le caselle degli incantesimi, per Guerriero e Ladro restano come sono.

## Ancora da decidere

- Bastoni, libri, vesti e amuleti con i loro pro e contro.
- Velocità dei proiettili e portata di ogni incantesimo.
- Il danno base è più lento della spada del Guerriero (Scintilla: circa 7,5 s per l'orco sgherro contro 4,8 s): da recuperare con bastoni e amuleti, o da rivedere giocando.
