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

        // ---------- partita: barre, inventario, oggetti ----------
        // ---------- co-op (pannello F9, Assets/Scripts/Rete/ReteCoop.cs) ----------
        ["rete.titolo"] = new[] { "Co-op (prova in locale)", "Co-op (local test)", "Cooperativo (prueba local)", "Coop (test local)", "Koop (lokaler Test)", "Cooperativo (teste local)", "Кооператив (локальный тест)", "合作模式（本地测试）" },
        ["rete.ospita"] = new[] { "Ospita (F6)", "Host (F6)", "Alojar (F6)", "Héberger (F6)", "Hosten (F6)", "Hospedar (F6)", "Создать (F6)", "创建房间 (F6)" },
        ["rete.entra"] = new[] { "Entra (F7)", "Join (F7)", "Unirse (F7)", "Rejoindre (F7)", "Beitreten (F7)", "Entrar (F7)", "Войти (F7)", "加入 (F7)" },
        ["rete.esci"] = new[] { "Disconnetti (F8)", "Disconnect (F8)", "Desconectar (F8)", "Déconnecter (F8)", "Trennen (F8)", "Desconectar (F8)", "Отключиться (F8)", "断开 (F8)" },
        ["rete.indirizzo"] = new[] { "Indirizzo dell'host", "Host address", "Dirección del anfitrión", "Adresse de l'hôte", "Adresse des Hosts", "Endereço do host", "Адрес хоста", "主机地址" },
        ["rete.giocatori"] = new[] { "Giocatori", "Players", "Jugadores", "Joueurs", "Spieler", "Jogadores", "Игроки", "玩家" },
        ["rete.host"] = new[] { "Sei l'host", "You are the host", "Eres el anfitrión", "Vous êtes l'hôte", "Du bist der Host", "Você é o host", "Вы хост", "你是主机" },
        ["rete.client"] = new[] { "Collegato a un host", "Connected to a host", "Conectado a un anfitrión", "Connecté à un hôte", "Mit Host verbunden", "Conectado a um host", "Подключено к хосту", "已连接到主机" },
        ["rete.spento"] = new[] { "Non collegato", "Not connected", "Sin conexión", "Non connecté", "Nicht verbunden", "Não conectado", "Нет соединения", "未连接" },
        ["rete.suggerimento"] = new[] { "F9: co-op", "F9: co-op", "F9: coop", "F9 : coop", "F9: Koop", "F9: co-op", "F9: кооператив", "F9：合作" },
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
