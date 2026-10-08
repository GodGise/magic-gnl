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
    };
}
