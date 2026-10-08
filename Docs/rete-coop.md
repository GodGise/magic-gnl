# Rete co-op: quale sistema usare

Ricerca del 7 ottobre 2026. Obiettivo: co-op da 1 a 3 giocatori su Steam, **gratis** e **solido** (mantenuto da qualcuno di affidabile, che non sparisca a metà progetto).

## Come funziona il co-op che ci serve

- Un giocatore **ospita** la partita sul suo PC (host), gli altri due si collegano a lui tramite Steam (invito fra amici).
- Niente server da pagare: il traffico passa attraverso la rete di Valve (**Steam Datagram Relay**), che nasconde gli indirizzi IP dei giocatori e supera i problemi di router e NAT. Per i giocatori Steam basta usare le funzioni standard di Steam: "Steam si occupa del resto" (documentazione Steamworks).
- Con 3 giocatori al massimo e un gioco co-op (non competitivo) non servono tecniche complicate anti-lag: ognuno comanda il proprio personaggio, l'host comanda i nemici, gli altri vedono le posizioni "ammorbidite".

## Le opzioni confrontate

| Sistema | Costo | Chi lo mantiene | Ultimo aggiornamento (verificato) | Collegamento a Steam | Note |
| --- | --- | --- | --- | --- | --- |
| **Netcode for GameObjects (NGO) 2.13** | Gratis | **Unity** (ufficiale) | 18 settembre 2026 (2.13.3, richiede Unity 6000.0 o più nuovo: va bene per la nostra 6.3) | Trasporto **SteamNetworkingSockets** nel repository ufficiale dei contributi della comunità di Unity (aggiornato l'8 settembre 2026), basato su **Steamworks.NET** (aggiornato agosto 2026) | Niente previsione lato client integrata, ma per il co-op non serve |
| FishNet | Gratis anche per giochi commerciali | Un solo sviluppatore (FirstGearGames) | Settembre 2026 | **FishySteamworks**: ultimo aggiornamento **agosto 2024** | Molte funzioni avanzate, ma il pezzo per Steam è fermo da più di due anni |
| Mirror | Gratis (licenza MIT) | Comunità | Settembre 2026 | **FizzySteamworks**: ultimo aggiornamento luglio 2025 | Solido e usatissimo; la previsione è ancora "in ricerca" |
| Photon Fusion | **A pagamento** oltre la soglia gratuita (utenti contemporanei e banda) | Azienda (Exit Games) | 2026 | Proprio | Escluso: non è gratis |
| Servizi Unity (Relay, Lobby) | Gratis fino a 50 utenti contemporanei medi al mese, poi a pagamento | Unity | 2026 | Non serve se usiamo Steam | Escluso: con Steam non ci serve, e oltre la soglia si paga |

## Proposta

**Netcode for GameObjects 2.13 + trasporto SteamNetworkingSockets + Steamworks.NET.**

Perché:
1. È il sistema **ufficiale di Unity**: documentazione, esempi e aggiornamenti continui. È la scelta meno rischiosa per un progetto di anni.
2. È **gratis** in tutte le sue parti, e con il relay di Steam non ci sono server da pagare.
3. Il "trasporto" (il pezzo che spedisce i dati) è **intercambiabile**: possiamo sviluppare e provare il co-op **in locale** con il trasporto di base di Unity (due finestre di gioco sullo stesso PC, o due PC in casa) senza Steam, e collegare Steam solo quando avremo il nostro App ID.
4. Il trasporto Steam e Steamworks.NET sono aggiornati nel 2026.

Da sapere:
- Le versioni 3.x di NGO richiedono Unity 6000.7: noi restiamo sulla **2.13** finché siamo su Unity 6.3. Quando passeremo a un LTS più nuovo (CLAUDE.md, "Motore"), si valuta l'aggiornamento.
- Per le prove su Steam con amici serve un **App ID** nostro: si ottiene pagando la tassa **Steam Direct di 100 $** per gioco, non rimborsabile ma recuperata dopo 1.000 $ di ricavi. L'App ID di prova 480 ("Spacewar") viene usato da molti per i primi test, ma ci sono segnalazioni di limiti sulle lobby: non ci si conta.

## Cosa cambia nel codice (quando si parte)

Il co-op tocca soprattutto **giocatore, nemici e combattimento**. Ogni oggetto che si muove deve diventare un "oggetto di rete" e ogni colpo deve essere deciso da chi ha l'autorità (l'host per i nemici). Per questo va deciso **prima** di scrivere ancora molto codice di combattimento: più ne scriviamo "solo per un giocatore", più ne va riadattato.

Ordine proposto (stato all'8 ottobre):
1. **[Fatto e provato]** Installare NGO 2.13 e provare due giocatori in locale.
2. **[Fatto e provato]** Movimento e azioni del giocatore in rete (figure degli altri giocatori).
3. **[Fatto e provato]** Combattimento: colpi ai nemici, parata, schivata, vita.
4. **[Fatto e provato]** Nemici guidati dall'host.
5. Steam: Steamworks.NET, trasporto Steam, inviti fra amici (serve l'App ID, si paga quando il gioco è all'80-90%).

## Come funziona adesso (ramo `giuseppec/rete-coop`)

- **Menu**: "Multigiocatore" nel menu iniziale. "Ospita una partita": si sceglie la classe e la partita parte; nella schermata si vedono gli indirizzi del PC da dare agli amici. "Entra in una partita": si scrive l'indirizzo dell'host, si sceglie la classe, ci si collega e la scena dell'host si carica da sola. Fino a 3 giocatori, porta 7777.
- **Dove si gioca senza Steam**: stesso PC (due finestre, 127.0.0.1), stessa casa (indirizzo locale), oppure su internet con una rete privata gratuita (ZeroTier o Radmin VPN) che dà a ognuno un indirizzo come se fossero in casa.
- **Chi decide cosa** (`Assets/Scripts/Rete/`):
  - ogni giocatore comanda il **proprio personaggio** (`GiocatoreControllo`, che non è un oggetto di rete); la sua **figura** (`GiocatoreRete`) copia posizione e azioni e le mostra agli altri, animata come il giocatore vero;
  - i **nemici pensano solo sull'host** (vista, inseguimento, attacchi, vita, morte); `MondoRete` manda a tutti posizioni (12 volte al secondo), eventi (preavviso, colpito, sbilanciato, morto, rinato, "!") e l'ora del giorno;
  - i **colpi ai nemici** di chi non ospita vanno all'host (`Bersaglio.RiceviColpo` lo fa da solo), che li applica e li rimanda a tutti; lo stesso per parate perfette ed esecuzioni furtive;
  - i **colpi dei nemici** a un giocatore arrivano al suo PC: lì si decide se ha parato o schivato e quanto danno prende (con la sua armatura);
  - il premio dell'uccisione (mana, vita degli amuleti) va a chi ha dato l'ultimo colpo; le sfere magiche si vedono da tutti.
- **Regole per chi scrive codice nuovo**: i nemici cercano i giocatori in `ObiettiviNemici` (non con `FindFirstObjectByType<GiocatoreControllo>`); il danno ai nemici passa sempre da `Bersaglio.RiceviColpo`; per sapere se si comanda il mondo si usa `Rete.ComandaIlMondo` (vero anche da soli); in pausa, in rete, il tempo non si ferma.
- **Oggetti delle zone condivisi** (8 ottobre, ramo `giuseppec/livelli-coop`, deciso da Giuseppe): porte, leve, bauli, chiavi, muri crepati e oggetti da raccogliere sono uguali per tutti. Chi li usa, se non ospita, lo chiede all'host (`MondoRete.ChiediUso`); l'host li usa e lo dice a tutti (`MondoRete.InviaEvento`); chi entra dopo riceve il loro stato. Regole: il contenuto di un baule e un oggetto raccolto vanno a chi li prende; una chiave raccolta vale per tutto il gruppo. Le **trappole** scattano per chiunque ci passi sopra, ma ogni PC dà il danno solo al suo giocatore. I **checkpoint** restano personali (ognuno rinasce dove ha acceso il suo).
- **Nuovi oggetti delle zone**: se qualcosa cambia lo stato del mondo (si apre, si rompe, sparisce), deve implementare `IOggettoCondiviso` (in `Assets/Scripts/Rete/Rete.cs`) come fanno `Porta` e `Leva`. La figura degli altri giocatori è ancora quella provvisoria a blocchi.

## Fonti

- Netcode for GameObjects, manuale: https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.8/manual/index.html
- Repository NGO (versioni e requisiti): https://github.com/Unity-Technologies/com.unity.netcode.gameobjects
- Trasporto SteamNetworkingSockets per NGO: https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/Transports/com.community.netcode.transport.steamnetworkingsockets
- Steamworks.NET: https://github.com/rlabrecque/Steamworks.NET
- Steam Datagram Relay: https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay
- Tassa Steam Direct: https://partner.steamgames.com/doc/gettingstarted/appfee
- FishySteamworks: https://fish-networking.gitbook.io/docs/fishnet-building-blocks/transports/fishysteamworks
- Licenza FishNet: https://fish-networking.gitbook.io/docs/overview/readme/legal-restrictions
- Prezzi Unity Relay: https://support.unity.com/hc/en-us/articles/4410136449812-How-is-the-Relay-Service-Priced
- Confronto 2026 degli stack di rete per Unity: https://dev.to/gamedevtoollab/choosing-the-right-real-time-networking-stack-for-unity-in-2026-29f4
- Discussione sull'App ID 480: https://forum.godotengine.org/t/if-i-want-to-test-the-online-multiplayer-functionality-using-steam-do-i-need-to-register-a-game-or-will-using-the-480-app-be-sufficient/131625
