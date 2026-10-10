using System;
using System.Collections.Generic;
using UnityEngine;

// Lingue del gioco e traduzione dei testi che vede il giocatore.
// Lingue: italiano, inglese, spagnolo, francese, tedesco, portoghese (Brasile), russo, cinese semplificato.
// Al primo avvio sceglie la lingua del computer (se non è fra queste, l'inglese); poi ricorda quella scelta
// nelle opzioni del menu.
// Come si usa dal codice: Lingua.T("menu.nuova") restituisce "Nuova partita", "New game", ... secondo la lingua.
// Per un testo nuovo: aggiungere una riga nella tabella qui sotto, con le 8 traduzioni nell'ordine delle lingue.
// Se una traduzione manca (null), si usa l'inglese.
// Come montarlo: non si monta su nessun oggetto, si usa direttamente dal codice.
public static class Lingua
{
    public static readonly string[] Nomi =
        { "Italiano", "English", "Español", "Français", "Deutsch", "Português (Brasil)", "Русский", "简体中文" };
    public static readonly string[] Codici = { "it", "en", "es", "fr", "de", "pt-BR", "ru", "zh-Hans" };

    const string Chiave = "Lingua";
    const int Inglese = 1;

    // Avvisa chi deve ridisegnare i testi quando la lingua cambia.
    public static event Action Cambiata;

    public static int Indice
    {
        get
        {
            if (!PlayerPrefs.HasKey(Chiave)) return LinguaDelSistema();
            return Mathf.Clamp(PlayerPrefs.GetInt(Chiave), 0, Nomi.Length - 1);
        }
        set
        {
            int nuovo = ((value % Nomi.Length) + Nomi.Length) % Nomi.Length;
            PlayerPrefs.SetInt(Chiave, nuovo);
            PlayerPrefs.Save();
            Cambiata?.Invoke();
        }
    }

    public static string NomeAttuale => Nomi[Indice];

    public static string T(string chiave)
    {
        if (!testi.TryGetValue(chiave, out var traduzioni)) return chiave;
        int i = Indice;
        if (i < traduzioni.Length && !string.IsNullOrEmpty(traduzioni[i])) return traduzioni[i];
        return traduzioni.Length > Inglese && !string.IsNullOrEmpty(traduzioni[Inglese]) ? traduzioni[Inglese] : chiave;
    }

    static int LinguaDelSistema()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Italian: return 0;
            case SystemLanguage.English: return 1;
            case SystemLanguage.Spanish: return 2;
            case SystemLanguage.French: return 3;
            case SystemLanguage.German: return 4;
            case SystemLanguage.Portuguese: return 5;
            case SystemLanguage.Russian: return 6;
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified: return 7;
            default: return Inglese;
        }
    }

    //                      italiano, inglese, spagnolo, francese, tedesco, portoghese, russo, cinese
    static readonly Dictionary<string, string[]> testi = new Dictionary<string, string[]>
    {
        ["menu.premi"] = new[] { "Premi un tasto", "Press any key", "Pulsa cualquier tecla", "Appuyez sur une touche", "Drücke eine beliebige Taste", "Pressione qualquer tecla", "Нажмите любую клавишу", "按任意键" },
        ["menu.nome_provvisorio"] = new[] { "nome provvisorio", "working title", "título provisional", "titre provisoire", "Arbeitstitel", "título provisório", "рабочее название", "暂定名" },
        ["menu.nuova"] = new[] { "Nuova partita", "New game", "Nueva partida", "Nouvelle partie", "Neues Spiel", "Novo jogo", "Новая игра", "新游戏" },
        ["menu.continua"] = new[] { "Continua", "Continue", "Continuar", "Continuer", "Fortsetzen", "Continuar", "Продолжить", "继续" },
        ["menu.opzioni"] = new[] { "Opzioni", "Options", "Opciones", "Options", "Optionen", "Opções", "Настройки", "选项" },
        ["menu.crediti"] = new[] { "Crediti", "Credits", "Créditos", "Crédits", "Credits", "Créditos", "Титры", "制作人员" },
        ["menu.esci"] = new[] { "Esci", "Quit", "Salir", "Quitter", "Beenden", "Sair", "Выход", "退出" },
        ["menu.indietro"] = new[] { "Indietro", "Back", "Volver", "Retour", "Zurück", "Voltar", "Назад", "返回" },
        ["menu.aiuto"] = new[]
        {
            "↑ ↓  scegli      Invio  conferma      Esc  indietro      ◄ ►  regola",
            "↑ ↓  select      Enter  confirm      Esc  back      ◄ ►  adjust",
            "↑ ↓  elegir      Intro  confirmar      Esc  volver      ◄ ►  ajustar",
            "↑ ↓  choisir      Entrée  valider      Échap  retour      ◄ ►  régler",
            "↑ ↓  wählen      Enter  bestätigen      Esc  zurück      ◄ ►  einstellen",
            "↑ ↓  escolher      Enter  confirmar      Esc  voltar      ◄ ►  ajustar",
            "↑ ↓  выбор      Enter  подтвердить      Esc  назад      ◄ ►  настроить",
            "↑ ↓  选择      Enter  确认      Esc  返回      ◄ ►  调整",
        },
        ["menu.crediti_testo"] = new[] { "Work in progress", null, null, null, null, null, null, null },
        ["menu.salta"] = new[] { "Spazio  salta", "Space  skip", "Espacio  saltar", "Espace  passer", "Leertaste  überspringen", "Espaço  pular", "Пробел  пропустить", "空格键  跳过" },

        ["pausa.titolo"] = new[] { "Pausa", "Paused", "Pausa", "Pause", "Pause", "Pausado", "Пауза", "已暂停" },
        ["pausa.riprendi"] = new[] { "Riprendi", "Resume", "Reanudar", "Reprendre", "Fortsetzen", "Retomar", "Продолжить", "继续游戏" },
        ["pausa.menu_principale"] = new[] { "Torna al menu principale", "Back to main menu", "Volver al menú principal", "Retour au menu principal", "Zurück zum Hauptmenü", "Voltar ao menu principal", "Выйти в главное меню", "返回主菜单" },
        ["pausa.esci_gioco"] = new[] { "Esci dal gioco", "Quit game", "Salir del juego", "Quitter le jeu", "Spiel beenden", "Sair do jogo", "Выйти из игры", "退出游戏" },
        ["pausa.conferma_menu"] = new[] { "Tornare al menu principale? I progressi non salvati andranno persi.", "Return to the main menu? Unsaved progress will be lost.", "¿Volver al menú principal? Se perderá el progreso no guardado.", "Retourner au menu principal ? La progression non sauvegardée sera perdue.", "Zum Hauptmenü zurückkehren? Nicht gespeicherter Fortschritt geht verloren.", "Voltar ao menu principal? O progresso não salvo será perdido.", "Вернуться в главное меню? Несохранённый прогресс будет потерян.", "返回主菜单？未保存的进度将会丢失。" },
        ["pausa.conferma_esci"] = new[] { "Uscire dal gioco? I progressi non salvati andranno persi.", "Quit the game? Unsaved progress will be lost.", "¿Salir del juego? Se perderá el progreso no guardado.", "Quitter le jeu ? La progression non sauvegardée sera perdue.", "Spiel beenden? Nicht gespeicherter Fortschritt geht verloren.", "Sair do jogo? O progresso não salvo será perdido.", "Выйти из игры? Несохранённый прогресс будет потерян.", "退出游戏？未保存的进度将会丢失。" },

        // ---------- multigiocatore (menu iniziale e co-op, vedi ReteCoop) ----------
        ["menu.multigiocatore"] = new[] { "Multigiocatore", "Multiplayer", "Multijugador", "Multijoueur", "Mehrspieler", "Multijogador", "Мультиплеер", "多人游戏" },
        ["rete.sottotitolo"] = new[] { "Gioca con gli amici, fino a 3", "Play with friends, up to 3", "Juega con amigos, hasta 3", "Jouez avec vos amis, jusqu'à 3", "Spiele mit Freunden, bis zu 3", "Jogue com amigos, até 3", "Играйте с друзьями, до 3 игроков", "与好友一起玩，最多3人" },
        ["rete.ospita"] = new[] { "Ospita una partita", "Host a game", "Crear una partida", "Héberger une partie", "Spiel hosten", "Hospedar uma partida", "Создать игру", "创建游戏" },
        ["rete.entra"] = new[] { "Entra in una partita", "Join a game", "Unirse a una partida", "Rejoindre une partie", "Spiel beitreten", "Entrar em uma partida", "Присоединиться к игре", "加入游戏" },
        ["rete.tuo_indirizzo"] = new[] { "Il tuo indirizzo", "Your address", "Tu dirección", "Votre adresse", "Deine Adresse", "Seu endereço", "Ваш адрес", "你的地址" },
        ["rete.spiega"] = new[] { "Chi ospita dà il suo indirizzo agli amici. Stessa casa: l'indirizzo qui sopra. Su internet: una rete privata gratuita come ZeroTier o Radmin VPN. Stesso PC: 127.0.0.1.", "The host gives their address to friends. Same home: the address above. Over the internet: a free private network such as ZeroTier or Radmin VPN. Same PC: 127.0.0.1.", "Quien crea la partida da su dirección a los amigos. Misma casa: la dirección de arriba. Por internet: una red privada gratuita como ZeroTier o Radmin VPN. Mismo PC: 127.0.0.1.", "L'hôte donne son adresse à ses amis. Même maison : l'adresse ci-dessus. Par internet : un réseau privé gratuit comme ZeroTier ou Radmin VPN. Même PC : 127.0.0.1.", "Der Host gibt seinen Freunden seine Adresse. Gleiches Zuhause: die Adresse oben. Über das Internet: ein kostenloses privates Netzwerk wie ZeroTier oder Radmin VPN. Gleicher PC: 127.0.0.1.", "Quem hospeda passa o endereço para os amigos. Mesma casa: o endereço acima. Pela internet: uma rede privada gratuita como ZeroTier ou Radmin VPN. Mesmo PC: 127.0.0.1.", "Хост сообщает друзьям свой адрес. В одном доме: адрес выше. Через интернет: бесплатная частная сеть, например ZeroTier или Radmin VPN. Тот же ПК: 127.0.0.1.", "主机把自己的地址告诉好友。同一个家里：使用上面的地址。通过互联网：使用免费的私人网络，例如 ZeroTier 或 Radmin VPN。同一台电脑：127.0.0.1。" },
        ["rete.scrivi_indirizzo"] = new[] { "Scrivi l'indirizzo di chi ospita", "Type the host's address", "Escribe la dirección del anfitrión", "Saisissez l'adresse de l'hôte", "Gib die Adresse des Hosts ein", "Digite o endereço do host", "Введите адрес хоста", "输入主机地址" },
        ["rete.indirizzo"] = new[] { "Indirizzo", "Address", "Dirección", "Adresse", "Adresse", "Endereço", "Адрес", "地址" },
        ["rete.continua"] = new[] { "Continua", "Continue", "Continuar", "Continuer", "Weiter", "Continuar", "Далее", "继续" },
        ["rete.spiega_indirizzo"] = new[] { "Numeri e punti, per esempio 192.168.1.20. Backspace cancella.", "Numbers and dots, for example 192.168.1.20. Backspace deletes.", "Números y puntos, por ejemplo 192.168.1.20. Retroceso borra.", "Chiffres et points, par exemple 192.168.1.20. Retour arrière efface.", "Zahlen und Punkte, zum Beispiel 192.168.1.20. Rücktaste löscht.", "Números e pontos, por exemplo 192.168.1.20. Backspace apaga.", "Цифры и точки, например 192.168.1.20. Backspace стирает.", "数字和点，例如 192.168.1.20。退格键删除。" },
        ["rete.collegamento"] = new[] { "Collegamento a", "Connecting to", "Conectando con", "Connexion à", "Verbinde mit", "Conectando a", "Подключение к", "正在连接" },
        ["rete.annulla"] = new[] { "Annulla", "Cancel", "Cancelar", "Annuler", "Abbrechen", "Cancelar", "Отмена", "取消" },
        ["rete.giocatore"] = new[] { "Giocatore", "Player", "Jugador", "Joueur", "Spieler", "Jogador", "Игрок", "玩家" },
        ["rete.piena"] = new[] { "La partita è piena (massimo 3 giocatori).", "The game is full (3 players max).", "La partida está llena (máximo 3 jugadores).", "La partie est pleine (3 joueurs maximum).", "Das Spiel ist voll (maximal 3 Spieler).", "A partida está cheia (máximo 3 jogadores).", "Игра заполнена (максимум 3 игрока).", "游戏已满（最多3名玩家）。" },
        ["rete.host_uscito"] = new[] { "La partita è finita: chi ospitava è uscito o la connessione è caduta.", "The game has ended: the host left or the connection was lost.", "La partida ha terminado: el anfitrión salió o se perdió la conexión.", "La partie est terminée : l'hôte est parti ou la connexion a été perdue.", "Das Spiel ist beendet: Der Host ist gegangen oder die Verbindung ist abgebrochen.", "A partida terminou: o host saiu ou a conexão caiu.", "Игра окончена: хост вышел или соединение потеряно.", "游戏已结束：主机已离开或连接已断开。" },
        ["rete.errore_collega"] = new[] { "Impossibile collegarsi. Controlla l'indirizzo e che l'host abbia avviato la partita.", "Could not connect. Check the address and that the host has started the game.", "No se pudo conectar. Comprueba la dirección y que el anfitrión haya iniciado la partida.", "Connexion impossible. Vérifiez l'adresse et que l'hôte a lancé la partie.", "Verbindung fehlgeschlagen. Prüfe die Adresse und ob der Host das Spiel gestartet hat.", "Não foi possível conectar. Verifique o endereço e se o host iniciou a partida.", "Не удалось подключиться. Проверьте адрес и что хост начал игру.", "无法连接。请检查地址，并确认主机已开始游戏。" },
        ["rete.errore_ospita"] = new[] { "Impossibile ospitare la partita (forse la porta 7777 è già in uso).", "Could not host the game (port 7777 may already be in use).", "No se pudo crear la partida (quizá el puerto 7777 ya está en uso).", "Impossible d'héberger la partie (le port 7777 est peut-être déjà utilisé).", "Spiel konnte nicht gehostet werden (Port 7777 wird vielleicht schon verwendet).", "Não foi possível hospedar a partida (talvez a porta 7777 já esteja em uso).", "Не удалось создать игру (возможно, порт 7777 уже занят).", "无法创建游戏（端口 7777 可能已被占用）。" },
        ["rete.errore_occupato"] = new[] { "La rete è ancora occupata: riprova tra un attimo.", "The network is still busy: try again in a moment.", "La red sigue ocupada: inténtalo de nuevo en un momento.", "Le réseau est encore occupé : réessayez dans un instant.", "Das Netzwerk ist noch belegt: Versuche es gleich noch einmal.", "A rede ainda está ocupada: tente de novo em instantes.", "Сеть ещё занята: попробуйте через мгновение.", "网络仍在使用中：请稍后再试。" },
        ["rete.errore_prefab"] = new[] { "Mancano i file di rete del gioco (prefab): riapri Unity.", "The game's network files (prefabs) are missing: reopen Unity.", "Faltan los archivos de red del juego (prefabs): vuelve a abrir Unity.", "Les fichiers réseau du jeu (prefabs) sont manquants : rouvrez Unity.", "Die Netzwerkdateien des Spiels (Prefabs) fehlen: Öffne Unity neu.", "Faltam os arquivos de rede do jogo (prefabs): reabra o Unity.", "Отсутствуют сетевые файлы игры (префабы): перезапустите Unity.", "缺少游戏的网络文件（预制体）：请重新打开 Unity。" },
        ["rete.avviso_host_esce"] = new[] { "Stai ospitando: uscendo, la partita finisce anche per gli altri.", "You are hosting: if you leave, the game ends for everyone.", "Eres el anfitrión: si sales, la partida termina para todos.", "Vous êtes l'hôte : si vous partez, la partie se termine pour tout le monde.", "Du bist der Host: Wenn du gehst, endet das Spiel für alle.", "Você é o host: se sair, a partida termina para todos.", "Вы хост: если вы выйдете, игра закончится для всех.", "你是主机：如果你离开，所有人的游戏都会结束。" },
        // ---------- partita: barre, inventario, oggetti ----------
        ["inv.distanza"] = new[] { "Arma a distanza", "Ranged weapon", "Arma a distancia", "Arme à distance", "Fernkampfwaffe", "Arma à distância", "Дальнобойное оружие", "远程武器" },
        ["inv.no_scudo_ladro"] = new[] { "Il Ladro non usa scudi.", "The Thief does not use shields.", "El Ladrón no usa escudos.", "Le Voleur n'utilise pas de bouclier.", "Der Dieb benutzt keine Schilde.", "O Ladrão não usa escudos.", "Вор не пользуется щитами.", "盗贼不使用盾牌。" },
        ["inv.no_distanza"] = new[] { "Archi e balestre sono solo del Ladro.", "Bows and crossbows are for the Thief only.", "Los arcos y las ballestas son solo del Ladrón.", "Les arcs et les arbalètes sont réservés au Voleur.", "Bögen und Armbrüste sind nur für den Dieb.", "Arcos e bestas são só do Ladrão.", "Луки и арбалеты только для Вора.", "弓和弩只有盗贼能用。" },
        ["stat.portata"] = new[] { "Portata", "Range", "Alcance", "Portée", "Reichweite", "Alcance", "Дальность", "射程" },
        ["stat.ricarica"] = new[] { "Ricarica", "Reload", "Recarga", "Rechargement", "Nachladen", "Recarga", "Перезарядка", "装填" },
        ["stat.furtivita"] = new[] { "Furtività", "Stealth", "Sigilo", "Discrétion", "Heimlichkeit", "Furtividade", "Скрытность", "隐匿" },
        ["stat.cuoio"] = new[] { "Cuoio", "Leather", "Cuero", "Cuir", "Leder", "Couro", "Кожа", "皮革" },
        ["stat.ombra"] = new[] { "Ombra", "Shadow", "Sombra", "Ombre", "Schatten", "Sombra", "Тень", "暗影" },
        ["tipo.pugnale"] = new[] { "Pugnale", "Dagger", "Daga", "Dague", "Dolch", "Adaga", "Кинжал", "匕首" },
        ["tipo.stiletto"] = new[] { "Stiletto", "Stiletto", "Estilete", "Stylet", "Stilett", "Estilete", "Стилет", "短剑" },
        ["tipo.doppi_pugnali"] = new[] { "Pugnali doppi", "Twin daggers", "Dagas dobles", "Dagues doubles", "Doppeldolche", "Adagas duplas", "Парные кинжалы", "双匕首" },
        ["tipo.arco_corto"] = new[] { "Arco corto", "Short bow", "Arco corto", "Arc court", "Kurzbogen", "Arco curto", "Короткий лук", "短弓" },
        ["tipo.arco_lungo"] = new[] { "Arco lungo", "Longbow", "Arco largo", "Arc long", "Langbogen", "Arco longo", "Длинный лук", "长弓" },
        ["tipo.balestra"] = new[] { "Balestra", "Crossbow", "Ballesta", "Arbalète", "Armbrust", "Besta", "Арбалет", "弩" },
        ["tipo.armatura_cuoio"] = new[] { "Armatura di cuoio", "Leather armour", "Armadura de cuero", "Armure de cuir", "Lederrüstung", "Armadura de couro", "Кожаная броня", "皮甲" },
        ["tipo.armatura_ombra"] = new[] { "Veste d'ombra", "Shadow garb", "Vestimenta de sombra", "Tenue d'ombre", "Schattengewand", "Veste das sombras", "Теневое одеяние", "暗影之衣" },
        ["hud.tasto_attacco"] = new[] { "SX", "LMB", "IZQ", "G", "L", "ESQ", "ЛКМ", "左键" },
        ["hud.esecuzione"] = new[] { "Esecuzione furtiva", "Stealth execution", "Ejecución sigilosa", "Exécution furtive", "Heimliche Hinrichtung", "Execução furtiva", "Скрытная казнь", "潜行处决" },
        ["hud.mana_insufficiente"] = new[] { "Mana insufficiente", "Not enough mana", "Maná insuficiente", "Mana insuffisant", "Nicht genug Mana", "Mana insuficiente", "Недостаточно маны", "法力不足" },
        ["hud.morto"] = new[] { "Sei morto", "You died", "Has muerto", "Tu es mort", "Du bist gestorben", "Você morreu", "Ты погиб", "你死了" },
        ["hud.leva"] = new[] { "Tira la leva", "Pull the lever", "Tira de la palanca", "Tirer le levier", "Hebel ziehen", "Puxar a alavanca", "Потянуть рычаг", "拉动拉杆" },
        ["hud.baule"] = new[] { "Apri il baule", "Open the chest", "Abrir el cofre", "Ouvrir le coffre", "Truhe öffnen", "Abrir o baú", "Открыть сундук", "打开宝箱" },
        ["hud.checkpoint"] = new[] { "Accendi il checkpoint", "Light the checkpoint", "Encender el punto de control", "Allumer le point de contrôle", "Kontrollpunkt entzünden", "Acender o ponto de controle", "Зажечь контрольную точку", "点燃检查点" },
        ["hud.apri_chiave"] = new[] { "Apri con la chiave", "Open with the key", "Abrir con la llave", "Ouvrir avec la clé", "Mit dem Schlüssel öffnen", "Abrir com a chave", "Открыть ключом", "用钥匙打开" },
        ["hud.chiusa_chiave"] = new[] { "Chiusa a chiave", "Locked", "Cerrada con llave", "Fermée à clé", "Verschlossen", "Trancada", "Заперто", "已上锁" },
        ["hud.serve"] = new[] { "Serve", "You need", "Necesitas", "Il faut", "Benötigt", "Você precisa de", "Нужно", "需要" },
        ["hud.trovato_bastone"] = new[] { "Hai trovato il bastone magico: 2 per impugnarlo, 1 per tornare alla spada.", "You found the magic staff: 2 to wield it, 1 to switch back to the sword.", "Has encontrado el bastón mágico: 2 para empuñarlo, 1 para volver a la espada.", "Tu as trouvé le bâton magique : 2 pour le prendre en main, 1 pour revenir à l'épée.", "Du hast den Zauberstab gefunden: 2 zum Ausrüsten, 1 zurück zum Schwert.", "Você encontrou o cajado mágico: 2 para empunhá-lo, 1 para voltar à espada.", "Ты нашёл магический посох: 2 — взять посох, 1 — вернуться к мечу.", "你找到了魔法法杖：按 2 装备，按 1 换回剑。" },
        ["hud.raccolto_compagno"] = new[] { "Un compagno ha raccolto", "A companion picked up", "Un compañero ha recogido", "Un compagnon a ramassé", "Ein Gefährte hat aufgehoben", "Um companheiro pegou", "Соратник подобрал", "同伴拾取了" },
        ["inv.tutto"] = new[] { "Tutto", "All", "Todo", "Tout", "Alles", "Tudo", "Всё", "全部" },
        ["inv.armi"] = new[] { "Armi", "Weapons", "Armas", "Armes", "Waffen", "Armas", "Оружие", "武器" },
        ["inv.scudi"] = new[] { "Scudi", "Shields", "Escudos", "Boucliers", "Schilde", "Escudos", "Щиты", "盾牌" },
        ["inv.armature"] = new[] { "Armature", "Armour", "Armaduras", "Armures", "Rüstungen", "Armaduras", "Броня", "护甲" },
        ["inv.amuleti"] = new[] { "Amuleti", "Amulets", "Amuletos", "Amulettes", "Amulette", "Amuletos", "Амулеты", "护符" },
        ["inv.titolo"] = new[] { "Equipaggiamento", "Equipment", "Equipo", "Équipement", "Ausrüstung", "Equipamento", "Снаряжение", "装备" },
        ["inv.zaino"] = new[] { "Zaino", "Backpack", "Mochila", "Sac", "Rucksack", "Mochila", "Рюкзак", "背包" },
        ["inv.arma"] = new[] { "Arma", "Weapon", "Arma", "Arme", "Waffe", "Arma", "Оружие", "武器" },
        ["inv.scudo"] = new[] { "Scudo", "Shield", "Escudo", "Bouclier", "Schild", "Escudo", "Щит", "盾牌" },
        ["inv.armatura"] = new[] { "Armatura", "Armor", "Armadura", "Armure", "Rüstung", "Armadura", "Доспех", "护甲" },
        ["inv.amuleto"] = new[] { "Amuleto", "Amulet", "Amuleto", "Amulette", "Amulett", "Amuleto", "Амулет", "护符" },
        ["inv.vuoto"] = new[] { "Vuoto", "Empty", "Vacío", "Vide", "Leer", "Vazio", "Пусто", "空" },
        ["inv.zaino_vuoto"] = new[] { "Lo zaino è vuoto", "Your backpack is empty", "La mochila está vacía", "Le sac est vide", "Der Rucksack ist leer", "A mochila está vazia", "Рюкзак пуст", "背包是空的" },
        ["inv.aiuto"] = new[] { "E  equipaggia / togli      Q R  schede      Tab  chiudi      Frecce  scegli", "E  equip / remove      Q R  tabs      Tab  close      Arrows  select", "E  equipar / quitar      Q R  pestañas      Tab  cerrar      Flechas  elegir", "E  équiper / retirer      Q R  onglets      Tab  fermer      Flèches  choisir", "E  ausrüsten / ablegen      Q R  Reiter      Tab  schließen      Pfeiltasten  wählen", "E  equipar / remover      Q R  abas      Tab  fechar      Setas  escolher", "E  надеть / снять      Q R  вкладки      Tab  закрыть      Стрелки  выбрать", "E  装备 / 卸下      Q R  分类      Tab  关闭      方向键  选择" },
        ["stat.vita"] = new[] { "Vita", "Health", "Vida", "Vie", "Leben", "Vida", "Здоровье", "生命" },
        ["stat.resistenza"] = new[] { "Resistenza", "Stamina", "Aguante", "Endurance", "Ausdauer", "Vigor", "Выносливость", "耐力" },
        ["stat.mana"] = new[] { "Mana", "Mana", "Maná", "Mana", "Mana", "Mana", "Мана", "法力" },
        ["stat.armatura"] = new[] { "Armatura", "Armor", "Armadura", "Armure", "Rüstung", "Armadura", "Броня", "护甲值" },
        ["stat.danno"] = new[] { "Danno", "Damage", "Daño", "Dégâts", "Schaden", "Dano", "Урон", "伤害" },
        ["stat.critico"] = new[] { "Critico", "Critical", "Crítico", "Critique", "Kritisch", "Crítico", "Крит", "暴击" },
        ["stat.costo"] = new[] { "Costo resistenza", "Stamina cost", "Coste de aguante", "Coût d'endurance", "Ausdauerkosten", "Custo de vigor", "Расход выносливости", "耐力消耗" },
        ["stat.velocita"] = new[] { "Velocità", "Speed", "Velocidad", "Vitesse", "Tempo", "Velocidade", "Скорость", "速度" },
        ["stat.rapida"] = new[] { "Rapida", "Fast", "Rápida", "Rapide", "Schnell", "Rápida", "Быстрая", "快" },
        ["stat.media"] = new[] { "Media", "Medium", "Media", "Moyenne", "Mittel", "Média", "Средняя", "中" },
        ["stat.lenta"] = new[] { "Lenta", "Slow", "Lenta", "Lente", "Langsam", "Lenta", "Медленная", "慢" },
        ["stat.parata"] = new[] { "Danno parato", "Damage blocked", "Daño bloqueado", "Dégâts bloqués", "Geblockter Schaden", "Dano bloqueado", "Блок урона", "格挡减伤" },
        ["stat.costo_parata"] = new[] { "Costo parata", "Block cost", "Coste de bloqueo", "Coût de blocage", "Blockkosten", "Custo de bloqueio", "Расход на блок", "格挡消耗" },
        ["stat.schivata"] = new[] { "Costo schivata", "Dodge cost", "Coste de esquiva", "Coût d'esquive", "Ausweichkosten", "Custo de esquiva", "Расход на уклонение", "闪避消耗" },
        ["stat.parata_perfetta"] = new[] { "Parata perfetta", "Perfect parry", "Bloqueo perfecto", "Parade parfaite", "Perfekte Parade", "Bloqueio perfeito", "Идеальный блок", "完美格挡" },
        ["stat.peso"] = new[] { "Peso", "Weight", "Peso", "Poids", "Gewicht", "Peso", "Вес", "重量" },
        ["stat.leggera"] = new[] { "Leggera", "Light", "Ligera", "Légère", "Leicht", "Leve", "Лёгкая", "轻" },
        ["stat.pesante"] = new[] { "Pesante", "Heavy", "Pesada", "Lourde", "Schwer", "Pesada", "Тяжёлая", "重" },
        ["stat.movimento"] = new[] { "Movimento", "Movement", "Movimiento", "Déplacement", "Bewegung", "Movimento", "Движение", "移动速度" },
        ["stat.tipo"] = new[] { "Tipo", "Type", "Tipo", "Type", "Art", "Tipo", "Тип", "类型" },
        ["stat.magico"] = new[] { "Magico", "Magic", "Mágico", "Magique", "Magisch", "Mágico", "Магический", "魔法" },
        ["stat.arcano"] = new[] { "Arcano", "Arcane", "Arcano", "Arcanique", "Arkan", "Arcano", "Тайный", "奥术" },
        ["tipo.spada"] = new[] { "Spada", "Sword", "Espada", "Épée", "Schwert", "Espada", "Меч", "剑" },
        ["tipo.spadone"] = new[] { "Spadone a due mani", "Greatsword", "Mandoble", "Espadon", "Zweihänder", "Montante", "Двуручный меч", "双手大剑" },
        ["tipo.ascia"] = new[] { "Ascia", "Axe", "Hacha", "Hache", "Axt", "Machado", "Топор", "斧" },
        ["tipo.mazza"] = new[] { "Mazza", "Mace", "Maza", "Masse", "Streitkolben", "Maça", "Булава", "锤" },
        ["tipo.scudo_grande"] = new[] { "Scudo grande", "Large shield", "Escudo grande", "Grand bouclier", "Großer Schild", "Escudo grande", "Большой щит", "大盾" },
        ["tipo.scudo_medio"] = new[] { "Scudo medio", "Medium shield", "Escudo mediano", "Bouclier moyen", "Mittlerer Schild", "Escudo médio", "Средний щит", "中盾" },
        ["tipo.scudo_piccolo"] = new[] { "Scudo piccolo", "Small shield", "Escudo pequeño", "Petit bouclier", "Kleiner Schild", "Escudo pequeno", "Малый щит", "小盾" },
        ["tipo.armatura_leggera"] = new[] { "Armatura leggera", "Light armor", "Armadura ligera", "Armure légère", "Leichte Rüstung", "Armadura leve", "Лёгкий доспех", "轻甲" },
        ["tipo.armatura_media"] = new[] { "Armatura media", "Medium armor", "Armadura media", "Armure intermédiaire", "Mittlere Rüstung", "Armadura média", "Средний доспех", "中甲" },
        ["tipo.armatura_pesante"] = new[] { "Armatura pesante", "Heavy armor", "Armadura pesada", "Armure lourde", "Schwere Rüstung", "Armadura pesada", "Тяжёлый доспех", "重甲" },
        ["tipo.amuleto_magico"] = new[] { "Amuleto magico", "Magic amulet", "Amuleto mágico", "Amulette magique", "Magisches Amulett", "Amuleto mágico", "Магический амулет", "魔法护符" },
        ["tipo.amuleto_arcano"] = new[] { "Amuleto arcano", "Arcane amulet", "Amuleto arcano", "Amulette arcanique", "Arkanes Amulett", "Amuleto arcano", "Тайный амулет", "奥术护符" },
        ["hud.raccogli"] = new[] { "Raccogli", "Pick up", "Recoger", "Ramasser", "Aufheben", "Pegar", "Подобрать", "拾取" },
        ["hud.raccolto"] = new[] { "Raccolto", "Picked up", "Recogido", "Ramassé", "Aufgehoben", "Pegou", "Подобрано", "已拾取" },
        ["hud.alba"] = new[] { "Alba", "Dawn", "Alba", "Aube", "Morgengrauen", "Amanhecer", "Рассвет", "黎明" },
        ["hud.giorno"] = new[] { "Giorno", "Day", "Día", "Jour", "Tag", "Dia", "День", "白昼" },
        ["hud.tramonto"] = new[] { "Tramonto", "Dusk", "Atardecer", "Crépuscule", "Abenddämmerung", "Entardecer", "Закат", "黄昏" },
        ["hud.notte"] = new[] { "Notte", "Night", "Noche", "Nuit", "Nacht", "Noite", "Ночь", "夜晚" },
        ["menu.nessuna_scena"] = new[] { "Nessuna scena di gioco nelle Build Settings", "No game scene in the Build Settings", null, null, null, null, null, null },

        ["classe.scegli"] = new[] { "Scegli chi eri, prima di quella notte", "Choose who you were, before that night", "Elige quién eras antes de aquella noche", "Choisis qui tu étais, avant cette nuit-là", "Wähle, wer du warst – vor jener Nacht", "Escolha quem você era antes daquela noite", "Выбери, кем ты был до той ночи", "选择那一夜之前的你" },
        ["classe.guerriero"] = new[] { "Guerriero", "Warrior", "Guerrero", "Guerrier", "Krieger", "Guerreiro", "Воин", "战士" },
        ["classe.ladro"] = new[] { "Ladro", "Thief", "Ladrón", "Voleur", "Dieb", "Ladrão", "Вор", "盗贼" },
        ["classe.stregone"] = new[] { "Stregone", "Sorcerer", "Hechicero", "Sorcier", "Zauberer", "Feiticeiro", "Колдун", "术士" },
        ["classe.guerriero.descrizione"] = new[]
        {
            "Più vita, più forza e colpi potenti. Spada e scudo.",
            "More health, more strength and powerful blows. Sword and shield.",
            "Más vida, más fuerza y golpes poderosos. Espada y escudo.",
            "Plus de vie, plus de force et des coups puissants. Épée et bouclier.",
            "Mehr Leben, mehr Stärke und wuchtige Schläge. Schwert und Schild.",
            "Mais vida, mais força e golpes poderosos. Espada e escudo.",
            "Больше здоровья, больше силы и мощные удары. Меч и щит.",
            "更多生命、更强力量和强力攻击。剑与盾。",
        },
        ["classe.ladro.descrizione"] = new[]
        {
            "Veloce e preciso. Arco e pugnale.",
            "Fast and precise. Bow and dagger.",
            "Rápido y preciso. Arco y daga.",
            "Rapide et précis. Arc et dague.",
            "Schnell und präzise. Bogen und Dolch.",
            "Rápido e preciso. Arco e adaga.",
            "Быстрый и точный. Лук и кинжал.",
            "迅捷而精准。弓与匕首。",
        },
        ["classe.stregone.descrizione"] = new[]
        {
            "Attacchi a distanza ed evocazioni. Bastone incantato e libro.",
            "Ranged attacks and summoning. Enchanted staff and tome.",
            "Ataques a distancia e invocaciones. Bastón encantado y libro.",
            "Attaques à distance et invocations. Bâton enchanté et grimoire.",
            "Fernangriffe und Beschwörungen. Verzauberter Stab und Buch.",
            "Ataques à distância e invocações. Cajado encantado e livro.",
            "Дальние атаки и призыв. Зачарованный посох и книга.",
            "远程攻击与召唤。附魔法杖与法典。",
        },

        ["opzioni.lingua"] = new[] { "Lingua", "Language", "Idioma", "Langue", "Sprache", "Idioma", "Язык", "语言" },
        ["opzioni.volume_generale"] = new[] { "Volume generale", "Master volume", "Volumen general", "Volume général", "Gesamtlautstärke", "Volume geral", "Общая громкость", "总音量" },
        ["opzioni.volume_musica"] = new[] { "Volume musica", "Music volume", "Volumen de la música", "Volume de la musique", "Musiklautstärke", "Volume da música", "Громкость музыки", "音乐音量" },
        ["opzioni.schermo_intero"] = new[] { "Schermo intero", "Fullscreen", "Pantalla completa", "Plein écran", "Vollbild", "Tela cheia", "Полноэкранный режим", "全屏" },
        ["opzioni.effetto_retro"] = new[] { "Effetto retro PS2", "PS2 retro effect", "Efecto retro PS2", "Effet rétro PS2", "PS2-Retro-Effekt", "Efeito retrô PS2", "Ретро-эффект PS2", "PS2 复古效果" },
        ["comune.si"] = new[] { "Sì", "Yes", "Sí", "Oui", "Ja", "Sim", "Да", "是" },
        ["comune.no"] = new[] { "No", "No", "No", "Non", "Nein", "Não", "Нет", "否" },

        // Oggetti del Guerriero (chiavi usate da DatiOggetto: oggetto.<chiave>.nome e oggetto.<chiave>.descrizione).
        // ---------- oggetti del Ladro (Docs/oggetti-ladro.md) ----------
        ["oggetto.pugnale_scuoiare.nome"] = new[] { "Pugnale da scuoiare", "Skinning Knife", "Cuchillo de desollar", "Couteau à dépecer", "Häutemesser", "Faca de esfolar", "Нож для свежевания", "剥皮匕首" },
        ["oggetto.pugnale_scuoiare.descrizione"] = new[]
        {
            "Serviva per le pelli. Ora serve per gli orchi.",
            "It was meant for hides. Now it is meant for orcs.",
            "Servía para las pieles. Ahora sirve para los orcos.",
            "Il servait pour les peaux. Désormais, il sert pour les orques.",
            "Es war für Felle gedacht. Jetzt ist es für Orks gedacht.",
            "Servia para as peles. Agora serve para os orcs.",
            "Им снимали шкуры. Теперь им убивают орков.",
            "它曾用来剥兽皮，如今用来对付兽人。",
        },
        ["oggetto.pugnale_ricurvo.nome"] = new[] { "Pugnale ricurvo degli orchi", "Orcish Curved Dagger", "Daga curva de los orcos", "Dague courbe des orques", "Krummdolch der Orks", "Adaga curva dos orcs", "Изогнутый кинжал орков", "兽人弯匕" },
        ["oggetto.pugnale_ricurvo.descrizione"] = new[]
        {
            "Lama storta e pesante, rubata a chi l'ha usata contro il villaggio.",
            "A heavy, crooked blade, taken from those who used it against the village.",
            "Hoja torcida y pesada, robada a quienes la usaron contra la aldea.",
            "Une lame tordue et lourde, prise à ceux qui l'ont utilisée contre le village.",
            "Eine schwere, krumme Klinge, denen abgenommen, die sie gegen das Dorf führten.",
            "Lâmina torta e pesada, tomada de quem a usou contra a aldeia.",
            "Тяжёлый кривой клинок, отнятый у тех, кто поднял его против деревни.",
            "沉重扭曲的刀刃，夺自那些用它袭击村庄的人。",
        },
        ["oggetto.stiletto_tagliagole.nome"] = new[] { "Stiletto del tagliagole", "Cutthroat's Stiletto", "Estilete del degollador", "Stylet de l'égorgeur", "Stilett des Halsabschneiders", "Estilete do degolador", "Стилет головореза", "割喉者短剑" },
        ["oggetto.stiletto_tagliagole.descrizione"] = new[]
        {
            "Sottile come un ago, passa tra le piastre di qualsiasi armatura.",
            "Thin as a needle, it slips between the plates of any armour.",
            "Fino como una aguja, pasa entre las placas de cualquier armadura.",
            "Fin comme une aiguille, il passe entre les plaques de n'importe quelle armure.",
            "Dünn wie eine Nadel, gleitet es zwischen die Platten jeder Rüstung.",
            "Fino como uma agulha, passa entre as placas de qualquer armadura.",
            "Тонкий, как игла, он проходит между пластинами любой брони.",
            "细如针尖，能刺入任何盔甲的缝隙。",
        },
        ["oggetto.stiletto_ombra.nome"] = new[] { "Stiletto d'ombra", "Shadow Stiletto", "Estilete de sombra", "Stylet d'ombre", "Schattenstilett", "Estilete das sombras", "Теневой стилет", "暗影短剑" },
        ["oggetto.stiletto_ombra.descrizione"] = new[]
        {
            "Una lama annerita che non riflette la luce delle torce.",
            "A blackened blade that does not catch the torchlight.",
            "Una hoja ennegrecida que no refleja la luz de las antorchas.",
            "Une lame noircie qui ne reflète pas la lumière des torches.",
            "Eine geschwärzte Klinge, die das Fackellicht nicht spiegelt.",
            "Uma lâmina enegrecida que não reflete a luz das tochas.",
            "Почернённый клинок, не отражающий свет факелов.",
            "乌黑的刀刃，不反射一丝火光。",
        },
        ["oggetto.pugnali_gemelli.nome"] = new[] { "Pugnali gemelli", "Twin Daggers", "Dagas gemelas", "Dagues jumelles", "Zwillingsdolche", "Adagas gêmeas", "Парные кинжалы", "双子匕首" },
        ["oggetto.pugnali_gemelli.descrizione"] = new[]
        {
            "Due lame leggere, una per mano: colpi rapidissimi, nessuna parata.",
            "Two light blades, one in each hand: lightning-fast strikes, no blocking.",
            "Dos hojas ligeras, una en cada mano: golpes rapidísimos, sin parada.",
            "Deux lames légères, une dans chaque main : des coups très rapides, aucune parade.",
            "Zwei leichte Klingen, eine in jeder Hand: blitzschnelle Hiebe, keine Parade.",
            "Duas lâminas leves, uma em cada mão: golpes rapidíssimos, sem bloqueio.",
            "Два лёгких клинка, по одному в каждой руке: молниеносные удары, никакого блока.",
            "两把轻刃，一手一把：出手极快，无法格挡。",
        },
        ["oggetto.arco_caccia.nome"] = new[] { "Arco corto da caccia", "Short Hunting Bow", "Arco corto de caza", "Arc court de chasse", "Kurzer Jagdbogen", "Arco curto de caça", "Короткий охотничий лук", "短猎弓" },
        ["oggetto.arco_caccia.descrizione"] = new[]
        {
            "L'arco con cui portavi a casa la cena. Leggero e veloce.",
            "The bow you used to bring dinner home. Light and quick.",
            "El arco con el que llevabas la cena a casa. Ligero y rápido.",
            "L'arc avec lequel tu rapportais le dîner. Léger et rapide.",
            "Der Bogen, mit dem du das Abendessen heimgebracht hast. Leicht und schnell.",
            "O arco com que você levava o jantar para casa. Leve e rápido.",
            "Лук, которым ты добывал ужин. Лёгкий и быстрый.",
            "你曾用它打猎带回晚餐。轻巧而迅捷。",
        },
        ["oggetto.arco_osso.nome"] = new[] { "Arco d'osso degli orchi", "Orcish Bone Bow", "Arco de hueso de los orcos", "Arc d'os des orques", "Knochenbogen der Orks", "Arco de osso dos orcs", "Костяной лук орков", "兽人骨弓" },
        ["oggetto.arco_osso.descrizione"] = new[]
        {
            "Corde di tendine e ossa legate: rozzo, ma tira più forte di quanto sembri.",
            "Sinew strings and bound bones: crude, but it shoots harder than it looks.",
            "Cuerdas de tendón y huesos atados: tosco, pero dispara más fuerte de lo que parece.",
            "Cordes de tendon et os liés : grossier, mais il tire plus fort qu'il n'en a l'air.",
            "Sehnen und zusammengebundene Knochen: grob, aber er schießt stärker, als er aussieht.",
            "Cordas de tendão e ossos amarrados: rústico, mas atira mais forte do que parece.",
            "Жилы и связанные кости: грубый, но бьёт сильнее, чем кажется.",
            "筋弦缚骨，做工粗糙，威力却超乎想象。",
        },
        ["oggetto.arco_tasso.nome"] = new[] { "Arco lungo di tasso", "Yew Longbow", "Arco largo de tejo", "Arc long en if", "Eibenlangbogen", "Arco longo de teixo", "Тисовый длинный лук", "紫杉长弓" },
        ["oggetto.arco_tasso.descrizione"] = new[]
        {
            "Lento da tendere, ma la sua freccia arriva lontano e trapassa.",
            "Slow to draw, but its arrow flies far and pierces deep.",
            "Lento de tensar, pero su flecha llega lejos y atraviesa.",
            "Lent à bander, mais sa flèche va loin et transperce.",
            "Langsam zu spannen, doch sein Pfeil fliegt weit und durchbohrt.",
            "Lento de puxar, mas sua flecha vai longe e atravessa.",
            "Его долго натягивать, но стрела летит далеко и пробивает насквозь.",
            "拉弓缓慢，但箭矢射得远、穿透力强。",
        },
        ["oggetto.balestra_posta.nome"] = new[] { "Balestra da posta", "Ambush Crossbow", "Ballesta de acecho", "Arbalète d'affût", "Ansitzarmbrust", "Besta de tocaia", "Засадный арбалет", "伏击弩" },
        ["oggetto.balestra_posta.descrizione"] = new[]
        {
            "Pensata per aspettare nascosti: un colpo solo, ma che conta.",
            "Made for waiting in hiding: a single shot, but one that counts.",
            "Pensada para esperar escondido: un solo disparo, pero que cuenta.",
            "Faite pour attendre caché : un seul tir, mais qui compte.",
            "Gemacht, um versteckt zu warten: ein einziger Schuss, aber er zählt.",
            "Feita para esperar escondido: um único disparo, mas que conta.",
            "Создан для засады: всего один выстрел, но решающий.",
            "专为埋伏而造：只有一发，却足以致命。",
        },
        ["oggetto.balestra_carceriere.nome"] = new[] { "Balestra del carceriere", "Jailer's Crossbow", "Ballesta del carcelero", "Arbalète du geôlier", "Armbrust des Kerkermeisters", "Besta do carcereiro", "Арбалет тюремщика", "狱卒之弩" },
        ["oggetto.balestra_carceriere.descrizione"] = new[]
        {
            "Presa nella gattabuia degli orchi. Ricarica lentissima, colpo devastante.",
            "Taken from the orcs' dungeon. Very slow to reload, devastating to be hit by.",
            "Tomada en la mazmorra de los orcos. Recarga lentísima, disparo devastador.",
            "Prise dans le cachot des orques. Rechargement très lent, tir dévastateur.",
            "Aus dem Kerker der Orks. Sehr langsames Nachladen, verheerender Schuss.",
            "Tomada da masmorra dos orcs. Recarga lentíssima, disparo devastador.",
            "Взят в темнице орков. Перезаряжается очень долго, бьёт сокрушительно.",
            "取自兽人地牢。装填极慢，威力惊人。",
        },
        ["oggetto.giubba_scura.nome"] = new[] { "Giubba scura", "Dark Jerkin", "Jubón oscuro", "Pourpoint sombre", "Dunkles Wams", "Gibão escuro", "Тёмная куртка", "暗色短衣" },
        ["oggetto.giubba_scura.descrizione"] = new[]
        {
            "Stoffa scura e silenziosa: protegge poco, ma si confonde con la notte.",
            "Dark, quiet cloth: little protection, but it blends into the night.",
            "Tela oscura y silenciosa: protege poco, pero se confunde con la noche.",
            "Un tissu sombre et silencieux : il protège peu, mais se fond dans la nuit.",
            "Dunkler, leiser Stoff: schützt wenig, verschmilzt aber mit der Nacht.",
            "Tecido escuro e silencioso: protege pouco, mas se confunde com a noite.",
            "Тёмная бесшумная ткань: защищает слабо, зато сливается с ночью.",
            "深色静音的布料：防护不多，却能融入夜色。",
        },
        ["oggetto.corpetto_cuoio.nome"] = new[] { "Corpetto di cuoio rinforzato", "Reinforced Leather Bodice", "Corpiño de cuero reforzado", "Corselet de cuir renforcé", "Verstärktes Lederwams", "Corpete de couro reforçado", "Укреплённый кожаный жилет", "加固皮甲" },
        ["oggetto.corpetto_cuoio.descrizione"] = new[]
        {
            "Cuoio bollito e borchie: un compromesso fra difesa e silenzio.",
            "Boiled leather and studs: a compromise between defence and silence.",
            "Cuero hervido y tachuelas: un equilibrio entre defensa y silencio.",
            "Cuir bouilli et clous : un compromis entre défense et silence.",
            "Gekochtes Leder und Nieten: ein Kompromiss zwischen Schutz und Stille.",
            "Couro fervido e tachas: um meio-termo entre defesa e silêncio.",
            "Варёная кожа и заклёпки: компромисс между защитой и тишиной.",
            "熟皮与铆钉：在防护与隐匿之间取得平衡。",
        },
        ["oggetto.manto_ombra.nome"] = new[] { "Manto dell'ombra", "Mantle of Shadow", "Manto de sombra", "Manteau d'ombre", "Schattenmantel", "Manto das sombras", "Плащ тени", "暗影斗篷" },
        ["oggetto.manto_ombra.descrizione"] = new[]
        {
            "Quasi non protegge. Ma chi non ti vede, non ti colpisce.",
            "It barely protects. But those who cannot see you cannot strike you.",
            "Casi no protege. Pero quien no te ve, no te golpea.",
            "Il protège à peine. Mais qui ne te voit pas ne te frappe pas.",
            "Er schützt kaum. Doch wer dich nicht sieht, trifft dich nicht.",
            "Quase não protege. Mas quem não te vê, não te acerta.",
            "Почти не защищает. Но тот, кто тебя не видит, не может ударить.",
            "几乎毫无防护。但看不见你的人，也伤不到你。",
        },
        ["oggetto.piuma_civetta.nome"] = new[] { "Piuma di civetta", "Owl Feather", "Pluma de lechuza", "Plume de chouette", "Eulenfeder", "Pena de coruja", "Перо совы", "鸮羽" },
        ["oggetto.piuma_civetta.descrizione"] = new[]
        {
            "La civetta caccia senza un rumore. Chi la porta, anche.",
            "The owl hunts without a sound. So does whoever wears this.",
            "La lechuza caza sin hacer ruido. Quien la lleva, también.",
            "La chouette chasse sans un bruit. Celui qui la porte aussi.",
            "Die Eule jagt lautlos. Wer sie trägt, ebenso.",
            "A coruja caça sem fazer barulho. Quem a usa, também.",
            "Сова охотится бесшумно. Как и тот, кто носит это перо.",
            "鸮狩猎时悄无声息，佩戴者亦然。",
        },
        ["oggetto.dente_vipera.nome"] = new[] { "Dente di vipera", "Viper Fang", "Colmillo de víbora", "Croc de vipère", "Vipernzahn", "Presa de víbora", "Зуб гадюки", "蝰蛇之牙" },
        ["oggetto.dente_vipera.descrizione"] = new[]
        {
            "Morde nei punti giusti, ma rende la mano più lenta.",
            "It bites in the right places, but it slows the hand.",
            "Muerde en los puntos justos, pero vuelve la mano más lenta.",
            "Il mord aux bons endroits, mais ralentit la main.",
            "Er beißt an den richtigen Stellen, macht die Hand aber langsamer.",
            "Morde nos pontos certos, mas deixa a mão mais lenta.",
            "Жалит в нужные места, но замедляет руку.",
            "总能咬中要害，却让出手变慢。",
        },
        ["oggetto.laccio_borsaiolo.nome"] = new[] { "Laccio del borsaiolo", "Pickpocket's Cord", "Cordón del ratero", "Lacet du tire-laine", "Schnur des Taschendiebs", "Cordão do batedor de carteiras", "Шнурок карманника", "扒手绳结" },
        ["oggetto.laccio_borsaiolo.descrizione"] = new[]
        {
            "Mani svelte, colpi rapidi, ma meno attenzione ai punti deboli.",
            "Quick hands, fast strikes, but less care for weak spots.",
            "Manos ágiles, golpes rápidos, pero menos atención a los puntos débiles.",
            "Des mains agiles, des coups rapides, mais moins d'attention aux points faibles.",
            "Flinke Hände, schnelle Hiebe, aber weniger Blick für Schwachstellen.",
            "Mãos ágeis, golpes rápidos, mas menos atenção aos pontos fracos.",
            "Ловкие руки и быстрые удары, но меньше внимания к слабым местам.",
            "手法灵巧、出手迅速，却顾不上要害。",
        },
        ["oggetto.ultimo_respiro.nome"] = new[] { "Ultimo respiro", "Last Breath", "Último aliento", "Dernier souffle", "Letzter Atemzug", "Último suspiro", "Последний вздох", "最后一息" },
        ["oggetto.ultimo_respiro.descrizione"] = new[]
        {
            "Tasto Q: svanisci nell'ombra e i nemici non ti vedono per qualche istante.",
            "Q key: vanish into the shadows and enemies cannot see you for a few moments.",
            "Tecla Q: desapareces en las sombras y los enemigos no te ven durante unos instantes.",
            "Touche Q : disparais dans l'ombre et les ennemis ne te voient plus pendant quelques instants.",
            "Taste Q: Verschwinde in den Schatten, und Feinde sehen dich für kurze Zeit nicht.",
            "Tecla Q: desapareça nas sombras e os inimigos não te veem por alguns instantes.",
            "Клавиша Q: растворись в тени, и враги на мгновение тебя не видят.",
            "Q键：遁入阴影，敌人片刻之间无法看见你。",
        },
        ["oggetto.fiato_predatore.nome"] = new[] { "Fiato del predatore", "Predator's Breath", "Aliento del depredador", "Souffle du prédateur", "Atem des Räubers", "Fôlego do predador", "Дыхание хищника", "猎食者之息" },
        ["oggetto.fiato_predatore.descrizione"] = new[]
        {
            "Il fiato torna in fretta, ma il corpo regge meno colpi.",
            "Your breath returns quickly, but your body takes fewer blows.",
            "El aliento vuelve deprisa, pero el cuerpo aguanta menos golpes.",
            "Le souffle revient vite, mais le corps encaisse moins de coups.",
            "Der Atem kehrt schnell zurück, doch der Körper hält weniger Treffer aus.",
            "O fôlego volta depressa, mas o corpo aguenta menos golpes.",
            "Дыхание быстро восстанавливается, но тело выдерживает меньше ударов.",
            "气息恢复得很快，身体却更经不起打击。",
        },
        ["oggetto.goccia_sangue_nero.nome"] = new[] { "Goccia di sangue nero", "Drop of Black Blood", "Gota de sangre negra", "Goutte de sang noir", "Tropfen schwarzen Blutes", "Gota de sangue negro", "Капля чёрной крови", "黑血之滴" },
        ["oggetto.goccia_sangue_nero.descrizione"] = new[]
        {
            "Una parte del sangue che versi torna a te.",
            "Part of the blood you spill flows back to you.",
            "Una parte de la sangre que derramas vuelve a ti.",
            "Une part du sang que tu verses te revient.",
            "Ein Teil des Blutes, das du vergießt, fließt zu dir zurück.",
            "Parte do sangue que você derrama volta para você.",
            "Часть пролитой тобой крови возвращается к тебе.",
            "你所洒下的鲜血，有一部分会回到你身上。",
        },
        ["oggetto.spada_vecchia_vita.nome"] = new[] { "Spada della vecchia vita", "Sword of the Old Life", "Espada de la vieja vida", "Épée de l'ancienne vie", "Schwert des alten Lebens", "Espada da vida antiga", "Меч прежней жизни", "旧日之剑" },
        ["oggetto.spada_vecchia_vita.descrizione"] = new[]
        {
            "La spada che avevi appeso al muro. Pensavi di non doverla impugnare mai più.",
            "The sword you had hung on the wall. You thought you would never have to hold it again.",
            "La espada que habías colgado en la pared. Creías que nunca tendrías que volver a empuñarla.",
            "L'épée que tu avais accrochée au mur. Tu pensais ne plus jamais devoir la tenir.",
            "Das Schwert, das du an die Wand gehängt hattest. Du dachtest, du müsstest es nie wieder führen.",
            "A espada que você tinha pendurado na parede. Achava que nunca mais precisaria empunhá-la.",
            "Меч, который ты повесил на стену. Ты думал, что больше никогда не возьмёшь его в руки.",
            "你早已挂在墙上的剑。你以为再也不必握起它。",
        },
        ["oggetto.spada_capitano.nome"] = new[] { "Spada del capitano", "Captain's Sword", "Espada del capitán", "Épée du capitaine", "Schwert des Hauptmanns", "Espada do capitão", "Меч капитана", "队长之剑" },
        ["oggetto.spada_capitano.descrizione"] = new[]
        {
            "Lama ben bilanciata di un capitano della guardia. Chi la portava non ha fatto in tempo a sguainarla.",
            "A well-balanced blade from a captain of the guard. Its owner never had time to draw it.",
            "Hoja bien equilibrada de un capitán de la guardia. Su dueño no tuvo tiempo de desenvainarla.",
            "Une lame bien équilibrée, celle d'un capitaine de la garde. Son porteur n'a pas eu le temps de la dégainer.",
            "Eine gut ausbalancierte Klinge eines Hauptmanns der Wache. Ihr Träger kam nicht mehr dazu, sie zu ziehen.",
            "Lâmina bem equilibrada de um capitão da guarda. Seu dono não teve tempo de desembainhá-la.",
            "Хорошо сбалансированный клинок капитана стражи. Его владелец так и не успел его обнажить.",
            "卫队长的均衡之刃。它的主人还没来得及拔剑。",
        },
        ["oggetto.ascia_legna.nome"] = new[] { "Ascia da legna", "Woodcutter's Axe", "Hacha de leñador", "Hache de bûcheron", "Holzfälleraxt", "Machado de lenhador", "Топор дровосека", "伐木斧" },
        ["oggetto.ascia_legna.descrizione"] = new[]
        {
            "Serviva a spaccare la legna per l'inverno. Quella notte ha imparato a fare altro.",
            "It was meant for splitting firewood for the winter. That night, it learned something else.",
            "Servía para partir leña para el invierno. Aquella noche aprendió a hacer otra cosa.",
            "Elle servait à fendre le bois pour l'hiver. Cette nuit-là, elle a appris autre chose.",
            "Sie diente dazu, Holz für den Winter zu spalten. In jener Nacht lernte sie etwas anderes.",
            "Servia para rachar lenha para o inverno. Naquela noite, aprendeu a fazer outra coisa.",
            "Им кололи дрова на зиму. Той ночью он научился другому.",
            "它本是为过冬劈柴而用。那一夜，它学会了别的事。",
        },
        ["oggetto.ascia_boia.nome"] = new[] { "Ascia del boia", "Executioner's Axe", "Hacha del verdugo", "Hache du bourreau", "Henkersaxt", "Machado do carrasco", "Топор палача", "刽子手之斧" },
        ["oggetto.ascia_boia.descrizione"] = new[]
        {
            "Pesante e senza pietà. Ogni tacca sul manico è una condanna eseguita.",
            "Heavy and merciless. Every notch on the haft is a sentence carried out.",
            "Pesada y despiadada. Cada muesca del mango es una condena cumplida.",
            "Lourde et impitoyable. Chaque encoche du manche est une sentence exécutée.",
            "Schwer und gnadenlos. Jede Kerbe im Stiel ist ein vollstrecktes Urteil.",
            "Pesado e impiedoso. Cada entalhe no cabo é uma sentença cumprida.",
            "Тяжёлый и беспощадный. Каждая зарубка на топорище — исполненный приговор.",
            "沉重而无情。斧柄上的每道刻痕，都是一次行刑。",
        },
        ["oggetto.mazza_ferrata.nome"] = new[] { "Mazza ferrata", "Spiked Mace", "Maza claveteada", "Masse cloutée", "Beschlagener Streitkolben", "Maça cravejada", "Шипастая булава", "铁刺钉锤" },
        ["oggetto.mazza_ferrata.descrizione"] = new[]
        {
            "Le borchie di ferro non tagliano: sfondano. Contro di lei le armature servono a poco.",
            "Iron studs don't cut: they crush. Armor does little against it.",
            "Los clavos de hierro no cortan: aplastan. Contra ella, las armaduras sirven de poco.",
            "Les clous de fer ne tranchent pas : ils défoncent. Les armures n'y peuvent pas grand-chose.",
            "Eisenbeschläge schneiden nicht, sie zertrümmern. Rüstungen helfen dagegen wenig.",
            "Os cravos de ferro não cortam: esmagam. Contra ela, armaduras pouco adiantam.",
            "Железные шипы не режут — они проламывают. Доспехи против неё почти бесполезны.",
            "铁刺不会切割，只会砸碎。盔甲对它几乎无用。",
        },
        ["oggetto.martello_ossa.nome"] = new[] { "Martello di ossa", "Bone Hammer", "Martillo de hueso", "Marteau d'os", "Knochenhammer", "Martelo de ossos", "Костяной молот", "骨锤" },
        ["oggetto.martello_ossa.descrizione"] = new[]
        {
            "Arma degli orchi, legata con tendini e ossa di chi l'ha subita.",
            "An orc weapon, bound with the sinews and bones of those who felt it.",
            "Arma de los orcos, atada con tendones y huesos de quienes la sufrieron.",
            "Une arme d'orc, liée avec les tendons et les os de ceux qui l'ont subie.",
            "Eine Orkwaffe, gebunden mit Sehnen und Knochen derer, die sie zu spüren bekamen.",
            "Arma dos orcs, amarrada com tendões e ossos de quem a sofreu.",
            "Оружие орков, связанное жилами и костями тех, кто испытал его удар.",
            "兽人的武器，用受害者的筋与骨捆扎而成。",
        },
        ["oggetto.spadone_cavaliere.nome"] = new[] { "Spadone del cavaliere", "Knight's Greatsword", "Mandoble del caballero", "Espadon du chevalier", "Zweihänder des Ritters", "Montante do cavaleiro", "Двуручный меч рыцаря", "骑士巨剑" },
        ["oggetto.spadone_cavaliere.descrizione"] = new[]
        {
            "Si impugna a due mani. Lasci indietro lo scudo, e con lui la prudenza.",
            "Wielded with both hands. You leave your shield behind, and caution with it.",
            "Se empuña a dos manos. Dejas atrás el escudo, y con él la prudencia.",
            "Il se manie à deux mains. Tu laisses ton bouclier derrière toi, et la prudence avec.",
            "Er wird mit beiden Händen geführt. Du lässt den Schild zurück – und mit ihm die Vorsicht.",
            "Empunhado com as duas mãos. Você deixa o escudo para trás, e a prudência junto.",
            "Его держат обеими руками. Щит остаётся позади, а вместе с ним и осторожность.",
            "需双手持握。你舍下盾牌，也舍下了谨慎。",
        },
        ["oggetto.scudo_vecchia_vita.nome"] = new[] { "Scudo della vecchia vita", "Shield of the Old Life", "Escudo de la vieja vida", "Bouclier de l'ancienne vie", "Schild des alten Lebens", "Escudo da vida antiga", "Щит прежней жизни", "旧日之盾" },
        ["oggetto.scudo_vecchia_vita.descrizione"] = new[]
        {
            "Ammaccato e familiare. Ha già fermato colpi che avrebbero dovuto ucciderti.",
            "Dented and familiar. It has already stopped blows that should have killed you.",
            "Abollado y familiar. Ya ha detenido golpes que deberían haberte matado.",
            "Cabossé et familier. Il a déjà arrêté des coups qui auraient dû te tuer.",
            "Verbeult und vertraut. Er hat schon Schläge aufgehalten, die dich hätten töten sollen.",
            "Amassado e familiar. Já deteve golpes que deveriam ter matado você.",
            "Помятый и привычный. Он уже останавливал удары, которые должны были тебя убить.",
            "坑坑洼洼，却无比熟悉。它曾挡下本该要你性命的攻击。",
        },
        ["oggetto.pavese_quercia.nome"] = new[] { "Pavese di quercia", "Oak Pavise", "Pavés de roble", "Pavois de chêne", "Eichenpavese", "Pavês de carvalho", "Дубовая павеза", "橡木大盾" },
        ["oggetto.pavese_quercia.descrizione"] = new[]
        {
            "Un muro di legno da portare con sé. Lento, ma quasi nulla lo attraversa.",
            "A wooden wall you carry with you. Slow, but almost nothing gets through.",
            "Un muro de madera que llevas contigo. Lento, pero casi nada lo atraviesa.",
            "Un mur de bois que l'on porte avec soi. Lent, mais presque rien ne le traverse.",
            "Eine Holzwand zum Mitnehmen. Langsam, aber fast nichts dringt hindurch.",
            "Uma muralha de madeira para levar consigo. Lento, mas quase nada o atravessa.",
            "Деревянная стена, которую носишь с собой. Медленно, зато почти ничто не пробьёт.",
            "一面随身携带的木墙。笨重，但几乎无物能穿透。",
        },
        ["oggetto.brocchiere_ferro.nome"] = new[] { "Brocchiere di ferro", "Iron Buckler", "Broquel de hierro", "Bocle de fer", "Eiserner Buckler", "Broquel de ferro", "Железный баклер", "铁制圆盾" },
        ["oggetto.brocchiere_ferro.descrizione"] = new[]
        {
            "Piccolo e leggero. Nelle mani giuste, una parata al momento esatto vale più di un muro.",
            "Small and light. In the right hands, a perfectly timed parry is worth more than a wall.",
            "Pequeño y ligero. En las manos adecuadas, un bloqueo en el momento justo vale más que un muro.",
            "Petit et léger. Entre de bonnes mains, une parade au bon moment vaut mieux qu'un mur.",
            "Klein und leicht. In den richtigen Händen ist eine Parade im exakten Moment mehr wert als eine Mauer.",
            "Pequeno e leve. Nas mãos certas, um bloqueio no momento exato vale mais que uma muralha.",
            "Маленький и лёгкий. В умелых руках парирование в нужный миг стоит больше, чем стена.",
            "小巧轻便。在行家手中，一次恰到好处的格挡胜过一堵墙。",
        },
        ["oggetto.giubba_cuoio.nome"] = new[] { "Giubba di cuoio imbottito", "Padded Leather Jerkin", "Jubón de cuero acolchado", "Pourpoint de cuir rembourré", "Gepolstertes Lederwams", "Gibão de couro acolchoado", "Стёганая кожаная куртка", "衬垫皮甲" },
        ["oggetto.giubba_cuoio.descrizione"] = new[]
        {
            "Leggera e silenziosa. Protegge poco, ma non ti rallenta.",
            "Light and quiet. It offers little protection, but it won't slow you down.",
            "Ligero y silencioso. Protege poco, pero no te frena.",
            "Léger et silencieux. Il protège peu, mais ne te ralentit pas.",
            "Leicht und leise. Es schützt wenig, bremst dich aber nicht.",
            "Leve e silencioso. Protege pouco, mas não deixa você mais lento.",
            "Лёгкая и бесшумная. Защищает слабо, зато не сковывает.",
            "轻便而安静。防护有限，但不拖累你。",
        },
        ["oggetto.cotta_maglia.nome"] = new[] { "Cotta di maglia rattoppata", "Patched Chainmail", "Cota de malla remendada", "Cotte de mailles rapiécée", "Geflicktes Kettenhemd", "Cota de malha remendada", "Залатанная кольчуга", "补缀锁子甲" },
        ["oggetto.cotta_maglia.descrizione"] = new[]
        {
            "Anelli vecchi e nuovi, ricuciti dopo troppe battaglie.",
            "Old rings and new, mended after too many battles.",
            "Anillas viejas y nuevas, remendadas tras demasiadas batallas.",
            "Des anneaux anciens et neufs, raccommodés après trop de batailles.",
            "Alte und neue Ringe, geflickt nach zu vielen Schlachten.",
            "Anéis velhos e novos, remendados depois de batalhas demais.",
            "Старые и новые кольца, залатанные после слишком многих битв.",
            "新旧铁环交错，在无数战斗后一再修补。",
        },
        ["oggetto.corazza_piastre.nome"] = new[] { "Corazza di piastre annerite", "Blackened Plate Armor", "Coraza de placas ennegrecidas", "Cuirasse de plaques noircies", "Geschwärzter Plattenpanzer", "Couraça de placas enegrecidas", "Почерневшие латы", "焦黑板甲" },
        ["oggetto.corazza_piastre.descrizione"] = new[]
        {
            "Piastre annerite dal fuoco. Pesa come una colpa, ma ferma quasi tutto.",
            "Plates blackened by fire. It weighs like guilt, but stops almost everything.",
            "Placas ennegrecidas por el fuego. Pesa como una culpa, pero lo detiene casi todo.",
            "Des plaques noircies par le feu. Elle pèse comme une faute, mais arrête presque tout.",
            "Vom Feuer geschwärzte Platten. Sie wiegt schwer wie Schuld, hält aber fast alles auf.",
            "Placas enegrecidas pelo fogo. Pesa como uma culpa, mas detém quase tudo.",
            "Пластины, почерневшие от огня. Тяжелы, как вина, но держат почти любой удар.",
            "被火焰熏黑的甲片。沉重如罪孽，却几乎刀枪不入。",
        },
        ["oggetto.zanna_lupo.nome"] = new[] { "Zanna di lupo", "Wolf Fang", "Colmillo de lobo", "Croc de loup", "Wolfszahn", "Presa de lobo", "Волчий клык", "狼牙" },
        ["oggetto.zanna_lupo.descrizione"] = new[]
        {
            "Un trofeo di caccia appeso a un laccio. Rende i colpi più feroci.",
            "A hunting trophy on a cord. It makes your blows fiercer.",
            "Un trofeo de caza colgado de un cordón. Hace tus golpes más feroces.",
            "Un trophée de chasse au bout d'un lacet. Il rend tes coups plus féroces.",
            "Eine Jagdtrophäe an einer Schnur. Sie macht deine Schläge wilder.",
            "Um troféu de caça pendurado num cordão. Deixa seus golpes mais ferozes.",
            "Охотничий трофей на шнурке. Делает удары свирепее.",
            "系在绳上的狩猎战利品。让你的攻击更加凶猛。",
        },
        ["oggetto.occhio_corvo.nome"] = new[] { "Occhio di corvo", "Raven's Eye", "Ojo de cuervo", "Œil de corbeau", "Rabenauge", "Olho de corvo", "Вороний глаз", "鸦眼" },
        ["oggetto.occhio_corvo.descrizione"] = new[]
        {
            "Il corvo vede dove colpire. Chi lo porta, anche.",
            "The raven sees where to strike. So does the one who wears it.",
            "El cuervo ve dónde golpear. Quien lo lleva, también.",
            "Le corbeau voit où frapper. Celui qui le porte aussi.",
            "Der Rabe sieht, wohin er schlagen muss. Wer es trägt, ebenfalls.",
            "O corvo vê onde golpear. Quem o carrega também.",
            "Ворон видит, куда бить. Тот, кто его носит, — тоже.",
            "乌鸦看得见该往哪里下手。佩戴者亦然。",
        },
        ["oggetto.pietra_focolare.nome"] = new[] { "Pietra del focolare", "Stone of the Hearth", "Piedra del hogar", "Pierre du foyer", "Herdstein", "Pedra da lareira", "Камень очага", "炉边之石" },
        ["oggetto.pietra_focolare.descrizione"] = new[]
        {
            "Una pietra presa dal focolare di casa. È ancora tiepida, e ti protegge.",
            "A stone taken from the hearth at home. Still warm, it protects you.",
            "Una piedra tomada del hogar de casa. Aún está tibia, y te protege.",
            "Une pierre prise dans le foyer de la maison. Encore tiède, elle te protège.",
            "Ein Stein aus dem heimischen Herd. Noch warm – und er beschützt dich.",
            "Uma pedra tirada da lareira de casa. Ainda está morna, e protege você.",
            "Камень из домашнего очага. Он ещё тёплый и защищает тебя.",
            "取自家中炉灶的石头。它仍有余温，守护着你。",
        },
        ["oggetto.cuore_brace.nome"] = new[] { "Cuore di brace", "Ember Heart", "Corazón de brasa", "Cœur de braise", "Glutherz", "Coração de brasa", "Тлеющее сердце", "余烬之心" },
        ["oggetto.cuore_brace.descrizione"] = new[]
        {
            "Arde della vita di chi cade sotto i tuoi colpi.",
            "It burns with the life of those who fall to your blows.",
            "Arde con la vida de quienes caen bajo tus golpes.",
            "Il brûle de la vie de ceux qui tombent sous tes coups.",
            "Es glüht vom Leben derer, die unter deinen Schlägen fallen.",
            "Arde com a vida de quem cai sob seus golpes.",
            "Оно горит жизнью тех, кто пал от твоих ударов.",
            "以倒在你手下之人的生命燃烧。",
        },
        ["oggetto.respiro_lago.nome"] = new[] { "Respiro del lago", "Breath of the Lake", "Aliento del lago", "Souffle du lac", "Atem des Sees", "Fôlego do lago", "Дыхание озера", "湖之息" },
        ["oggetto.respiro_lago.descrizione"] = new[]
        {
            "Una goccia d'acqua nera chiusa nel vetro. Con lei il fiato torna più in fretta.",
            "A drop of black water sealed in glass. With it, your breath returns faster.",
            "Una gota de agua negra encerrada en vidrio. Con ella, el aliento vuelve más rápido.",
            "Une goutte d'eau noire enfermée dans du verre. Avec elle, le souffle revient plus vite.",
            "Ein Tropfen schwarzen Wassers, in Glas eingeschlossen. Mit ihm kehrt der Atem schneller zurück.",
            "Uma gota de água negra presa no vidro. Com ela, o fôlego volta mais depressa.",
            "Капля чёрной воды, заключённая в стекло. С ней дыхание возвращается быстрее.",
            "封在玻璃中的一滴黑水。有它在，气息恢复得更快。",
        },
        ["oggetto.sangue_antico.nome"] = new[] { "Sangue antico", "Ancient Blood", "Sangre antigua", "Sang ancien", "Uraltes Blut", "Sangue antigo", "Древняя кровь", "远古之血" },
        ["oggetto.sangue_antico.descrizione"] = new[]
        {
            "Sangue di un'era dimenticata. Ogni ferita che infliggi te ne restituisce un po'.",
            "Blood from a forgotten age. Every wound you inflict gives a little back to you.",
            "Sangre de una era olvidada. Cada herida que infliges te devuelve un poco.",
            "Le sang d'une ère oubliée. Chaque blessure que tu infliges t'en rend un peu.",
            "Blut aus einem vergessenen Zeitalter. Jede Wunde, die du schlägst, gibt dir ein wenig zurück.",
            "Sangue de uma era esquecida. Cada ferida que você causa devolve um pouco a você.",
            "Кровь забытой эпохи. Каждая нанесённая тобой рана немного возвращает тебе.",
            "来自被遗忘时代的血。你造成的每道伤口，都会回馈你一些。",
        },
        // ---------- Stregone: inventario, statistiche e tipi (Docs/incantesimi-stregone.md) ----------
        ["inv.solo_guerriero"] = new[] { "Questo oggetto è del Guerriero.", "This item is for the Warrior.", "Este objeto es del Guerrero.", "Cet objet est réservé au Guerrier.", "Dieser Gegenstand ist für den Krieger.", "Este item é do Guerreiro.", "Этот предмет для Воина.", "此物品属于战士。" },
        ["inv.solo_ladro"] = new[] { "Questo oggetto è del Ladro.", "This item is for the Thief.", "Este objeto es del Ladrón.", "Cet objet est réservé au Voleur.", "Dieser Gegenstand ist für den Dieb.", "Este item é do Ladrão.", "Этот предмет для Вора.", "此物品属于盗贼。" },
        ["inv.solo_stregone"] = new[] { "Questo oggetto è dello Stregone.", "This item is for the Sorcerer.", "Este objeto es del Hechicero.", "Cet objet est réservé au Sorcier.", "Dieser Gegenstand ist für den Zauberer.", "Este item é do Feiticeiro.", "Этот предмет для Колдуна.", "此物品属于术士。" },
        ["inv.amuleto_solo_ladro"] = new[] { "Questo amuleto è solo del Ladro.", "This amulet is for the Thief only.", "Este amuleto es solo del Ladrón.", "Cette amulette est réservée au Voleur.", "Dieses Amulett ist nur für den Dieb.", "Este amuleto é só do Ladrão.", "Этот амулет только для Вора.", "此护符只有盗贼能用。" },
        ["inv.libro"] = new[] { "Libro", "Book", "Libro", "Livre", "Buch", "Livro", "Книга", "书" },
        ["inv.libri"] = new[] { "Libri", "Books", "Libros", "Livres", "Bücher", "Livros", "Книги", "书籍" },
        ["inv.no_scudo_stregone"] = new[] { "Lo Stregone non usa scudi.", "The Sorcerer does not use shields.", "El Hechicero no usa escudos.", "Le Sorcier n'utilise pas de bouclier.", "Der Zauberer benutzt keine Schilde.", "O Feiticeiro não usa escudos.", "Колдун не пользуется щитами.", "术士不使用盾牌。" },
        ["inv.no_libro"] = new[] { "I libri sono solo dello Stregone.", "Books are for the Sorcerer only.", "Los libros son solo del Hechicero.", "Les livres sont réservés au Sorcier.", "Bücher sind nur für den Zauberer.", "Os livros são só do Feiticeiro.", "Книги только для Колдуна.", "书只有术士能用。" },
        ["stat.costo_mana"] = new[] { "Costo mana", "Mana cost", "Coste de maná", "Coût de mana", "Manakosten", "Custo de mana", "Расход маны", "法力消耗" },
        ["tipo.bastone"] = new[] { "Bastone", "Staff", "Báculo", "Bâton", "Stab", "Cajado", "Посох", "法杖" },
        ["tipo.bastone_due_mani"] = new[] { "Bastone a due mani", "Two-handed staff", "Báculo a dos manos", "Bâton à deux mains", "Zweihändiger Stab", "Cajado de duas mãos", "Двуручный посох", "双手法杖" },
        ["tipo.verga"] = new[] { "Verga", "Wand", "Vara", "Baguette", "Rute", "Varinha", "Жезл", "短杖" },
        ["tipo.libro"] = new[] { "Libro di incantesimi", "Spellbook", "Libro de hechizos", "Grimoire", "Zauberbuch", "Livro de feitiços", "Книга заклинаний", "法术书" },
        ["tipo.veste_leggera"] = new[] { "Veste leggera", "Light robe", "Túnica ligera", "Robe légère", "Leichte Robe", "Veste leve", "Лёгкое одеяние", "轻便法袍" },
        ["tipo.veste_media"] = new[] { "Veste media", "Medium robe", "Túnica media", "Robe intermédiaire", "Mittlere Robe", "Veste média", "Среднее одеяние", "中型法袍" },
        ["tipo.veste_pesante"] = new[] { "Veste pesante", "Heavy robe", "Túnica pesada", "Robe lourde", "Schwere Robe", "Veste pesada", "Тяжёлое одеяние", "厚重法袍" },
        // ---------- oggetti dello Stregone (Docs/incantesimi-stregone.md) ----------
        ["oggetto.bastone_vecchia_vita.nome"] = new[] { "Bastone della vecchia vita", "Staff of the Old Life", "Báculo de la vieja vida", "Bâton de l'ancienne vie", "Stab des alten Lebens", "Cajado da vida antiga", "Посох прежней жизни", "旧日之杖" },
        ["oggetto.bastone_vecchia_vita.descrizione"] = new[]
        {
            "Il bastone con cui camminavi fino al lago. Non sapevi che ascoltasse.",
            "The staff you walked to the lake with. You did not know it was listening.",
            "El báculo con el que caminabas hasta el lago. No sabías que escuchaba.",
            "Le bâton avec lequel tu marchais jusqu'au lac. Tu ne savais pas qu'il écoutait.",
            "Der Stab, mit dem du zum See gingst. Du wusstest nicht, dass er zuhörte.",
            "O cajado com que você caminhava até o lago. Não sabia que ele escutava.",
            "Посох, с которым ты ходил к озеру. Ты не знал, что он слушает.",
            "你常拄着走到湖边的手杖。你不知道它一直在聆听。",
        },
        ["oggetto.bastone_quercia_nera.nome"] = new[] { "Bastone di quercia nera", "Black Oak Staff", "Báculo de roble negro", "Bâton de chêne noir", "Schwarzeichenstab", "Cajado de carvalho negro", "Посох из чёрного дуба", "黑橡法杖" },
        ["oggetto.bastone_quercia_nera.descrizione"] = new[]
        {
            "Legno di una quercia colpita dal fulmine. Trattiene ancora un po' del suo fuoco.",
            "Wood from an oak struck by lightning. It still holds a little of its fire.",
            "Madera de un roble alcanzado por un rayo. Aún guarda algo de su fuego.",
            "Le bois d'un chêne frappé par la foudre. Il garde encore un peu de son feu.",
            "Holz einer vom Blitz getroffenen Eiche. Es bewahrt noch ein wenig von seinem Feuer.",
            "Madeira de um carvalho atingido por um raio. Ainda guarda um pouco do seu fogo.",
            "Древесина дуба, в который ударила молния. В ней ещё теплится его огонь.",
            "取自遭雷击的橡树。它仍留着几分雷火。",
        },
        ["oggetto.bastone_ossidiana.nome"] = new[] { "Bastone d'ossidiana", "Obsidian Staff", "Báculo de obsidiana", "Bâton d'obsidienne", "Obsidianstab", "Cajado de obsidiana", "Обсидиановый посох", "黑曜石法杖" },
        ["oggetto.bastone_ossidiana.descrizione"] = new[]
        {
            "Pietra nera e tagliente in cima a un'asta di frassino. La magia che lancia passa anche il ferro.",
            "Sharp black stone atop an ash shaft. The magic it casts cuts even through iron.",
            "Piedra negra y afilada sobre un asta de fresno. La magia que lanza atraviesa incluso el hierro.",
            "Une pierre noire et tranchante au sommet d'une hampe de frêne. Sa magie traverse même le fer.",
            "Scharfer schwarzer Stein auf einem Eschenschaft. Seine Magie dringt sogar durch Eisen.",
            "Pedra negra e afiada sobre uma haste de freixo. A magia que lança atravessa até o ferro.",
            "Острый чёрный камень на ясеневом древке. Его магия пробивает даже железо.",
            "白蜡木杖顶嵌着锋利的黑石。它施放的魔法连铁甲也能穿透。",
        },
        ["oggetto.verga_osso.nome"] = new[] { "Verga d'osso", "Bone Wand", "Vara de hueso", "Baguette d'os", "Knochenrute", "Varinha de osso", "Костяной жезл", "骨制短杖" },
        ["oggetto.verga_osso.descrizione"] = new[]
        {
            "Un osso inciso dagli sciamani degli orchi. La magia costa meno, ma esce più debole e più lenta.",
            "A bone carved by orc shamans. Magic costs less, but comes out weaker and slower.",
            "Un hueso tallado por los chamanes orcos. La magia cuesta menos, pero sale más débil y lenta.",
            "Un os gravé par les chamanes orques. La magie coûte moins, mais sort plus faible et plus lente.",
            "Ein von Ork-Schamanen beschnitzter Knochen. Magie kostet weniger, kommt aber schwächer und langsamer.",
            "Um osso entalhado pelos xamãs orcs. A magia custa menos, mas sai mais fraca e mais lenta.",
            "Кость с резьбой орочьих шаманов. Магия дешевле, но слабее и медленнее.",
            "兽人萨满雕刻的骨头。施法消耗更少，但威力更弱、速度更慢。",
        },
        ["oggetto.bastone_lago.nome"] = new[] { "Bastone del lago", "Staff of the Lake", "Báculo del lago", "Bâton du lac", "Stab des Sees", "Cajado do lago", "Посох озера", "湖之法杖" },
        ["oggetto.bastone_lago.descrizione"] = new[]
        {
            "Ripescato dal fondo del Lago Nero. Servono due mani per reggerlo, e il libro deve restare chiuso.",
            "Dragged up from the bottom of the Black Lake. It takes two hands to hold, and the book must stay closed.",
            "Sacado del fondo del Lago Negro. Hacen falta dos manos para sostenerlo, y el libro debe quedar cerrado.",
            "Repêché au fond du Lac Noir. Il faut deux mains pour le tenir, et le livre doit rester fermé.",
            "Vom Grund des Schwarzen Sees geborgen. Man braucht beide Hände, und das Buch muss geschlossen bleiben.",
            "Resgatado do fundo do Lago Negro. São precisas duas mãos para segurá-lo, e o livro deve ficar fechado.",
            "Поднят со дна Чёрного озера. Его держат двумя руками, а книга должна оставаться закрытой.",
            "从黑湖湖底打捞上来。需双手才能握住，书也只能合上。",
        },
        ["oggetto.libro_vecchia_vita.nome"] = new[] { "Libro della vecchia vita", "Book of the Old Life", "Libro de la vieja vida", "Livre de l'ancienne vie", "Buch des alten Lebens", "Livro da vida antiga", "Книга прежней жизни", "旧日之书" },
        ["oggetto.libro_vecchia_vita.descrizione"] = new[]
        {
            "Le tue annotazioni di un tempo, ricette e preghiere. Tra le pagine c'è ancora il disegno di tuo figlio.",
            "Your old notes, recipes and prayers. Your son's drawing is still between the pages.",
            "Tus viejas notas, recetas y oraciones. Entre las páginas sigue el dibujo de tu hijo.",
            "Tes anciennes notes, recettes et prières. Le dessin de ton fils est encore entre les pages.",
            "Deine alten Notizen, Rezepte und Gebete. Zwischen den Seiten liegt noch die Zeichnung deines Sohnes.",
            "Suas antigas anotações, receitas e orações. O desenho do seu filho ainda está entre as páginas.",
            "Твои старые записи, рецепты и молитвы. Между страницами всё ещё лежит рисунок твоего сына.",
            "你往日的笔记、药方与祷词。书页间仍夹着你儿子的画。",
        },
        ["oggetto.libro_sussurri.nome"] = new[] { "Libro dei sussurri", "Book of Whispers", "Libro de los susurros", "Livre des murmures", "Buch des Flüsterns", "Livro dos sussurros", "Книга шёпотов", "低语之书" },
        ["oggetto.libro_sussurri.descrizione"] = new[]
        {
            "Le pagine sono bianche finché non c'è silenzio. Allora parlano, e c'è posto per due incantesimi in più.",
            "The pages stay blank until there is silence. Then they speak, and there is room for two more spells.",
            "Las páginas están en blanco hasta que hay silencio. Entonces hablan, y caben dos hechizos más.",
            "Les pages restent blanches jusqu'au silence. Alors elles parlent, et il y a place pour deux sorts de plus.",
            "Die Seiten bleiben leer, bis Stille herrscht. Dann sprechen sie, und es gibt Platz für zwei weitere Zauber.",
            "As páginas ficam em branco até haver silêncio. Então falam, e cabem mais dois feitiços.",
            "Страницы пусты, пока не наступит тишина. Тогда они говорят, и в них есть место ещё для двух заклинаний.",
            "书页一片空白，直到四下寂静。那时它们开口低语，还能多容纳两个法术。",
        },
        ["oggetto.libro_braci.nome"] = new[] { "Libro delle braci", "Book of Embers", "Libro de las brasas", "Livre des braises", "Buch der Glut", "Livro das brasas", "Книга углей", "余烬之书" },
        ["oggetto.libro_braci.descrizione"] = new[]
        {
            "La copertina è sempre tiepida. Gli incantesimi letti da qui tornano pronti prima.",
            "The cover is always warm. Spells read from it are ready again sooner.",
            "La cubierta siempre está tibia. Los hechizos leídos de aquí vuelven a estar listos antes.",
            "La couverture est toujours tiède. Les sorts qu'on y lit sont prêts plus tôt.",
            "Der Einband ist immer warm. Zauber daraus sind früher wieder bereit.",
            "A capa está sempre morna. Os feitiços lidos daqui ficam prontos mais cedo.",
            "Обложка всегда тёплая. Заклинания из этой книги снова готовы быстрее.",
            "封面总是温热的。从中念出的法术冷却得更快。",
        },
        ["oggetto.libro_evocatore.nome"] = new[] { "Libro dell'evocatore", "Summoner's Book", "Libro del invocador", "Livre de l'invocateur", "Buch des Beschwörers", "Livro do invocador", "Книга призывателя", "召唤者之书" },
        ["oggetto.libro_evocatore.descrizione"] = new[]
        {
            "Un elenco di nomi, molti cancellati. Ciò che chiami con queste pagine resta con te più a lungo.",
            "A list of names, many crossed out. What you call with these pages stays with you longer.",
            "Una lista de nombres, muchos tachados. Lo que llamas con estas páginas se queda contigo más tiempo.",
            "Une liste de noms, beaucoup rayés. Ce que tu appelles avec ces pages reste plus longtemps avec toi.",
            "Eine Liste von Namen, viele durchgestrichen. Was du mit diesen Seiten rufst, bleibt länger bei dir.",
            "Uma lista de nomes, muitos riscados. O que você chama com estas páginas fica mais tempo ao seu lado.",
            "Список имён, многие вычеркнуты. То, что ты призываешь этими страницами, остаётся с тобой дольше.",
            "一串名字，许多已被划去。借这些书页召唤之物，会在你身边停留更久。",
        },
        ["oggetto.libro_lago_nero.nome"] = new[] { "Libro del lago nero", "Book of the Black Lake", "Libro del lago negro", "Livre du lac noir", "Buch des Schwarzen Sees", "Livro do lago negro", "Книга Чёрного озера", "黑湖之书" },
        ["oggetto.libro_lago_nero.descrizione"] = new[]
        {
            "Pesante e gonfio d'acqua, scritto in una lingua che nessuno al villaggio sapeva leggere. Tu, adesso, sì.",
            "Heavy and swollen with water, written in a tongue no one in the village could read. Now you can.",
            "Pesado e hinchado de agua, escrito en una lengua que nadie en la aldea sabía leer. Ahora tú sí.",
            "Lourd et gonflé d'eau, écrit dans une langue que personne au village ne savait lire. Toi, maintenant, si.",
            "Schwer und vom Wasser aufgequollen, in einer Sprache, die im Dorf niemand lesen konnte. Du kannst es jetzt.",
            "Pesado e inchado de água, escrito numa língua que ninguém na aldeia sabia ler. Agora você sabe.",
            "Тяжёлая, разбухшая от воды, написанная на языке, который никто в деревне не умел читать. А ты теперь умеешь.",
            "沉重且被水泡胀，用村里无人能读的文字写成。而如今，你读得懂了。",
        },
        ["oggetto.tunica_stracciata.nome"] = new[] { "Tunica stracciata", "Torn Tunic", "Túnica rasgada", "Tunique déchirée", "Zerrissene Tunika", "Túnica rasgada", "Рваная туника", "破旧长衫" },
        ["oggetto.tunica_stracciata.descrizione"] = new[]
        {
            "Quello che avevi addosso la notte della razzia. Non protegge quasi niente, ma non pesa.",
            "What you were wearing on the night of the raid. It protects almost nothing, but it weighs nothing.",
            "Lo que llevabas puesto la noche del saqueo. Casi no protege, pero no pesa.",
            "Ce que tu portais la nuit du raid. Elle ne protège presque rien, mais elle ne pèse rien.",
            "Was du in der Nacht des Überfalls getragen hast. Sie schützt kaum, aber sie wiegt nichts.",
            "O que você vestia na noite do ataque. Quase não protege, mas não pesa.",
            "То, что было на тебе в ночь набега. Почти не защищает, зато ничего не весит.",
            "劫掠之夜你身上穿的衣服。几乎毫无防护，却轻若无物。",
        },
        ["oggetto.veste_evocatore.nome"] = new[] { "Veste dell'evocatore", "Summoner's Robe", "Túnica del invocador", "Robe de l'invocateur", "Robe des Beschwörers", "Veste do invocador", "Одеяние призывателя", "召唤者法袍" },
        ["oggetto.veste_evocatore.descrizione"] = new[]
        {
            "Lana scura con rune cucite all'interno, dove solo chi la indossa le sente.",
            "Dark wool with runes sewn on the inside, where only the wearer can feel them.",
            "Lana oscura con runas cosidas por dentro, donde solo quien la lleva las siente.",
            "De la laine sombre avec des runes cousues à l'intérieur, là où seul celui qui la porte les sent.",
            "Dunkle Wolle mit eingenähten Runen an der Innenseite, wo nur der Träger sie spürt.",
            "Lã escura com runas costuradas por dentro, onde só quem a veste as sente.",
            "Тёмная шерсть с рунами, вшитыми изнутри, где их чувствует лишь тот, кто её носит.",
            "深色羊毛，符文缝在内侧，只有穿着者才能感觉到。",
        },
        ["oggetto.manto_cenere.nome"] = new[] { "Manto di cenere", "Mantle of Ash", "Manto de ceniza", "Manteau de cendre", "Aschenmantel", "Manto de cinzas", "Плащ из пепла", "灰烬斗篷" },
        ["oggetto.manto_cenere.descrizione"] = new[]
        {
            "Tessuto grigio e spesso, che odora ancora di villaggio bruciato. Pesa, ma tiene la magia vicina.",
            "Thick grey cloth that still smells of the burned village. It is heavy, but it keeps magic close.",
            "Tela gris y gruesa que aún huele a aldea quemada. Pesa, pero mantiene la magia cerca.",
            "Un tissu gris et épais qui sent encore le village brûlé. Il pèse, mais garde la magie proche.",
            "Dicker grauer Stoff, der noch nach dem verbrannten Dorf riecht. Er ist schwer, hält die Magie aber nah.",
            "Tecido cinza e grosso que ainda cheira à aldeia queimada. Pesa, mas mantém a magia por perto.",
            "Плотная серая ткань, всё ещё пахнущая сожжённой деревней. Тяжёлый, но держит магию рядом.",
            "厚重的灰布，仍带着焚毁村庄的气味。很沉，却能把魔力留在身边。",
        },
        ["oggetto.osso_inciso.nome"] = new[] { "Osso inciso", "Carved Bone", "Hueso tallado", "Os gravé", "Geschnitzter Knochen", "Osso entalhado", "Резная кость", "刻纹之骨" },
        ["oggetto.osso_inciso.descrizione"] = new[]
        {
            "Un frammento d'osso coperto di segni minuscoli. Ti dà più mana, ma ti succhia un po' di vita.",
            "A shard of bone covered in tiny marks. It gives you more mana, but drains a little of your life.",
            "Un fragmento de hueso cubierto de signos diminutos. Te da más maná, pero te quita algo de vida.",
            "Un éclat d'os couvert de signes minuscules. Il te donne plus de mana, mais te prend un peu de vie.",
            "Ein Knochensplitter voller winziger Zeichen. Er gibt dir mehr Mana, saugt dir aber etwas Leben ab.",
            "Um fragmento de osso coberto de sinais minúsculos. Dá mais mana, mas tira um pouco da sua vida.",
            "Осколок кости, покрытый крошечными знаками. Даёт больше маны, но отнимает немного жизни.",
            "布满细小刻痕的骨片。它赐你更多法力，却会吸走些许生命。",
        },
        ["oggetto.cristallo_opaco.nome"] = new[] { "Cristallo opaco", "Clouded Crystal", "Cristal opaco", "Cristal opaque", "Trüber Kristall", "Cristal opaco", "Мутный кристалл", "浑浊水晶" },
        ["oggetto.cristallo_opaco.descrizione"] = new[]
        {
            "Dentro si muove qualcosa, come nebbia. Il mana torna prima, ma le mani si fanno lente.",
            "Something moves inside, like fog. Mana returns sooner, but your hands grow slow.",
            "Algo se mueve dentro, como niebla. El maná vuelve antes, pero las manos se vuelven lentas.",
            "Quelque chose bouge à l'intérieur, comme du brouillard. Le mana revient plus tôt, mais les mains ralentissent.",
            "Darin bewegt sich etwas, wie Nebel. Das Mana kehrt früher zurück, doch die Hände werden langsam.",
            "Algo se move lá dentro, como névoa. O mana volta antes, mas as mãos ficam lentas.",
            "Внутри что-то движется, как туман. Мана возвращается быстрее, но руки становятся медленнее.",
            "其中有物在动，如雾一般。法力恢复更快，双手却变得迟缓。",
        },
        ["oggetto.cenere_benedetta.nome"] = new[] { "Cenere benedetta", "Blessed Ash", "Ceniza bendita", "Cendre bénie", "Gesegnete Asche", "Cinza abençoada", "Благословенный пепел", "受祝之灰" },
        ["oggetto.cenere_benedetta.descrizione"] = new[]
        {
            "Cenere della chiesetta, chiusa in un sacchetto. Rende gli incantesimi più forti, ma il mana ne soffre.",
            "Ash from the little church, sealed in a pouch. It makes your spells stronger, but your mana suffers.",
            "Ceniza de la capilla, guardada en una bolsita. Hace más fuertes los hechizos, pero el maná se resiente.",
            "De la cendre de la chapelle, gardée dans une bourse. Elle renforce les sorts, mais le mana en souffre.",
            "Asche aus der kleinen Kirche, in einem Beutel verwahrt. Sie stärkt die Zauber, doch das Mana leidet.",
            "Cinza da capela, guardada numa bolsinha. Deixa os feitiços mais fortes, mas o mana sofre.",
            "Пепел из часовни в маленьком мешочке. Делает заклинания сильнее, но мана страдает.",
            "小教堂的灰烬，封在一只小袋里。法术因它更强，法力却受了损。",
        },
        ["oggetto.sigillo_focolare.nome"] = new[] { "Sigillo del focolare", "Seal of the Hearth", "Sello del hogar", "Sceau du foyer", "Siegel des Herdes", "Selo da lareira", "Печать очага", "炉火印记" },
        ["oggetto.sigillo_focolare.descrizione"] = new[]
        {
            "Il sigillo della tua casa, ora che la casa non c'è più. Ogni nemico che cade ti ridà un po' di mana.",
            "The seal of your house, now that the house is gone. Every enemy that falls gives you back some mana.",
            "El sello de tu casa, ahora que la casa ya no existe. Cada enemigo que cae te devuelve algo de maná.",
            "Le sceau de ta maison, maintenant qu'elle n'est plus. Chaque ennemi qui tombe te rend un peu de mana.",
            "Das Siegel deines Hauses, jetzt, da es kein Haus mehr gibt. Jeder Feind, der fällt, gibt dir etwas Mana zurück.",
            "O selo da sua casa, agora que a casa não existe mais. Cada inimigo que cai devolve um pouco de mana.",
            "Печать твоего дома, которого больше нет. Каждый павший враг возвращает тебе немного маны.",
            "你家的印记，如今家已不在。每倒下一个敌人，都会还你些许法力。",
        },
        ["oggetto.occhio_lago.nome"] = new[] { "Occhio del lago", "Eye of the Lake", "Ojo del lago", "Œil du lac", "Auge des Sees", "Olho do lago", "Око озера", "湖之眼" },
        ["oggetto.occhio_lago.descrizione"] = new[]
        {
            "Una pietra liscia e azzurra che sembra guardarti. Puoi chiamare due lupi e due bambole insieme, ma durano meno e tu sei più fragile.",
            "A smooth blue stone that seems to watch you. You can call two wolves and two dolls at once, but they last less and you are more fragile.",
            "Una piedra lisa y azul que parece mirarte. Puedes invocar dos lobos y dos muñecas a la vez, pero duran menos y tú eres más frágil.",
            "Une pierre lisse et bleue qui semble te regarder. Tu peux appeler deux loups et deux poupées à la fois, mais ils durent moins et tu es plus fragile.",
            "Ein glatter blauer Stein, der dich anzusehen scheint. Du kannst zwei Wölfe und zwei Puppen zugleich rufen, doch sie halten kürzer und du bist zerbrechlicher.",
            "Uma pedra lisa e azul que parece olhar para você. Pode chamar dois lobos e duas bonecas ao mesmo tempo, mas duram menos e você fica mais frágil.",
            "Гладкий синий камень, будто следящий за тобой. Можно призвать двух волков и две куклы сразу, но они живут меньше, а ты становишься хрупче.",
            "一块光滑的蓝石，仿佛在注视着你。可同时召唤两只狼和两个骨偶，但它们持续更短，你也更脆弱。",
        },
        ["oggetto.cuore_lago_nero.nome"] = new[] { "Cuore del lago nero", "Heart of the Black Lake", "Corazón del lago negro", "Cœur du lac noir", "Herz des Schwarzen Sees", "Coração do lago negro", "Сердце Чёрного озера", "黑湖之心" },
        ["oggetto.cuore_lago_nero.descrizione"] = new[]
        {
            "Batte piano, freddo, sul tuo petto. Parte del danno dei tuoi incantesimi torna a te come mana, e prende vita in cambio.",
            "It beats slowly, cold, against your chest. Part of your spells' damage returns to you as mana, and it takes life in exchange.",
            "Late despacio, frío, sobre tu pecho. Parte del daño de tus hechizos vuelve a ti como maná, y a cambio toma vida.",
            "Il bat lentement, froid, contre ta poitrine. Une partie des dégâts de tes sorts te revient en mana, et il prend de la vie en échange.",
            "Es schlägt langsam und kalt auf deiner Brust. Ein Teil des Schadens deiner Zauber kehrt als Mana zurück, und es nimmt dafür Leben.",
            "Bate devagar, frio, no seu peito. Parte do dano dos seus feitiços volta como mana, e em troca leva vida.",
            "Оно медленно и холодно бьётся у тебя на груди. Часть урона заклинаний возвращается маной, а взамен оно забирает жизнь.",
            "它在你胸前缓慢而冰冷地跳动。你法术造成的部分伤害会化作法力回归，代价是你的生命。",
        },
        // ---------- Stregone: incantesimi, scuole, pro e contro (Docs/incantesimi-stregone.md) ----------
        ["inv.bastone"] = new[] { "Bastone", "Staff", "Báculo", "Bâton", "Stab", "Cajado", "Посох", "法杖" },
        ["inv.incantesimi"] = new[] { "Incantesimi", "Spells", "Hechizos", "Sorts", "Zauber", "Feitiços", "Заклинания", "法术" },
        ["inv.massimo_scuola"] = new[] { "Al massimo 2 incantesimi della stessa scuola.", "At most 2 spells from the same school.", "Como máximo 2 hechizos de la misma escuela.", "Au maximum 2 sorts de la même école.", "Höchstens 2 Zauber derselben Schule.", "No máximo 2 feitiços da mesma escola.", "Не больше 2 заклинаний одной школы.", "同一流派的法术最多2个。" },
        ["hud.nessun_incantesimo"] = new[] { "Nessun incantesimo in questa casella", "No spell in this slot", "Ningún hechizo en esta casilla", "Aucun sort dans cet emplacement", "Kein Zauber in diesem Feld", "Nenhum feitiço neste espaço", "В этой ячейке нет заклинания", "此栏位没有法术" },
        ["hud.non_pronto"] = new[] { "Non ancora pronto", "Not ready yet", "Aún no está listo", "Pas encore prêt", "Noch nicht bereit", "Ainda não está pronto", "Ещё не готово", "尚未就绪" },
        ["stat.carica"] = new[] { "Carica", "Cast time", "Carga", "Incantation", "Wirkzeit", "Carga", "Подготовка", "吟唱" },
        ["stat.attesa"] = new[] { "Attesa", "Cooldown", "Espera", "Recharge", "Abklingzeit", "Espera", "Перезарядка", "冷却" },
        ["tipo.incantesimo"] = new[] { "Incantesimo", "Spell", "Hechizo", "Sort", "Zauber", "Feitiço", "Заклинание", "法术" },
        ["scuola.brace"] = new[] { "Brace", "Embers", "Brasa", "Braise", "Glut", "Brasa", "Угли", "余烬" },
        ["scuola.lago_nero"] = new[] { "Lago Nero", "Black Lake", "Lago Negro", "Lac Noir", "Schwarzer See", "Lago Negro", "Чёрное озеро", "黑湖" },
        ["scuola.ombra"] = new[] { "Ombra", "Shadow", "Sombra", "Ombre", "Schatten", "Sombra", "Тень", "暗影" },
        ["scuola.evocazione"] = new[] { "Evocazione", "Summoning", "Invocación", "Invocation", "Beschwörung", "Invocação", "Призыв", "召唤" },
        ["mod.carica_s"] = new[] { "Carica", "Cast time", "Carga", "Incantation", "Wirkzeit", "Carga", "Подготовка", "吟唱" },
        ["mod.carica"] = new[] { "Carica", "Cast time", "Carga", "Incantation", "Wirkzeit", "Carga", "Подготовка", "吟唱" },
        ["mod.recupero"] = new[] { "Recupero", "Recovery", "Recuperación", "Récupération", "Erholung", "Recuperação", "Восстановление", "收招" },
        ["mod.attesa"] = new[] { "Attesa", "Cooldown", "Espera", "Recharge", "Abklingzeit", "Espera", "Перезарядка", "冷却" },
        ["mod.costo"] = new[] { "Costo mana", "Mana cost", "Coste de maná", "Coût de mana", "Manakosten", "Custo de mana", "Расход маны", "法力消耗" },
        ["mod.danno_incantesimi"] = new[] { "Danno incantesimi", "Spell damage", "Daño de hechizos", "Dégâts des sorts", "Zauberschaden", "Dano de feitiços", "Урон заклинаний", "法术伤害" },
        ["mod.danno"] = new[] { "Danno", "Damage", "Daño", "Dégâts", "Schaden", "Dano", "Урон", "伤害" },
        ["mod.durata_effetti"] = new[] { "Durata effetti", "Effect duration", "Duración de efectos", "Durée des effets", "Effektdauer", "Duração dos efeitos", "Длительность эффектов", "效果持续" },
        ["mod.ignora_armatura"] = new[] { "Ignora armatura", "Ignores armour", "Ignora armadura", "Ignore l'armure", "Ignoriert Rüstung", "Ignora armadura", "Игнор. брони", "无视护甲" },
        ["mod.vita_evocazioni"] = new[] { "Vita evocazioni", "Summon health", "Vida de invocaciones", "Vie des invocations", "Leben der Beschwörungen", "Vida das invocações", "Здоровье призванных", "召唤物生命" },
        ["mod.durata_evocazioni"] = new[] { "Durata evocazioni", "Summon duration", "Duración de invocaciones", "Durée des invocations", "Beschwörungsdauer", "Duração das invocações", "Длительность призыва", "召唤持续时间" },
        ["mod.carica_evocazioni"] = new[] { "Carica evocazioni", "Summon cast time", "Carga de invocaciones", "Incantation des invocations", "Wirkzeit der Beschwörungen", "Carga das invocações", "Подготовка призыва", "召唤吟唱" },
        ["mod.doppia_evocazione"] = new[] { "Due lupi e due bambole insieme", "Two wolves and two dolls at once", "Dos lobos y dos muñecas a la vez", "Deux loups et deux poupées à la fois", "Zwei Wölfe und zwei Puppen gleichzeitig", "Dois lobos e duas bonecas ao mesmo tempo", "Два волка и две куклы сразу", "可同时召唤两只狼和两个骨偶" },
        ["mod.caselle"] = new[] { "Caselle incantesimi", "Spell slots", "Casillas de hechizos", "Emplacements de sorts", "Zauberplätze", "Espaços de feitiços", "Ячейки заклинаний", "法术栏位" },
        ["mod.armatura"] = new[] { "Armatura", "Armour", "Armadura", "Armure", "Rüstung", "Armadura", "Броня", "护甲" },
        ["mod.armatura_percento"] = new[] { "Armatura", "Armour", "Armadura", "Armure", "Rüstung", "Armadura", "Броня", "护甲" },
        ["mod.danno_tutto"] = new[] { "Danno", "Damage", "Daño", "Dégâts", "Schaden", "Dano", "Урон", "伤害" },
        ["mod.critico"] = new[] { "Critico", "Critical", "Crítico", "Critique", "Kritisch", "Crítico", "Крит", "暴击" },
        ["mod.velocita_parata"] = new[] { "Velocità parata", "Block speed", "Velocidad de bloqueo", "Vitesse de parade", "Blocktempo", "Velocidade de bloqueio", "Скорость блока", "格挡速度" },
        ["mod.velocita_attacco"] = new[] { "Velocità d'attacco", "Attack speed", "Velocidad de ataque", "Vitesse d'attaque", "Angriffstempo", "Velocidade de ataque", "Скорость атаки", "攻击速度" },
        ["mod.vita_massima"] = new[] { "Vita massima", "Max health", "Vida máxima", "Vie maximale", "Maximales Leben", "Vida máxima", "Макс. здоровье", "生命上限" },
        ["mod.furtivita"] = new[] { "Furtività", "Stealth", "Sigilo", "Discrétion", "Heimlichkeit", "Furtividade", "Скрытность", "隐匿" },
        ["mod.resistenza_massima"] = new[] { "Resistenza massima", "Max stamina", "Aguante máximo", "Endurance maximale", "Maximale Ausdauer", "Vigor máximo", "Макс. выносливость", "耐力上限" },
        ["mod.mana_massimo"] = new[] { "Mana massimo", "Max mana", "Maná máximo", "Mana maximal", "Maximales Mana", "Mana máximo", "Макс. мана", "法力上限" },
        ["mod.mana_massimo_percento"] = new[] { "Mana massimo", "Max mana", "Maná máximo", "Mana maximal", "Maximales Mana", "Mana máximo", "Макс. мана", "法力上限" },
        ["mod.recupero_mana"] = new[] { "Ricarica mana", "Mana regeneration", "Regeneración de maná", "Régénération de mana", "Manaregeneration", "Regeneração de mana", "Восстановление маны", "法力恢复" },
        ["mod.vita_per_uccisione"] = new[] { "Vita per uccisione", "Health per kill", "Vida por muerte", "Vie par ennemi tué", "Leben pro Tötung", "Vida por abate", "Здоровье за убийство", "击杀回复生命" },
        ["mod.due_mani"] = new[] { "A due mani: niente libro", "Two-handed: no book", "A dos manos: sin libro", "À deux mains : pas de livre", "Zweihändig: kein Buch", "Duas mãos: sem livro", "Двуручный: без книги", "双手持握：无法用书" },
        ["mod.nessuno"] = new[] { "Nessun pro né contro", "No upsides or downsides", "Sin ventajas ni desventajas", "Ni avantage ni inconvénient", "Weder Vor- noch Nachteile", "Sem vantagens nem desvantagens", "Без плюсов и минусов", "无任何优缺点" },
        ["mod.mana_per_danno"] = new[] { "Mana perso a ogni danno ricevuto", "Mana lost on every hit taken", "Maná perdido por cada daño recibido", "Mana perdu à chaque dégât subi", "Manaverlust bei jedem Treffer", "Mana perdido a cada dano recebido", "Потеря маны при каждом уроне", "每次受伤损失法力" },
        ["mod.mana_per_schivata"] = new[] { "Mana per schivata", "Mana per dodge", "Maná por esquiva", "Mana par esquive", "Mana pro Ausweichen", "Mana por esquiva", "Мана за уклонение", "每次闪避消耗法力" },
        ["mod.recupero_resistenza"] = new[] { "Ricarica resistenza", "Stamina regeneration", "Recuperación de aguante", "Récupération d'endurance", "Ausdauerregeneration", "Recuperação de vigor", "Восстановление выносливости", "耐力恢复" },
        ["mod.ruba_vita"] = new[] { "Danno che torna come vita", "Damage returned as health", "Daño devuelto como vida", "Dégâts rendus en vie", "Schaden kehrt als Leben zurück", "Dano devolvido como vida", "Урон возвращается здоровьем", "伤害转化为生命" },
        ["mod.svanire"] = new[] { "Svanire nell'ombra (Q)", "Fade into shadow (Q)", "Desvanecerse en la sombra (Q)", "Disparaître dans l'ombre (Q)", "Im Schatten verschwinden (Q)", "Sumir nas sombras (Q)", "Раствориться в тени (Q)", "隐入暗影（Q）" },
        ["mod.mana_per_uccisione"] = new[] { "Mana per uccisione", "Mana per kill", "Maná por muerte", "Mana par ennemi tué", "Mana pro Tötung", "Mana por abate", "Мана за убийство", "击杀回复法力" },
        ["mod.ruba_mana"] = new[] { "Danno che torna come mana", "Damage returned as mana", "Daño devuelto como maná", "Dégâts rendus en mana", "Schaden kehrt als Mana zurück", "Dano devolvido como mana", "Урон возвращается маной", "伤害转化为法力" },
        ["oggetto.scintilla.nome"] = new[] { "Scintilla", "Spark", "Chispa", "Étincelle", "Funke", "Fagulha", "Искра", "火花" },
        ["oggetto.scintilla.descrizione"] = new[]
        {
            "Una piccola fiamma lanciata con un gesto della mano. Poca, ma non si spegne.",
            "A small flame flung with a flick of the hand. Little, but it does not go out.",
            "Una pequeña llama lanzada con un gesto de la mano. Poca, pero no se apaga.",
            "Une petite flamme lancée d'un geste de la main. Peu de chose, mais elle ne s'éteint pas.",
            "Eine kleine Flamme, mit einer Handbewegung geschleudert. Wenig, aber sie erlischt nicht.",
            "Uma pequena chama lançada com um gesto da mão. Pouca, mas não se apaga.",
            "Маленькое пламя, брошенное взмахом руки. Немного, но оно не гаснет.",
            "随手一挥抛出的一小团火焰。虽然微弱，却不会熄灭。",
        },
        ["oggetto.palla_fuoco.nome"] = new[] { "Palla di fuoco", "Fireball", "Bola de fuego", "Boule de feu", "Feuerball", "Bola de fogo", "Огненный шар", "火球" },
        ["oggetto.palla_fuoco.descrizione"] = new[]
        {
            "Una sfera lenta e pesante che scoppia dove arriva e brucia tutti quelli che ha intorno.",
            "A slow, heavy sphere that bursts where it lands and burns everyone around it.",
            "Una esfera lenta y pesada que estalla donde llega y quema a todos los que la rodean.",
            "Une sphère lente et lourde qui éclate là où elle arrive et brûle tous ceux qui l'entourent.",
            "Eine langsame, schwere Kugel, die beim Aufprall birst und alle ringsum verbrennt.",
            "Uma esfera lenta e pesada que explode onde cai e queima todos ao redor.",
            "Медленный тяжёлый шар, который взрывается там, куда долетит, и сжигает всех вокруг.",
            "缓慢而沉重的火球，落地即爆，灼烧周围所有人。",
        },
        ["oggetto.scia_brace.nome"] = new[] { "Scia di brace", "Trail of Embers", "Estela de brasas", "Traînée de braises", "Glutspur", "Rastro de brasas", "След углей", "余烬之径" },
        ["oggetto.scia_brace.descrizione"] = new[]
        {
            "Una striscia di fuoco a terra davanti a te. Chi ti insegue deve attraversarla.",
            "A strip of fire on the ground in front of you. Whoever chases you must cross it.",
            "Una franja de fuego en el suelo frente a ti. Quien te persiga tendrá que cruzarla.",
            "Une bande de feu au sol devant toi. Qui te poursuit doit la traverser.",
            "Ein Feuerstreifen am Boden vor dir. Wer dich verfolgt, muss hindurch.",
            "Uma faixa de fogo no chão à sua frente. Quem te persegue precisa atravessá-la.",
            "Полоса огня на земле перед тобой. Тому, кто гонится за тобой, придётся её пересечь.",
            "在你面前的地面燃起一道火焰。追你的人必须穿过它。",
        },
        ["oggetto.scheggia_ghiaccio.nome"] = new[] { "Scheggia di ghiaccio", "Ice Shard", "Esquirla de hielo", "Éclat de glace", "Eissplitter", "Lasca de gelo", "Ледяной осколок", "冰棱" },
        ["oggetto.scheggia_ghiaccio.descrizione"] = new[]
        {
            "Acqua del lago gelata in un istante. Chi viene colpito si muove più piano per qualche momento.",
            "Lake water frozen in an instant. Whoever is hit moves slower for a few moments.",
            "Agua del lago helada en un instante. Quien recibe el golpe se mueve más despacio un momento.",
            "L'eau du lac gelée en un instant. Celui qui est touché ralentit quelques instants.",
            "Seewasser, im Nu gefroren. Wer getroffen wird, bewegt sich kurz langsamer.",
            "Água do lago congelada num instante. Quem é atingido se move mais devagar por alguns momentos.",
            "Озёрная вода, замёрзшая в одно мгновение. Пораженный на несколько мгновений замедляется.",
            "瞬间冻结的湖水。被击中者会短暂减速。",
        },
        ["oggetto.onda_lago.nome"] = new[] { "Onda del lago", "Wave of the Lake", "Ola del lago", "Vague du lac", "Welle des Sees", "Onda do lago", "Волна озера", "湖之浪" },
        ["oggetto.onda_lago.descrizione"] = new[]
        {
            "Un'onda nera che travolge chi è più basso di lei e lo lascia stordito. I più grandi la sentono appena.",
            "A black wave that sweeps away anyone shorter than it and leaves them stunned. The largest barely feel it.",
            "Una ola negra que arrastra a quien es más bajo que ella y lo deja aturdido. Los más grandes apenas la notan.",
            "Une vague noire qui emporte ceux qui sont plus petits qu'elle et les laisse étourdis. Les plus grands la sentent à peine.",
            "Eine schwarze Welle, die alle Kleineren mitreißt und betäubt. Die Größten spüren sie kaum.",
            "Uma onda negra que arrasta quem é mais baixo que ela e o deixa atordoado. Os maiores mal a sentem.",
            "Чёрная волна сбивает всех, кто ниже её, и оглушает. Самые большие едва её чувствуют.",
            "黑色巨浪卷走比它矮的人并使其眩晕。最高大的敌人几乎毫无感觉。",
        },
        ["oggetto.pozza_nera.nome"] = new[] { "Pozza nera", "Black Pool", "Charco negro", "Flaque noire", "Schwarze Pfütze", "Poça negra", "Чёрная лужа", "黑潭" },
        ["oggetto.pozza_nera.descrizione"] = new[]
        {
            "L'acqua del lago sale dal terreno e tiene fermi i piedi. Quando lascia la presa, le gambe sono ancora pesanti.",
            "Lake water rises from the ground and holds feet in place. When it lets go, legs are still heavy.",
            "El agua del lago brota del suelo y sujeta los pies. Cuando suelta, las piernas siguen pesadas.",
            "L'eau du lac monte du sol et retient les pieds. Quand elle lâche prise, les jambes restent lourdes.",
            "Seewasser steigt aus dem Boden und hält die Füße fest. Lässt es los, sind die Beine noch schwer.",
            "A água do lago sobe do chão e prende os pés. Quando solta, as pernas continuam pesadas.",
            "Вода озера поднимается из земли и держит ноги. Когда отпускает, ноги ещё тяжелы.",
            "湖水从地下涌出，缠住双脚。即使松开，双腿依然沉重。",
        },
        ["oggetto.dardo_ombra.nome"] = new[] { "Dardo d'ombra", "Shadow Dart", "Dardo de sombra", "Dard d'ombre", "Schattenpfeil", "Dardo das sombras", "Теневой дротик", "暗影飞镖" },
        ["oggetto.dardo_ombra.descrizione"] = new[]
        {
            "Veloce e silenzioso. Fa il doppio del danno a chi non sa ancora che ci sei.",
            "Fast and silent. It deals double damage to those who do not yet know you are there.",
            "Rápido y silencioso. Hace el doble de daño a quien aún no sabe que estás ahí.",
            "Rapide et silencieux. Il inflige le double de dégâts à qui ignore encore ta présence.",
            "Schnell und lautlos. Doppelter Schaden gegen alle, die noch nicht wissen, dass du da bist.",
            "Rápido e silencioso. Causa o dobro de dano a quem ainda não sabe que você está ali.",
            "Быстрый и бесшумный. Наносит двойной урон тем, кто ещё не знает о тебе.",
            "迅捷无声。对尚未察觉你的敌人造成双倍伤害。",
        },
        ["oggetto.passo_ombra.nome"] = new[] { "Passo d'ombra", "Shadow Step", "Paso de sombra", "Pas d'ombre", "Schattenschritt", "Passo das sombras", "Теневой шаг", "暗影步" },
        ["oggetto.passo_ombra.descrizione"] = new[]
        {
            "Per qualche secondo diventi un'ombra veloce. Il primo incantesimo che lanci così colpisce durissimo, ma ti tradisce.",
            "For a few seconds you become a swift shadow. The first spell you cast like this hits very hard, but gives you away.",
            "Durante unos segundos te vuelves una sombra veloz. El primer hechizo que lances así golpea durísimo, pero te delata.",
            "Pendant quelques secondes, tu deviens une ombre rapide. Le premier sort lancé ainsi frappe très fort, mais te trahit.",
            "Für einige Sekunden wirst du zu einem schnellen Schatten. Der erste Zauber so trifft sehr hart, verrät dich aber.",
            "Por alguns segundos você vira uma sombra veloz. O primeiro feitiço lançado assim acerta muito forte, mas te denuncia.",
            "На несколько секунд ты становишься быстрой тенью. Первое заклинание так бьёт очень сильно, но выдаёт тебя.",
            "数秒内化为迅捷的影子。以此状态施放的第一个法术威力极大，但会暴露你。",
        },
        ["oggetto.velo_nebbia.nome"] = new[] { "Velo di nebbia", "Veil of Mist", "Velo de niebla", "Voile de brume", "Nebelschleier", "Véu de névoa", "Покров тумана", "雾之帷幕" },
        ["oggetto.velo_nebbia.descrizione"] = new[]
        {
            "La nebbia del lago ti avvolge. Finché resti dentro, ti vedono solo da vicinissimo.",
            "The lake mist wraps around you. As long as you stay inside, you can only be seen up close.",
            "La niebla del lago te envuelve. Mientras sigas dentro, solo te ven de muy cerca.",
            "La brume du lac t'enveloppe. Tant que tu restes dedans, on ne te voit que de tout près.",
            "Der Seenebel hüllt dich ein. Solange du darin bleibst, sieht man dich nur aus nächster Nähe.",
            "A névoa do lago te envolve. Enquanto ficar dentro, só te veem de muito perto.",
            "Туман озера окутывает тебя. Пока ты внутри, тебя видно лишь вблизи.",
            "湖雾将你笼罩。只要你待在其中，敌人只能在极近处看见你。",
        },
        ["oggetto.fuoco_fatuo.nome"] = new[] { "Fuoco fatuo", "Will-o'-the-Wisp", "Fuego fatuo", "Feu follet", "Irrlicht", "Fogo-fátuo", "Блуждающий огонёк", "鬼火" },
        ["oggetto.fuoco_fatuo.descrizione"] = new[]
        {
            "Una luce che ti gira attorno e lancia scintille al nemico più vicino. Fino a tre insieme.",
            "A light that circles you and throws sparks at the nearest enemy. Up to three at once.",
            "Una luz que gira a tu alrededor y lanza chispas al enemigo más cercano. Hasta tres a la vez.",
            "Une lumière qui tourne autour de toi et lance des étincelles sur l'ennemi le plus proche. Jusqu'à trois à la fois.",
            "Ein Licht, das dich umkreist und Funken auf den nächsten Feind wirft. Bis zu drei gleichzeitig.",
            "Uma luz que gira à sua volta e lança fagulhas no inimigo mais próximo. Até três ao mesmo tempo.",
            "Огонёк кружит вокруг тебя и бросает искры в ближайшего врага. До трёх сразу.",
            "环绕你飞舞的光点，向最近的敌人投掷火花。最多同时存在三个。",
        },
        ["oggetto.spirito_lupo.nome"] = new[] { "Spirito del lupo", "Wolf Spirit", "Espíritu del lobo", "Esprit du loup", "Wolfsgeist", "Espírito do lobo", "Дух волка", "狼之灵" },
        ["oggetto.spirito_lupo.descrizione"] = new[]
        {
            "Un lupo fatto di luce fredda combatte al tuo fianco. Ogni preda che abbatte lo rinforza.",
            "A wolf of cold light fights at your side. Every prey it brings down makes it stronger.",
            "Un lobo hecho de luz fría lucha a tu lado. Cada presa que derriba lo fortalece.",
            "Un loup de lumière froide combat à tes côtés. Chaque proie abattue le renforce.",
            "Ein Wolf aus kaltem Licht kämpft an deiner Seite. Jede erlegte Beute stärkt ihn.",
            "Um lobo de luz fria luta ao seu lado. Cada presa que derruba o fortalece.",
            "Волк из холодного света сражается рядом с тобой. Каждая добыча делает его сильнее.",
            "冷光化成的狼与你并肩作战。每击倒一个猎物，它就更强一分。",
        },
        ["oggetto.bambola_ossa.nome"] = new[] { "Bambola di ossa", "Bone Doll", "Muñeca de huesos", "Poupée d'os", "Knochenpuppe", "Boneca de ossos", "Костяная кукла", "骨偶" },
        ["oggetto.bambola_ossa.descrizione"] = new[]
        {
            "I nemici vicini non riescono a staccarle gli occhi di dosso. Se la rompono esce un gas; se nessuno la rompe, si alza.",
            "Nearby enemies cannot take their eyes off it. If they break it, gas spills out; if no one does, it rises.",
            "Los enemigos cercanos no pueden apartar la vista de ella. Si la rompen, sale un gas; si nadie la rompe, se levanta.",
            "Les ennemis proches ne peuvent en détacher les yeux. S'ils la brisent, un gaz s'échappe ; si personne ne le fait, elle se lève.",
            "Nahe Feinde können den Blick nicht von ihr lassen. Zerbrechen sie sie, strömt Gas aus; tut es niemand, erhebt sie sich.",
            "Os inimigos próximos não conseguem tirar os olhos dela. Se a quebram, sai um gás; se ninguém a quebra, ela se levanta.",
            "Ближние враги не могут отвести от неё глаз. Если её разбить, вырвется газ; если никто не разобьёт, она поднимется.",
            "附近的敌人无法将视线从它身上移开。被打碎时会喷出毒气；若无人打碎，它便会站起来。",
        },
        ["oggetto.bastone_cimitero.nome"] = new[] { "Bastone del cimitero", "Graveyard Staff", "Báculo del cementerio", "Bâton du cimetière", "Friedhofsstab", "Cajado do cemitério", "Кладбищенский посох", "墓园法杖" },
        ["oggetto.bastone_cimitero.descrizione"] = new[]
        {
            "Ricavato da una croce del vecchio cimitero. Chi chiami con lui torna più forte, ma gli altri incantesimi si affievoliscono.",
            "Carved from a cross of the old graveyard. What you call with it returns stronger, but your other spells grow faint.",
            "Tallado de una cruz del viejo cementerio. Lo que invocas con él vuelve más fuerte, pero tus demás hechizos se debilitan.",
            "Taillé dans une croix du vieux cimetière. Ce que tu invoques revient plus fort, mais tes autres sorts faiblissent.",
            "Aus einem Kreuz des alten Friedhofs geschnitzt. Was du damit rufst, kehrt stärker zurück, doch deine anderen Zauber werden schwach.",
            "Feito de uma cruz do velho cemitério. O que você invoca volta mais forte, mas seus outros feitiços enfraquecem.",
            "Вырезан из креста старого кладбища. Призванное им возвращается сильнее, но остальные заклинания слабеют.",
            "由旧墓园的十字架削成。以它召唤之物会更强，但其他法术随之衰弱。",
        },
    };
}
