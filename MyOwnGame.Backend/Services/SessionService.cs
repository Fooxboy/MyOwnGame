using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Managers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.Questions;
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

    public SessionService(IConfiguration configuration, SiqPackageParser siqPackageParser, SessionsManager sessionsManager, 
        QuestionParser questionParser, UsersService usersService, ILogger<SessionService> logger, SessionCallbackService callbackService)
    {
        _configuration = configuration;
        _siqPackageParser = siqPackageParser;
        _sessionsManager = sessionsManager;
        _questionParser = questionParser;
        _usersService = usersService;
        _logger = logger;
        _callbackService = callbackService;
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

        var player = GetPlayer(sessionId, userId);

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
        
        _sessionsManager.DisconnectFromSession(connectionId);

        await _callbackService.PlayerDisconnectedFromSession(removingPlayer.SessionId, removingPlayer);

        _logger.LogInformation("Если пользователь был админом, тогда мы меняем админа");

        var newAdmin = TryChangeAdmin(removingPlayer.SessionId);

        if (newAdmin is not null)
        {
            _logger.LogInformation("Меняем админа");
            await _callbackService.AdminChanged(removingPlayer.SessionId, newAdmin);
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

    public Player? GetPlayer(long sessionId, long userId)
    {
        return _sessionsManager.GetPlayer(sessionId, userId);
    }
    
    public Player? GetPlayer(string connectionId)
    {
        return _sessionsManager.GetPlayer(connectionId);
    }

    public async Task ChangeRound(int roundPosition, string connectionId)
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

        await _callbackService.RoundChanged(player.SessionId, roundInfo.RoundInfo);

        await _callbackService.ChangeSelectQuestionPlayer(player.SessionId, roundInfo.QuestionPlayer);
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

    public async Task GetQuestionInfo(int themeNumber, int priceNumber, string connectionId)
    {
        var session = _sessionsManager.GetSessionByConnection(connectionId);

        if (session is null)
        {
            _logger.LogError("Не найдена сессия, в которой игрок учавствует");

            throw new Exception("Не найдена сессия");
        }

        if (session.CurrentRound is null)
        {
            _logger.LogError("Игра ещё не началась");

            throw new Exception("Игра ещё не началась");
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

        var currentRound = session.Package.Rounds.Round[session.CurrentRound.Number];

        var question = currentRound.Themes.Theme[themeNumber].Questions.Question[priceNumber];

        var questionInfo = _questionParser.Parse(question);

        questionInfo.ThemeNumber = themeNumber;
        questionInfo.PriceNumber = priceNumber;

        var adminConnectionId = session.Players.FirstOrDefault(p => p.IsAdmin)?.ConnectionId;

        if (adminConnectionId is null)
        {
            _logger.LogError("Не найден админ в сесси");

            throw new Exception("Не найден connection ID админа");
        }

        var currentPlayer = session.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

        if (currentPlayer is null)
        {
            throw new Exception("Не найден текущий пользователь што");
        }

        session.SelectCurrentQuestion(questionInfo);

        if (questionInfo.QuestionPackInfo is not null && questionInfo.QuestionPackInfo.Type != QuestionPackType.Simple)
        {
            //Если это аукцион - возвращаем инфу о том что выбран вопрос, но без вопросов.
            if (questionInfo.QuestionPackInfo.Type == QuestionPackType.Auction)
            {
                await _callbackService.QuestionSelectedAdmin(adminConnectionId, questionInfo.Answer);

                await _callbackService.QuestionSelected(currentPlayer.SessionId, new List<QuestionBase>(),
                    questionInfo.QuestionPackInfo, -1, themeNumber, priceNumber);

                await _callbackService.NeedSetQuestionPrice(currentPlayer.SessionId, currentPlayer, 1, currentPlayer.Score, 1);

                return;
            }
        }
        
        //simple
        var seconds = session.ChangeStateToQuestion(questionInfo.Questions.Count);
        
        DelayTaskRunner.Run(seconds, () => _callbackService.PlayerCanAnswer(currentPlayer.SessionId));

        session.CurrentRound.Themes[themeNumber].Prices[priceNumber].IsAnswered = true;

        await _callbackService.QuestionSelected(currentPlayer.SessionId, questionInfo.Questions,
            questionInfo.QuestionPackInfo, seconds, themeNumber, priceNumber);

        await _callbackService.QuestionSelectedAdmin(adminConnectionId, questionInfo.Answer);
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
            await _callbackService.PlayerTryedAnswer(player.SessionId, player);
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
        var selectedQuestion = answerData.QuestionInfo;
        var session = answerData.Session;
        
        //Если это не финал, мы действуем по обычной логике
        if (!answerData.Session.CurrentRound.IsFinal)
        {
            if (answerData.QuestionInfo.QuestionPackInfo != null &&
                answerData.QuestionInfo.QuestionPackInfo.Type == QuestionPackType.Auction)
            {
                var price = session.AuctionPrices.FirstOrDefault(x => x.Player.Id == player.Id);
                
                player.AddScore(price.Price);
            }
            else
            {
                player.AddScore(selectedQuestion.Price);
            }
            
            session.ChangeStateToTable();
            session.SetSelectQuestionPlayer(answerData.Player);

            await _callbackService.AcceptAnswer(player.SessionId, player, player.Score, answerData.QuestionInfo.Answer);

            await _callbackService.ChangeSelectQuestionPlayer(player.SessionId, player);
        }
        else
        {
            //Если это финал - цену мы берем из финальных ответов и не показываем ответ.
            
            var price = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id).Price;
            player.AddScore(price);
            
            await _callbackService.ScoreChanged(player.SessionId, player, player.Score);
        }
    }

    public async Task RejectAnswer(string connectionId)
    {
        var answerData = ValidateAnswerData(connectionId);
        var player = answerData.Player;
        var selectedQuestion = answerData.QuestionInfo;
        var session = answerData.Session;

        if (!answerData.Session.CurrentRound.IsFinal)
        {

            if (answerData.QuestionInfo.QuestionPackInfo != null &&
                answerData.QuestionInfo.QuestionPackInfo.Type == QuestionPackType.Auction)
            {
                var price = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id).Price;
                player.RemoveScore(price);
            }
            else
            {
                player.RemoveScore(selectedQuestion.Price);
            }
            
            session.ChangeStateToAnswer();

            await _callbackService.RejectAnswer(player.SessionId, player, player.Score);
        }
        else
        {
            var price = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id).Price;
            
            player.RemoveScore(price);

            await _callbackService.ScoreChanged(player.SessionId, player, player.Score);
        }
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

        var indexPlayer = session.Players.IndexOf(player);
        
        if (indexPlayer == -1)
        {
            throw new Exception("ВСЕ СЛОМАЛОСЬ, Я ОБОСРАЛСЯ С ИНДЕКСОМ НУЖНА ПОПРАВИТЬ!!!!");
        }

        indexPlayer++;

        var newPlayer = session.Players[indexPlayer];

        if (newPlayer.IsAdmin)
        {
            indexPlayer++;
            newPlayer = session.Players[indexPlayer];
        }

        await _callbackService.FinalThemeRemoved(player.SessionId, session.CurrentRound.Themes);
        await _callbackService.ChangeSelectQuestionPlayer(player.SessionId, newPlayer);
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
    }

    public async Task ShowFinalAnswer(int playerId, string connectionId)
    {
        var adminUser = _sessionsManager.GetPlayer(connectionId);

        if (adminUser is null)
        {
            throw new Exception("Админ не найден");
        }

        if (!adminUser.IsAdmin)
        {
            throw new Exception("Ты не админ уйди отсюдава");
        }

        var player = _sessionsManager.GetPlayer(adminUser.SessionId, playerId);

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
        
        session.AddAuctionPrice(player, price);

        var playersWithoutInstallPrices = session.Players.Where(p => !session.AuctionPrices!.Exists(x => x.Player.Id == p.Id) && !p.IsAdmin);

        var nextPlayer = playersWithoutInstallPrices.FirstOrDefault();

        if (nextPlayer is null)
        {
            var questionPlayer = session.AuctionPrices!.MaxBy(price => price.Price).Player;
            
            session.SetSelectQuestionPlayer(questionPlayer);

            await _callbackService.ChangeSelectQuestionPlayer(questionPlayer.SessionId, questionPlayer);

            var questionInfo = session.CurrentQuestion;
            
            session.CurrentRound.Themes[questionInfo.ThemeNumber].Prices[questionInfo.PriceNumber].IsAnswered = true;
            
            await _callbackService.QuestionSelected(questionPlayer.SessionId, questionInfo.Questions,
                questionInfo.QuestionPackInfo, -1, questionInfo.ThemeNumber, questionInfo.PriceNumber);

            return;
        }

        await _callbackService.QuestionPriceInstalled(player.SessionId, player, price);

        await _callbackService.NeedSetQuestionPrice(nextPlayer.SessionId, nextPlayer, 0, nextPlayer.Score, 1);
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
            return (player, null, session);
        }
        
        var selectedQuestion = session.CurrentQuestion;

        if (selectedQuestion is null)
        {
            throw new Exception("Нет ответа лол");
        }

        return (player, selectedQuestion, session);
    }
}