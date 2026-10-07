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
        ["inv.titolo"] = new[] { "Equipaggiamento", "Equipment", "Equipo", "Équipement", "Ausrüstung", "Equipamento", "Снаряжение", "装备" },
        ["inv.zaino"] = new[] { "Zaino", "Backpack", "Mochila", "Sac", "Rucksack", "Mochila", "Рюкзак", "背包" },
        ["inv.arma"] = new[] { "Arma", "Weapon", "Arma", "Arme", "Waffe", "Arma", "Оружие", "武器" },
        ["inv.scudo"] = new[] { "Scudo", "Shield", "Escudo", "Bouclier", "Schild", "Escudo", "Щит", "盾牌" },
        ["inv.armatura"] = new[] { "Armatura", "Armor", "Armadura", "Armure", "Rüstung", "Armadura", "Доспех", "护甲" },
        ["inv.amuleto"] = new[] { "Amuleto", "Amulet", "Amuleto", "Amulette", "Amulett", "Amuleto", "Амулет", "护符" },
        ["inv.vuoto"] = new[] { "Vuoto", "Empty", "Vacío", "Vide", "Leer", "Vazio", "Пусто", "空" },
        ["inv.zaino_vuoto"] = new[] { "Lo zaino è vuoto", "Your backpack is empty", "La mochila está vacía", "Le sac est vide", "Der Rucksack ist leer", "A mochila está vazia", "Рюкзак пуст", "背包是空的" },
        ["inv.aiuto"] = new[] { "E  equipaggia / togli      Tab  chiudi      Frecce  scegli", "E  equip / remove      Tab  close      Arrows  select", "E  equipar / quitar      Tab  cerrar      Flechas  elegir", "E  équiper / retirer      Tab  fermer      Flèches  choisir", "E  ausrüsten / ablegen      Tab  schließen      Pfeiltasten  wählen", "E  equipar / remover      Tab  fechar      Setas  escolher", "E  надеть / снять      Tab  закрыть      Стрелки  выбрать", "E  装备 / 卸下      Tab  关闭      方向键  选择" },
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
    };
}
