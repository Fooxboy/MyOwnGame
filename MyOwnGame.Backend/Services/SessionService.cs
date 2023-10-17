using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Managers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Answers;
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

    public SessionService(IConfiguration configuration, SiqPackageParser siqPackageParser, SessionsManager sessionsManager, QuestionParser questionParser, UsersService usersService, ILogger<SessionService> logger)
    {
        _configuration = configuration;
        _siqPackageParser = siqPackageParser;
        _sessionsManager = sessionsManager;
        _questionParser = questionParser;
        _usersService = usersService;
        _logger = logger;
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
        
        if (Directory.Exists(pathToUpackaged))
        {
            File.Delete(pathToPackage);
        }
        else
        {
            _logger.LogInformation($"Распаковка архива {Path.GetFileName(pathToPackage)}");
            _siqPackageParser.UnpackPackage(pathToPackage, hash);
        }

        var pathToContent = Path.Combine(pathToUpackaged, "content.xml");

        _logger.LogInformation("Парсинг content.xml");
        
        var package = _siqPackageParser.ParsePackage(pathToContent);

        if (package is null)
        {
            _logger.LogError("После парсинга content.xml, package оказался null");
            
            Directory.Delete(pathToUpackaged);
            
            throw new ArgumentNullException("Package был null");
        }

        var session = _sessionsManager.CreateSession(package, number);

        session.SetPackageHash(hash);

        return session;
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
       
        return session;
    }

    public Player? GetPlayer(long sessionId, long userId)
    {
        return _sessionsManager.GetPlayer(sessionId, userId);
    }
    
    public Player? GetPlayer(string connectionId)
    {
        return _sessionsManager.GetPlayer(connectionId);
    }

    public (RoundInfo, long, Player) ChangeRound(int roundPosition, string connectionId)
    {
        var player = GetPlayer(connectionId);

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

        return (roundInfo.RoundInfo, player.SessionId, roundInfo.QuestionPlayer);
    }

    public long Pause(string connectionId)
    {
        var player = GetPlayer(connectionId);

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
        var player = GetPlayer(connectionId);

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

    public Session GetSession(long id)
    {
        var session = _sessionsManager.GetSessionById(id);

        if (session is null)
        {
            throw new Exception("Не найдена сессия");
        }
        
        return session;
    }

    public (QuestionInfo, long, string) GetQuestionInfo(int themeNumber, int priceNumber, string connectionId)
    {
        var session = _sessionsManager.GetSessionByConnection(connectionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия");
        }

        if (session.CurrentRound is null)
        {
            throw new Exception("Игра ещё не началась");
        }
        
        var currentRound = session.Package.Rounds.Round[session.CurrentRound.Number];

        var question = currentRound.Themes.Theme[themeNumber].Questions.Question[priceNumber];

        var questionInfo = _questionParser.Parse(question);

        questionInfo.Question.PriceNumber = priceNumber;
        questionInfo.Question.ThemeNumber = themeNumber;

        var adminConnectionId = session.Players.FirstOrDefault(p => p.IsAdmin)?.ConnectionId;

        if (adminConnectionId is null)
        {
            throw new Exception("Не найден connection ID админа");
        }

        var currentPlayer = session.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

        if (currentPlayer is null)
        {
            throw new Exception("Не найден текущий пользователь што");
        }

        session.ChangeStateToQuestion();

        session.SelectCurrentQuestion(questionInfo);

        return (questionInfo, currentPlayer.SessionId, adminConnectionId);
    }

    public (bool IsAnswer, Player? Player, long? SessionId) GiveAnswer(DateTime time, string connectionId)
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

        if (session.State != SessionState.Question)
        {
            return (false, player, sessionInfo.Item2);
        }

        if (session.ReadyToAnswerTime > DateTime.UtcNow)
        {
            return  (false, player, sessionInfo.Item2);
        }
      
        if (session.RespondingPlayer is not null)
        {
            return  (false, player, sessionInfo.Item2);
        }
       
        session.ChangeRespondingPlayer(player);
        
        session.ChangeStateToAnswer();

        return (true, player, sessionInfo.Item2);
    }

    public (Player Player, int NewScore, AnswerBase Answer) AcceptAnswer(string connectionId)
    {
        var answerData = ValidateAnswerData(connectionId);
        var player = answerData.Player;
        var selectedQuestion = answerData.QuestionInfo;
        var session = answerData.Session;
        
        player.AddScore(selectedQuestion.Price);
        
        session.ChangeStateToTable();

        return (player, player.Score, selectedQuestion.Answer);
    }

    public (Player Player, int NewScore) RejectAnswer(string connectionId)
    {
        var answerData = ValidateAnswerData(connectionId);
        var player = answerData.Player;
        var selectedQuestion = answerData.QuestionInfo;
        var session = answerData.Session;
        
        player.RemoveScore(selectedQuestion.Price);
        
        session.ChangeStateToAnswer();

        return (player, player.Score);

    }

    public (long? SessionId, AnswerBase Answer) SkipQuestion(string connectionId)
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

        return (sessionInfo.Item2, session.CurrentQuestion.Answer);
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
        
        var selectedQuestion = session.CurrentQuestion;

        if (selectedQuestion is null)
        {
            throw new Exception("Нет ответа лол");
        }

        return (player, selectedQuestion, session);
    }
}