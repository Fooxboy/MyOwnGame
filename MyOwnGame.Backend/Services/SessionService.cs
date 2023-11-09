using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Managers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Parsers;

namespace MyOwnGame.Backend.Services;

public class SessionService
{
    private readonly IConfiguration _configuration;
    private readonly SiqPackageParser _siqPackageParser;
    private readonly SessionsManager _sessionsManager;
    private readonly UsersService _usersService;
    private readonly QuestionParser _questionParser;
    private readonly ILogger<SessionService> _logger;
    private readonly SessionCallbackService _callbackService;
    private readonly QuestionHandlerFactory _questionHandlerFactory;

    public SessionService(IConfiguration configuration, SiqPackageParser siqPackageParser, SessionsManager sessionsManager, 
        QuestionParser questionParser, UsersService usersService, ILogger<SessionService> logger, 
        SessionCallbackService callbackService, QuestionHandlerFactory questionHandlerFactory)
    {
        _configuration = configuration;
        _siqPackageParser = siqPackageParser;
        _sessionsManager = sessionsManager;
        _questionParser = questionParser;
        _usersService = usersService;
        _logger = logger;
        _callbackService = callbackService;
        _questionHandlerFactory = questionHandlerFactory;
    }

    public Session? CreateSession(string pathToPackage, long number)
    {
        if (_sessionsManager.GetSessionById(number) is not null)
        {
            _logger.LogError($"Сессия с ID {number} уже существует.");
            throw new ArgumentException("Такой number уже существует, лол");
        }
        
        var hash = HashHelper.ComputeHash(pathToPackage);

        var pathToUpackaged = Path.Combine(_configuration.GetValue<string>("packagesPath"), hash);
        
        if (!Directory.Exists(pathToUpackaged))
        {
            _logger.LogInformation($"Распаковка архива {Path.GetFileName(pathToPackage)}");
            _siqPackageParser.UnpackPackage(pathToPackage, hash);
        }

        File.Delete(pathToPackage);

        var pathToContent = Path.Combine(pathToUpackaged, "content.xml");

        _logger.LogInformation("Парсинг content.xml");
        
        var package = _siqPackageParser.ParsePackage(pathToContent);
        
        if (package is null)
        {
            _logger.LogError("После парсинга content.xml, package оказался null");
            
            Directory.Delete(pathToUpackaged);
            
            throw new ArgumentNullException("Package был null");
        }

        //Проверяем что нам не засунули в пакет лишнего
        var allMediaQuestions = package.Rounds.Round.SelectMany(r =>
            r.Themes.Theme.SelectMany(t => t.Questions!.Question
                .SelectMany(q => q.Scenario.Atom
                    .Where(a=> a.Type is "video" or "image" or "voice")
                    .Select(a => a.Text))))
            .Select(x=> x.Replace("@", string.Empty))
            .ToList();

        var files = Directory.GetFiles(pathToUpackaged).Select(f=> Path.GetFileName(f));

        foreach (var file in files)
        {
            if (file == "content.xml")
            {
                continue;
            }
            
            if (!allMediaQuestions.Exists(media=> media == file))
            {
                File.Delete(Path.Combine(pathToUpackaged, file));
            }
        }

        var session = _sessionsManager.CreateSession(package, number);

        session.SetPackageHash(hash);
        
        return session;
    }

    public Session? GetSession(long sessionId)
    {
        return  _sessionsManager.GetSessionById(sessionId);
    }
    
    public async Task<Session> ConnectToSession(long sessionId, long userId, string connectionId)
    {
        var user = await _usersService.GetUser(userId);

        if (user is null)
        {
            _logger.LogError($"Не найден пользователь с ID {userId}");
            
            throw new ArgumentException("Не найден пользователь");
        }
        
        var session = _sessionsManager.ConnectToSession(sessionId, user, connectionId);

        var player = _sessionsManager.GetPlayer(sessionId, userId);

        await _callbackService.PlayerConnectedToSession(sessionId, player);
        await _callbackService.SubscribeUserToEvents(sessionId, player.ConnectionId);
       
        return session;
    }

    public async Task<Player> DisconnectFromSession(string connectionId)
    {
        var removingPlayer = _sessionsManager.GetPlayer(connectionId);

        if (removingPlayer is null)
        {
            _logger.LogError($"Не найден игрок с connection id {connectionId}");

            throw new Exception($"Не найден игрок с connection id {connectionId}");
        }
        
        var session = _sessionsManager.DisconnectFromSession(connectionId);

        await _callbackService.PlayerDisconnectedFromSession(removingPlayer.SessionId, removingPlayer);

        _logger.LogInformation("Если пользователь был админом, тогда мы меняем админа");

        var newAdmin = TryChangeAdmin(removingPlayer.SessionId);

        if (newAdmin is not null)
        {
            _logger.LogInformation("Меняем админа");
            await _callbackService.AdminChanged(removingPlayer.SessionId, newAdmin);
        }

        if (session.Players.Count(p => !p.IsDisconnected) == 0)
        {
            _sessionsManager.CloseSession(removingPlayer.SessionId);
        }

        return removingPlayer;
    }

    public async Task<Player> PlayerNetworkDisconnected(string connectionId)
    {
        var disconnectedPlayer = _sessionsManager.GetPlayer(connectionId);

        if (disconnectedPlayer is null)
        {
            _logger.LogError($"Не найден пользотваель с id подключения '{connectionId}'");

            throw new Exception($"Не найден пользотваель с id подключения '{connectionId}'");
        }
        
        disconnectedPlayer.Disconnect();

        await _callbackService.PlayerOffline(disconnectedPlayer.SessionId, disconnectedPlayer);
        
        var questionPlayer = await CheckSelectQuestionPlayer(connectionId, disconnectedPlayer);

        if (questionPlayer is not null)
        {
            await _callbackService.ChangeSelectQuestionPlayer(disconnectedPlayer.SessionId, questionPlayer);
        }

        var session = _sessionsManager.GetSessionById(disconnectedPlayer.SessionId);
        
        if (session.Players.Count(p => !p.IsDisconnected) == 0)
        {
            _sessionsManager.CloseSession(disconnectedPlayer.SessionId);
        }
        
        return disconnectedPlayer;
    }

    public async Task<Player?> CheckSelectQuestionPlayer(string connectionId, Player disconnectedPlayer)
    {
        var session = _sessionsManager.GetSessionByConnection(connectionId);

        if (session is null)
        {
            _logger.LogError($"Не найдена сессия по ид подключения игрока '{connectionId}'");

            throw new Exception($"Не найдена сессия по ид подключения игрока '{connectionId}'");
        }

        if (session.SelectQuestionPlayer is null || session.SelectQuestionPlayer.Id != disconnectedPlayer.Id)
        {
            return null;
        }

        var selectQuestionPlayer = _sessionsManager.GetPlayerForSelectQuestion(session);

        if (selectQuestionPlayer is null)
        {
            throw new Exception("Не найден игрок для выбора вопроса");
        }
        
        session.SetSelectQuestionPlayer(selectQuestionPlayer);
        
        return selectQuestionPlayer;
    }
    
    public async Task ChangeRound(int roundPosition, string connectionId)
    {
        var player = _sessionsManager.GetPlayer(connectionId);

        if (player is null)
        {
            _logger.LogError($"Не найден пользователь в сессии с ID подключения '{connectionId}'");
            throw new Exception("Не найден пользователь");
        }

        if (!player.IsAdmin)
        {
            _logger.LogError("Пользователь не является админом");
            throw new Exception("Пользователь не админ епта");
        }

        var roundInfo = _sessionsManager.ChangeRound(roundPosition, player.SessionId);
        
        if (roundInfo.RoundInfo is null)
        {
            _logger.LogError("Ошибка при смене раунда");
            throw new Exception("Ошибка при смене раунда");
        }

        var session = _sessionsManager.GetSessionById(player.SessionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия");
        }
        
        session.ChangeStateToTable();
        
        await _callbackService.RoundChanged(player.SessionId, roundInfo.RoundInfo);

        await _callbackService.ChangeSelectQuestionPlayer(player.SessionId, roundInfo.QuestionPlayer);
    }

    public long Pause(string connectionId)
    {
        var player = _sessionsManager.GetPlayer(connectionId);

        if (player is null)
        {
            throw new Exception("Не найден пользователь");
        }

        if (!player.IsAdmin)
        {
            throw new Exception("У пользователя нет прав для установки паузы");
        }

        return player.SessionId;
    }

    public long Resume(string connectionId)
    {
        var player = _sessionsManager.GetPlayer(connectionId);

        if (player is null)
        {
            throw new Exception("Не найден пользователь");
        }

        if (!player.IsAdmin)
        {
            throw new Exception("У пользователя нет прав для установки паузы");
        }

        return player.SessionId;
    }

    public async Task SelectQuestion(int themeNumber, int priceNumber, string connectionId)
    {
        var session = _sessionsManager.GetSessionByConnection(connectionId);

        //Всякая валидация
        if (session is null)
        {
            _logger.LogError("Не найдена сессия, в которой игрок учавствует");

            throw new Exception("Не найдена сессия");
        }

        if (session.SelectQuestionPlayer is null)
        {
            throw new Exception("Не найден игрок, который выбирает вопрос");
        }

        if (session.SelectQuestionPlayer.ConnectionId != connectionId)
        {
            _logger.LogError("Пользователь не может выбирать вопрос");
            throw new Exception("Пользователь не может выбирать вопрос");
        }
        
        var admin = session.Players.FirstOrDefault(p => p.IsAdmin);

        if (admin is null)
        {
            _logger.LogError("Не найден админ в сесси");

            throw new Exception("Не найден connection ID админа");
        }
        
        var currentPlayer = session.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

        if (currentPlayer is null)
        {
            throw new Exception("Не найден текущий пользователь што");
        }
        
        var currentRound = session.Package.Rounds.Round[session.CurrentRound!.Number];
        var question = currentRound.Themes!.Theme[themeNumber]!.Questions!.Question[priceNumber];

        
        //Размазал парсинг вопроса блять. Это плохо надо это как то засунуть в парсер.
        var questionInfo = _questionParser.Parse(question);
        questionInfo.ThemeNumber = themeNumber;
        questionInfo.PriceNumber = priceNumber;

        if (session.CurrentRound.IsFinal)
        {
            questionInfo.QuestionPackInfo = new QuestionPackInfo() { Type = QuestionPackType.Final };
        }
        
        var handler = _questionHandlerFactory.GetHandler(questionInfo);

        if (handler is null)
        {
            throw new Exception($"Не найден хендлер типа '{questionInfo.QuestionPackInfo.Type}'");
        }

        await handler.HandleSelectQuestion(session, currentPlayer, admin, questionInfo);
    }
    
    public async Task GiveAnswer(DateTime time, string connectionId)
    {
        var sessionInfo = _sessionsManager.GetSessionInfoByConnection(connectionId);

        var session = sessionInfo.Item1;

        if (session is null)
        {
            throw new Exception("Не найдена сессия для этого игрока");
        }

        var player = session.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
        
        if (player is null)
        {
            throw new Exception("Не найден игрок лол");
        }

        if (session.State != SessionState.Question || session.ReadyToAnswerTime > DateTime.UtcNow || session.RespondingPlayer is not null)
        {
            await _callbackService.PlayerTriedAnswer(player.SessionId, player);
            return;
        }

        session.ChangeRespondingPlayer(player);
        
        session.ChangeStateToAnswer();

        await _callbackService.PlayerAnswer(player.SessionId, player);
    }

    public async Task AcceptAnswer(string connectionId)
    {
        var answerData = ValidateAnswerData(connectionId);
        var player = answerData.Player;
        var session = answerData.Session;
        
        var handler = _questionHandlerFactory.GetHandler(answerData.QuestionInfo);

        if (handler is null)
        {
            throw new Exception($"Не найден хендлер типа '{answerData.QuestionInfo.QuestionPackInfo.Type}'");
        }
        handler.SetCurrentSessionService(this);
        
        await handler.HandleAcceptQuestion(session, player);
    }

    public async Task RejectAnswer(string connectionId)
    {
        var answerData = ValidateAnswerData(connectionId);
        var player = answerData.Player;
        var session = answerData.Session;

        var handler = _questionHandlerFactory.GetHandler(answerData.QuestionInfo);

        if (handler is null)
        {
            throw new Exception($"Не найден хендлер типа '{answerData.QuestionInfo.QuestionPackInfo.Type}'");
        }
        handler.SetCurrentSessionService(this);

        await handler.HandleRejectAnswer(session, player);
    }

    public async Task SkipQuestion(string connectionId)
    {
        var sessionInfo = _sessionsManager.GetSessionInfoByConnection(connectionId); 
        
        var session = sessionInfo.Item1;

        if (session is null)
        {
            throw new Exception("Сессия не найдена");
        }
        
        if (!session.Players.Exists(x => x.IsAdmin && x.ConnectionId == connectionId))
        {
            throw new Exception("Пользователь не является админом лол");
        }
        
        session.ChangeStateToTable();

        await _callbackService.SkipQuestion(sessionInfo.Item2.Value, session.CurrentQuestion.Answer);
    }

    public Player? TryChangeAdmin(long sessionId)
    {
        var session = _sessionsManager.GetSessionById(sessionId);

        if (session is null)
        {
            throw new Exception("Сессия для поиска нового админа не найдена");
        }

        var hasAdmin = session.Players.Exists(p => p.IsAdmin);

        if (hasAdmin)
        {
            return null;
        }

        var newAdmin = session.Players.OrderBy(p => p.Score).FirstOrDefault();

        newAdmin?.SetAsAdmin();

        return newAdmin;
    }

    public async Task SetPlayerScore(int playerId, int score, string connectionId)
    {
        var admin = _sessionsManager.GetPlayer(connectionId);

        if (admin is null)
        {
            throw new Exception("Не найден игрок по id подключения");
        }

        if (!admin.IsAdmin)
        {
            throw new Exception($"Пользователь '{admin.Name} ({admin.Id})' не является админом сессии");
        }


        var user = _sessionsManager.GetPlayer(admin.SessionId, playerId);

        if (user is null)
        {
            throw new Exception(
                $"Неверный id пользователя '{playerId}' или пользователь не находится в одной сессии с админом");
        }
        
        user.SetScore(score);

        await _callbackService.ScoreChanged(user.SessionId, user, user.Score);
    }

    public async Task SetAdmin(int playerId, string connectionId)
    {
        var admin = _sessionsManager.GetPlayer(connectionId);

        if (admin is null)
        {
            throw new Exception("Не найден игрок по id подключения");
        }

        if (!admin.IsAdmin)
        {
            throw new Exception($"Пользователь '{admin.Name} ({admin.Id})' не является админом сессии");
        }

        var player = _sessionsManager.GetPlayer(admin.SessionId, playerId);
        
        if (player is null)
        {
            throw new Exception(
                $"Неверный id пользователя '{playerId}' или пользователь не находится в одной сессии с админом");
        }
        
        admin.RemoveAdmin();
        player.SetAsAdmin();

        await _callbackService.AdminChanged(player.SessionId, player);
    }

    public async Task SetSelectQuestionPlayer(int playerId, string connectionId)
    {
        var admin = _sessionsManager.GetPlayer(connectionId);

        if (admin is null)
        {
            throw new Exception("Не найден игрок по id подключения");
        }

        if (!admin.IsAdmin)
        {
            throw new Exception($"Пользователь '{admin.Name} ({admin.Id})' не является админом сессии");
        }

        var player = _sessionsManager.GetPlayer(admin.SessionId, playerId);
        
        if (player is null)
        {
            throw new Exception(
                $"Неверный id пользователя '{playerId}' или пользователь не находится в одной сессии с админом");
        }

        var session = _sessionsManager.GetSessionById(admin.SessionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия в которой находится админ");
        }
        
        session.SetSelectQuestionPlayer(player);

        await _callbackService.ChangeSelectQuestionPlayer(player.SessionId, player);
    }

    public async Task RemoveFinalTheme(int position, string connectionId)
    {
        var player = _sessionsManager.GetPlayer(connectionId);

        if (player is null)
        {
            throw new Exception($"Не найден игрок с id подключения '{connectionId}'");
        }

        var session = _sessionsManager.GetSessionById(player.SessionId);

        if (session is null)
        {
            throw new Exception($"Не найдена сессия с id '{player.SessionId}'");
        }

        if (session.SelectQuestionPlayer is null)
        {
            throw new Exception("Нет игрока, который должен выбирать вопрос.");
        }
        
        if (session.SelectQuestionPlayer.Id != player.Id)
        {
            throw new Exception($"Игрок с id '{player.Id}' не может выбирать вопрос");
        }
        
        session.RemoveTheme(position);

        var onlinePlayers = session.Players.Where(p => !p.IsAdmin && !p.IsDisconnected).ToList();

        var indexPlayer = onlinePlayers.IndexOf(player);
        
        if (indexPlayer == -1)
        {
            throw new Exception("ВСЕ СЛОМАЛОСЬ, Я ОБОСРАЛСЯ С ИНДЕКСОМ НУЖНА ПОПРАВИТЬ!!!!");
        }
        
        indexPlayer++;

        if (onlinePlayers.Count == indexPlayer)
        {
            indexPlayer = 0;
        }

        var nextPlayer = onlinePlayers[indexPlayer];

        session.SetSelectQuestionPlayer(nextPlayer);
      
        await _callbackService.FinalThemeRemoved(player.SessionId, session.CurrentRound.Themes);
        await _callbackService.ChangeSelectQuestionPlayer(player.SessionId, nextPlayer);
        
        if (session.CurrentRound.Themes.Count == 1)
        {
            var currentRound = session.Package.Rounds.Round[session.CurrentRound.Number];

            var question =
                currentRound.Themes.Theme.FirstOrDefault(x => session.CurrentRound.Themes.FirstOrDefault().Name == x.Name).Questions.Question.FirstOrDefault();

            var admin = session.Players.FirstOrDefault(x => x.IsAdmin);

            var questionInfo = _questionParser.Parse(question);
            questionInfo.QuestionPackInfo = new QuestionPackInfo() { Type = QuestionPackType.Final };
            
            session.SelectCurrentQuestion(questionInfo);
            
            await _callbackService.QuestionSelected(player.SessionId, questionInfo.Questions, questionInfo.QuestionPackInfo, 0, 0, 0);
            await _callbackService.QuestionSelectedAdmin(admin.ConnectionId, questionInfo.Answer);
            await _callbackService.PlayerCanAnswer(admin.SessionId);
        }
    }

    public async Task SendFinalAnswer(string message, int price, string connectionId)
    {
        var session = _sessionsManager.GetSessionByConnection(connectionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия :(");
        }

        var player = _sessionsManager.GetPlayer(connectionId);
        
        if (player is null)
        {
            throw new Exception("Не найден игрок, который отправил ответ");
        }
        
        session.AddFinalAnswer(player, message, price);

        await _callbackService.FinalQuestionResponsed(player.SessionId, player);

        if (session.Players.Count(x=> x is { IsAdmin: false, IsDisconnected: false }) >= session.FinalAnswers.Count)
        {
            var firstPlayer = session.FinalAnswers.FirstOrDefault().Player;
            
            session.ChangeRespondingPlayer(firstPlayer);

            await ShowFinalAnswer(firstPlayer.Id, firstPlayer.SessionId);
        }
    }

    public async Task ShowFinalAnswer(long playerId, long sessionId)
    {
        var player = _sessionsManager.GetPlayer(sessionId, playerId);

        if (player is null)
        {
            throw new Exception("Не найден игрок, которому надо показать финальный ответ");
        }

        var session = _sessionsManager.GetSessionById(player.SessionId);
        
        if (session is null)
        {
            throw new Exception("Не найдена сессия у игрока, вопрос котого надо показать");
        }
        
        session.ChangeStateToAnswer();

        var answer = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id);

        await _callbackService.UserFinalAnswer(player.SessionId, answer);
    }

    public async Task SetQuestionPrice(int price, string userConnectionId)
    {
        var player = _sessionsManager.GetPlayer(userConnectionId);

        if (player is null)
        {
            throw new Exception("Не найден игрок, который пытается установить цену");
        }

        var session = _sessionsManager.GetSessionById(player.SessionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия в которой находится игрок");
        }
        
        var handler = _questionHandlerFactory.GetHandler(session.CurrentQuestion);

        if (handler is null)
        {
            throw new Exception($"Не найден хендлер типа '{session.CurrentQuestion.QuestionPackInfo!.Type}'");
        }

        await handler.HandleSetQuestionPrice(session, player, price);
    }

    public async Task ForwardQuestion(string connectionId, int playerId)
    {
        var player = _sessionsManager.GetPlayer(connectionId);

        if (player is null)
        {
            throw new Exception("Бляяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяя");
        }

        var session = _sessionsManager.GetSessionById(player.SessionId);

        if (session is null)
        {
            throw new Exception("Сессия не найдена((((");
        }

        var forwardPlayer = _sessionsManager.GetPlayer(player.SessionId, playerId);

        if (forwardPlayer is null)
        {
            throw new Exception("Не найден игрок, которому передают вопрос");
        }

        session.ChangeStateToAnswer();
        
        session.ChangeRespondingPlayer(forwardPlayer);

        var handler = _questionHandlerFactory.GetHandler(session.CurrentQuestion);

        if (handler is null)
        {
            throw new Exception($"Не найден хендлер типа '{session.CurrentQuestion.QuestionPackInfo!.Type}'");
        }

        await handler.HandleForwardQuestion(session, forwardPlayer);
    }

    private (Player Player, QuestionInfo QuestionInfo, Session Session) ValidateAnswerData(string connectionId)
    {
        var sessionInfo = _sessionsManager.GetSessionInfoByConnection(connectionId);

        var session = sessionInfo.Item1;
        
        if (session is null)
        {
            throw new Exception("Сессия не найдена");
        }

        if (!session.Players.Exists(x => x.IsAdmin && x.ConnectionId == connectionId))
        {
            throw new Exception("Пользователь не является админом лол");
        }

        if (session.State != SessionState.Answer)
        {
            throw new Exception("Состояние сессии не позволяет принимать ответы");
        }

        var player = session.RespondingPlayer;

        if (player is null)
        {
            throw new Exception("Не найден игрок лол");
        }

        if (session.CurrentRound.IsFinal)
        {
            return (player, session.CurrentQuestion, session);
        }
        
        var selectedQuestion = session.CurrentQuestion;

        if (selectedQuestion is null)
        {
            throw new Exception("Нет ответа лол");
        }

        return (player, selectedQuestion, session);
    }
}