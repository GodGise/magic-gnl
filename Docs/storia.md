# Storia del gioco (bozza 2)

Testo di Giuseppe, sistemato con Claude. È la storia da raccontare: le scelte di gioco che ne derivano sono in fondo, nella sezione "Note di design".

## Il racconto

C'era una volta, in una terra lontana e ormai stanca, un mondo dove i grandi regni erano caduti uno dopo l'altro. I castelli erano diventati rovine coperte di muschio, le strade erano sparite sotto l'erba, e ognuno cercava soltanto di arrivare vivo al giorno dopo. In quel mondo vivevano creature di ogni tipo: orchi feroci con le zanne storte, elfi silenziosi che non si fidavano di nessuno, mutaforma che potevano diventare lupi o corvi, stregoni che parlavano con il fuoco e golem di pietra che dormivano per secoli. La magia esisteva davvero, ma la gente ne aveva paura, perché chi la usava spesso non tornava più lo stesso.

Gli uomini vivevano in piccoli villaggi, lontani l'uno dall'altro. Uno di questi era Villaggio Lago Nero. Era un villaggio antichissimo, con le case di legno e i tetti di paglia, costruito sulla riva di un lago così scuro che di notte sembrava un buco nel mondo. Nessuno sapeva perché l'acqua fosse nera. I bambini ci lanciavano i sassi per sentire il "plof", ma nessuno ci faceva mai il bagno. I vecchi del villaggio, la sera, davanti al fuoco, raccontavano sempre la stessa storia: "Sotto il lago dorme qualcosa. I nostri antenati costruirono il villaggio qui per fare la guardia". Ma ormai nessuno ci credeva più. Era solo una storia per far paura ai piccoli.

In quel villaggio viveva il nostro eroe. Chi fosse prima di quella notte lo decide il giocatore all'inizio del gioco: un guerriero che aveva appeso la spada al chiodo, un ladro che aveva smesso di rubare, oppure uno stregone che aveva giurato di non usare più la magia. In ogni caso aveva scelto una vita semplice: le mani ruvide dal lavoro, una moglie che rideva forte e un figlio piccolo che lo seguiva dappertutto. Per lui il mondo intero era quello: la sua casa, la sua famiglia e il lago nero davanti alla porta.

Poi, una notte senza luna, i cani cominciarono ad abbaiare tutti insieme. Dal bosco arrivò un rumore di tamburi, sempre più forte. E dagli alberi uscirono gli orchi. Erano tantissimi, con le torce in mano e gli occhi gialli che brillavano nel buio. Il villaggio non aveva mura né soldati, e nessuno era pronto. Gli orchi sfondarono le porte, bruciarono i tetti e trascinarono via la gente. Ma c'era una cosa strana: non prendevano solo il cibo e l'oro. Rovesciavano le case, scavavano sotto il tempio, aprivano perfino le tombe. Stavano cercando qualcosa.

Il nostro eroe corse fuori con la prima cosa che trovò, un'ascia da legna, per proteggere la sua famiglia. Vide cadere sua moglie, e non riuscì ad arrivare in tempo. Nel fumo e nelle urla, suo figlio sparì. Davanti a lui comparve un orco enorme, alto come una porta, con un'armatura fatta di ossa. Il nostro eroe combatté con tutte le sue forze, ma l'orco era troppo forte. Un colpo, il buio, e poi più niente.

Quando riaprì gli occhi, faceva freddo e c'era odore di terra bagnata. Era sotto terra, chiuso in una cella con le sbarre di ferro arrugginito, in fondo alla tana degli orchi. Intorno a lui c'erano altre persone del villaggio: alcune piangevano, altre non si muovevano più. Allora capì la cosa più terribile: gli orchi non lo avevano preso come prigioniero. Lo tenevano lì come si tiene il cibo in dispensa, per quando avrebbero avuto fame.

Ma il nostro eroe non si arrese. Trovò il modo di aprire la cella e iniziò a scappare: lungo corridoi bui dove si sentivano le guardie russare, giù per sotterranei pieni di catene e di ossa, e poi in grotte enormi, dove l'acqua gocciolava dal soffitto e strane cose si muovevano nell'ombra. Mentre fuggiva frugò nel bottino che gli orchi avevano ammucchiato, e lì, fra le cose rubate ai villaggi, ritrovò le armi della sua vecchia vita: una spada e uno scudo, oppure un arco e un pugnale, oppure un bastone incantato e un libro di magia. Ogni passo poteva essere l'ultimo. Alla fine vide una luce lontana, corse verso di lei e uscì all'aria aperta.

Tornò al villaggio più veloce che poteva, ma di Villaggio Lago Nero era rimasta solo cenere. Le case erano bruciate, il tempio era crollato. Trovò sua moglie e la seppellì con le sue mani, vicino al lago. Poi cercò suo figlio dappertutto, fra le macerie e nel bosco. Ma non lo trovò. E proprio lì, fra le rovine, incontrò pochi sopravvissuti nascosti, e uno di loro gli disse: "Ho visto gli orchi portare via dei bambini. Forse tuo figlio è ancora vivo".

Da quel momento il nostro eroe ebbe un solo pensiero: ritrovarlo. Si mise in cammino verso le due tane degli orchi nascoste nella regione, deciso a entrarci e a cercarlo, a costo della vita. Fu un viaggio pieno di pericoli e di combattimenti. In fondo a quelle tane scoprì due segreti. Il primo era la verità su suo figlio. Il secondo era il vero motivo per cui gli orchi avevano attaccato il villaggio. Non erano venuti per rubare o per uccidere: erano venuti per prendere una reliquia antica, nascosta per secoli sotto Villaggio Lago Nero. L'avevano trovata, e adesso la stavano portando nella loro tana più grande, per consegnarla al loro padrone.

Il padrone degli orchi non era un orco. Era un druido malvagio, un vecchio stregone della natura che viveva lassù, sulle montagne coperte di neve, dove il vento urla giorno e notte. Era lui che aveva mandato gli orchi. Era lui che voleva la reliquia. E nessuno sapeva ancora che cosa ne avrebbe fatto.

Così il nostro eroe si trovò davanti alla scelta più difficile della sua vita. Poteva inseguire gli orchi e strappare loro la reliquia prima che arrivasse nelle mani del druido, per fermare qualcosa che forse avrebbe potuto distruggere il mondo intero. Oppure poteva lasciar perdere tutto il resto e salire sulle montagne innevate per trovare il druido e fargliela pagare, per la moglie, per il figlio e per il suo villaggio.

Due strade, due destini diversi. E a scegliere, questa volta, sarai tu.

## Note di design

### Classi e armi
- All'inizio il giocatore sceglie la classe: **Guerriero**, **Ladro** o **Stregone**.
- Durante la fuga dalla gattabuia trova l'arma della sua classe nel bottino degli orchi:
  - Guerriero: spada e scudo.
  - Ladro: arco e pugnale.
  - Stregone: bastone incantato e libro.
- Prima di trovarla combatte con l'ascia da legna (o a mani nude).

### Ordine delle regioni
0. **La gattabuia**: tana sotterranea degli orchi, fuga e tutorial.
1. **Villaggio Lago Nero**: rovine, sopravvissuti, base a cui tornare.
2. e 3. **Le due tane degli orchi**: nell'ordine che sceglie il giocatore.
4. **La tana principale**: dove viene portata la reliquia (solo nella strada A).
5. **Le montagne innevate**: il druido.

### La scelta finale
- **Strada A, intercettare gli orchi.** Si va alla tana principale e si recupera la reliquia. Il druido diventa una missione secondaria (side quest) e si affronta in versione normale.
- **Strada B, la vendetta.** Si va direttamente dal druido. Il druido ottiene la reliquia prima dello scontro: ha **il triplo della vita** e attacchi più forti.

### Ancora da decidere
- Che potere ha la reliquia.
- La verità sul figlio: vivo o morto.
- Nella strada A, se il druido è una missione secondaria, come finisce la storia principale.
- Il nome del protagonista (o se lo sceglie il giocatore).
