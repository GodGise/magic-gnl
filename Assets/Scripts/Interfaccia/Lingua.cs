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
