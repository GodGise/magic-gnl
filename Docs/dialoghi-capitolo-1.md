# Dialoghi del giorno a Villaggio Lago Nero (capitolo 1, bozza 2: rivista da Lorenzo)

Scritti dal Claude di Giuseppe il 10 ottobre 2026 sulla base di `Docs/quest-capitolo-1.md`. **Sono in italiano e da approvare**: quando sono approvati si traducono nelle 8 lingue e si mettono in `Lingua.cs`. Tono: caldo e un po' scherzoso in famiglia, più cupo con gli altri. Righe di dialogo: circa 90. Il protagonista non parla (il suo nome è ancora aperto); i testi in *corsivo tra parentesi* sono suggerimenti a schermo del tutorial.

**Tasti nei suggerimenti (revisione di Lorenzo):** i tasti si cambiano in Opzioni > Comandi, quindi nei suggerimenti non si scrive il tasto ma il nome dell'azione fra graffe, come nell'enum `Azione` di `Comandi.cs`: `{Avanti}`, `{Indietro}`, `{Sinistra}`, `{Destra}`, `{Corsa}`, `{Schiva}`, `{Attacca}`, `{Para}`, `{Interagisci}`, `{Abilita}`, `{Aggancia}`, `{Inventario}`. Il codice del dialogo mette al loro posto il tasto scelto dal giocatore (`Comandi.NomeTasto`). Restano scritti per esteso solo i tasti fissi: mouse per la camera, rotellina e 1-6 per gli incantesimi.

**Co-op e personaggi (decisi da Lorenzo, da confermare da Giuseppe):** la famiglia (Maren e Tobin) è dell'**host**: "papà" è solo l'host; gli altri giocatori sentono le stesse battute. Per ora **tutti i personaggi giocabili sono uomini**.

## Quest 1 - Un'altra mattina
*Casa dell'eroe, mattina. Il giocatore può muoversi. Maren è vicino al camino, Tobin sul letto.*

- *(Muoviti con {Avanti} {Sinistra} {Indietro} {Destra}. Gira la camera con il mouse.)*
- **Tobin:** Papà, papà! Il sole è già alto e tu dormi ancora!
- **Maren:** Lascialo, ha sognato di nuovo il lago. Lo so dalla faccia.
- *(L'eroe si alza.)*
- **Maren:** Eccoti, finalmente. Il fuoco si sta spegnendo e il capanno è pieno di legna che nessuno porta dentro.
- *(Avvicinati a una persona e premi {Interagisci} per parlarle.)*
- **Tobin:** Posso venire anch'io? Io porto i pezzi piccoli!
- **Maren:** Resta dove ti vedo, ometto. Il lago oggi è strano: ha un colore più scuro del solito.
- **Maren:** Prendi la legna, poi passa dal fabbro. Brannoc ha detto che ti cercava. Non fargli aspettare, ha un carattere peggio di un tasso.
- *(Quest: porta la legna dal capanno a casa.)*
- **Tobin** *(al capanno):* Quella è troppo grossa! Tienila tu, io faccio la guardia.
- **Maren** *(quando torni con la legna):* Bravo. Adesso vai, prima che Brannoc si metta a gridare.

## Quest 2 - Prova la lama (Guerriero e Ladro)
*Fucina di Brannoc. Dietro c'è un manichino; per il Ladro anche dei bersagli di tiro. Brannoc ti dà un'arma semplice da villaggio.*

- **Brannoc:** Sei in ritardo. Non importa. Prendi questa: una lama (un arco, per il Ladro) vecchia, ma ancora buona.
- **Brannoc:** Dicono che gli orchi si fanno vedere nei boschi. Io non ci credo, ma un uomo con una lama in mano dorme meglio.
- **Brannoc:** Vediamo se sai ancora stare in piedi. Il manichino non si muove, ma tu sì.
- *(Attacca con {Attacca}. Costa resistenza.)*
- *(Tieni premuto {Para} per parare. Un colpo parato fa meno danno.)*
- *(Premi {Schiva} per schivare. Per un attimo i colpi non ti toccano.)*
- *(Premi {Aggancia} per agganciare il bersaglio. Premilo di nuovo per sganciarlo.)*
- *(Con un bersaglio agganciato, muovi il mouse verso un altro nemico per passare a lui.)*
- **Brannoc** *(dopo qualche colpo):* Non male. Hai la mano di uno che non ha dimenticato.
- **Brannoc** *(per il Ladro, ai bersagli):* L'arco è bravo quando chi lo tiene è fermo. Tu muoviti, ma tira quando respiri.
- **Brannoc:** Se hai voglia di fare un po' di pratica ancora, il manichino è tuo. Poi vai dai ratti: Hobb dice che il granaio ne è pieno.

## Quest 2 - Lo studio delle magie (Stregone)
*Tempio, sala di studio. Il vecchio Edric tiene dei bersagli di prova. Il protagonista aveva giurato di non usare più la magia.*

- **Edric:** Ti ho visto passare davanti al tempio per tre anni senza mai entrare. Eppure sei qui.
- **Edric:** Maren mi ha detto che ti manca. Che di notte muovi le dita come se tenessi qualcosa.
- **Edric:** Non ti chiedo di rompere il tuo giuramento. Ti chiedo solo di ricordarti come si fa. Prendi questo bastone: l'ho tenuto da parte per anni.
- **Edric:** Uno studioso non ha paura del fuoco, ha paura di non saperlo governare. Prova con la Scintilla.
- *(Lancia un incantesimo con {Attacca}. Ogni incantesimo costa mana.)*
- *(Cambia incantesimo con la rotellina o con i tasti da 1 a 6.)*
- **Edric:** Ora il ghiaccio. L'acqua del lago ha una memoria: ascolta quella.
- *(La barra blu è il mana: si ricarica da sola poco dopo l'ultimo incantesimo.)*
- *(Fai qualche passo indietro: lo Stregone resiste meglio a distanza.)*
- **Edric:** Bene. Ricordi tutto. È come tornare a casa, vero?
- **Edric:** Tienilo. Non so se ti servirà. Spero di no.

## Quest 3 - Ratti nel granaio
*Granaio sul lago. Pochi ratti giganti; in co-op ne arrivano di più.*

- **Hobb:** Eccoti! Ho una cantina che sembra un campo di battaglia e dei ratti grossi come cani.
- **Hobb:** Un tempo non erano così. Da quando il lago è diventato più scuro, tutto quello che vive vicino all'acqua è... cresciuto.
- **Hobb:** Non devi ucciderli tutti. Solo abbastanza da farmi dormire.
- *(Ogni azione costa resistenza. Se la barra è vuota, aspetta che si ricarichi.)*
- *(Schiva i colpi nemici: guarda il preavviso rosso prima dell'attacco.)*
- **Hobb** *(alla fine):* Un grazie sentito. Domani ti offro una birra... anzi no, domani te ne offro due!

## Quest 4 - Le erbe di Ilse
*Casa di Ilse, poi riva del lago.*

- **Ilse:** Ah, il padre di Tobin. Entra, entra, ma non toccare niente.
- **Ilse:** Mi servono tre fiori di riva. Crescono solo dove l'acqua è nera. Io alla mia età non mi bagno più i piedi.
- **Ilse:** Non fidarti dell'acqua. Io la guardo da una vita e non mi ha mai detto niente di buono.
- *(Premi {Interagisci} per raccogliere un oggetto.)*
- **Ilse** *(quando le porti i fiori):* Perfetto. Ecco una pozione. Non è per ora, è per quando ne avrai bisogno.
- *(Premi {Inventario} per aprire l'inventario. Seleziona la pozione e usala.)*
- **Ilse:** Da bambina ho visto uscire qualcosa dal lago. A nessuno ho mai detto cosa.

## Quest 5 - La leggenda del lago (facoltativa)
*Tempio, poi cimitero e cripta. Una porta con leva, un baule.*

- **Edric:** Chiedono sempre perché il lago è nero. Nessuno chiede mai da quanto tempo.
- **Edric:** I nostri antenati non costruirono qui il villaggio per caso. Sotto il lago dorme qualcosa, e loro erano la guardia.
- **Edric:** Nella cripta, sotto il cimitero, c'è un oggetto che ci ha lasciato la prima guardia. Non l'ho mai preso. Non ho mai avuto il coraggio.
- *(Premi {Interagisci} su una leva per tirarla. Una porta si aprirà.)*
- *(Premi {Interagisci} su un baule per aprirlo.)*
- **Edric** *(quando torni):* L'hai trovato. È bello da vedere, non è vero? Sembra quasi vivo.
- **Edric:** Tienilo. La guardia è finita da tempo. Forse ne servirà una nuova.

## Quest 6 - Una sera in taverna
*Taverna di Hobb, tramonto. Poi il suono dei tamburi.*

- **Hobb:** Siediti. Stasera la birra è buona e le notizie sono cattive.
- **Hobb:** Un mercante è passato dal bosco. Dice di aver visto fuochi nella foresta, e di aver sentito dei tamburi.
- **Hobb:** Io gli ho detto che era ubriaco. Lui mi ha risposto che non beve mai.
- **Maren** *(entrando con Tobin addormentato in braccio):* Eccoti. Ti ho cercato dappertutto. Andiamo a casa, qui fa un caldo che non si respira.
- **Maren:** Ho sentito la storia dei tamburi. Domani ne parliamo, va bene? Stasera... stasera restiamo insieme.
- *(In lontananza: tamburi.)*
- **Hobb:** ...Avete sentito anche voi?
- **Maren:** Dentro. Tutti dentro.
- *(Da qui parte la razzia.)*

## Note
- Totale: circa 90 righe, con i suggerimenti. Le quest 3 e 5 si tagliano per prime.
- Le battute di Ilse e di Edric anticipano il mistero del lago senza spiegarlo.
- Il protagonista non ha battute: serve decidere se parla (e come si chiama).
- **Pozione (quest 4):** gli oggetti da usare e consumare non esistono ancora nel codice; si aggiungono strada facendo (Lorenzo, 10 ottobre).
