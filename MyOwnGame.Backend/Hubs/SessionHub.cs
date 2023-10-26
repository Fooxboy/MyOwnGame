using Microsoft.AspNetCore.SignalR;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Dtos;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Hubs;

public class SessionHub : Hub
{
    private readonly ILogger<SessionHub> _logger;
    
    private readonly SessionService _sessionService;

    public SessionHub(ILogger<SessionHub> logger, SessionService sessionService)
    {
        _logger = logger;
        _sessionService = sessionService;
    }

    public async Task<SessionDto> ConnectToSession(long sessionId, long userId)
    {
        try
        {
            _logger.LogInformation($"Пользователь '{userId}' пытается подключиться к сессии '{sessionId}'");
            
            var session = await _sessionService.ConnectToSession(sessionId, userId, Context.ConnectionId);

            var player = _sessionService.GetPlayer(sessionId, userId);
            
            await Clients.Group(sessionId.ToString())
                .SendAsync(SessionEvents.PlayerConnectedToSession.ToString(), PlayerDto.Create(player));
            
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());
            
            _logger.LogInformation($"Пользователь '{userId}' подключился к сессии '{sessionId}'");

            return SessionDto.Create(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task DisconnectFromSession()
    {
        try
        {
            _logger.LogInformation("Пользотваель сам отключается от сессии");
            var removingPlayer = await _sessionService.DisconnectFromSession(Context.ConnectionId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, removingPlayer.SessionId.ToString());

            await Clients.Group(removingPlayer.SessionId.ToString())
                .SendAsync(SessionEvents.PlayerDisconnectedFromSession.ToString(), removingPlayer);
            
            _logger.LogInformation($"Пользователь '{removingPlayer.Id}' отключился от сессии");
            
            //проверяем шо он был не админом и все такое


            _logger.LogInformation("Если пользователь был админом, тогда мы меняем админа");

            var newAdmin = _sessionService.TryChangeAdmin(removingPlayer.SessionId);

            if (newAdmin != null)
            {
                await Clients.Group(removingPlayer.SessionId.ToString())
                    .SendAsync(SessionEvents.AdminChanged.ToString(), PlayerDto.Create(newAdmin));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task ChangeRound(int roundNumber)
    {
        try
        {
            _logger.LogInformation($"Изменение раунда игры на '{roundNumber}'");
            
            var round = _sessionService.ChangeRound(roundNumber, Context.ConnectionId);

            await Clients.Group(round.Item2.ToString()).SendAsync(SessionEvents.RoundChanged.ToString(), round.Item1);
            
            await Clients.Group(round.Item2.ToString()).SendAsync(SessionEvents.ChangeSelectQuestionPlayer.ToString(), PlayerDto.Create(round.Item3));
            
            _logger.LogInformation($"Раунд игры изменен на '{roundNumber}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }
    
    public async Task SelectQuestion(int themeNumber, int priceNumber)
    {
        try
        {
            _logger.LogInformation($"Выбор вопроса с темой: {themeNumber}, ценой: {priceNumber}");
            var questionInfoResponse = _sessionService.GetQuestionInfo(themeNumber, priceNumber, Context.ConnectionId);

            await Clients.Group(questionInfoResponse.Item2.ToString()).SendAsync(SessionEvents.QuestionSelected.ToString(),
                questionInfoResponse.Item1.Questions, new QuestionSelectedPosition() {QuestionNumber = priceNumber, ThemeNumber = themeNumber});

            await Clients.Client(questionInfoResponse.Item3).SendAsync(SessionEvents.QuestionSelectedAdmin.ToString(),
                questionInfoResponse.Item1.Answer);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task ReadyToAnswer(DateTime time)
    {
        try
        {
            _logger.LogInformation($"Попытка ответить в {time}");
            
            var isSuccessResult = _sessionService.GiveAnswer(time, Context.ConnectionId);

            if (isSuccessResult.IsAnswer)
            {
                await Clients.Group(isSuccessResult.SessionId.ToString()).SendAsync(SessionEvents.PlayerAnswer.ToString(),
                    PlayerDto.Create(isSuccessResult.Player));
            }
            else
            {
                await Clients.Group(isSuccessResult.SessionId.ToString()).SendAsync(SessionEvents.PlayerTryedAnswer.ToString(),
                    PlayerDto.Create(isSuccessResult.Player));
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task AcceptAnswer()
    {
        try
        {
            _logger.LogInformation("Принятие ответа");
            var acceptInfo = _sessionService.AcceptAnswer(Context.ConnectionId);

            await Clients.Group(acceptInfo.Player.SessionId.ToString()).SendAsync(
                SessionEvents.AcceptAnswer.ToString(), PlayerDto.Create(acceptInfo.Player), acceptInfo.NewScore,
                acceptInfo.Answer);
            
            await Clients.Group(acceptInfo.Player.SessionId.ToString()).SendAsync(
                SessionEvents.ChangeSelectQuestionPlayer.ToString(), PlayerDto.Create(acceptInfo.Player));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task RejectAnswer()
    {
        try
        {
            _logger.LogInformation("Не принятие ответа");

            var rejectInfo = _sessionService.RejectAnswer(Context.ConnectionId);
            
            await Clients.Group(rejectInfo.Player.SessionId.ToString()).SendAsync(
                SessionEvents.RejectAnswer.ToString(), PlayerDto.Create(rejectInfo.Player), rejectInfo.NewScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task SkipQuestion()
    {
        try
        {
            _logger.LogInformation("Пропуск вопроса");

            var skipInfo = _sessionService.SkipQuestion(Context.ConnectionId);
            
            await Clients.Group(skipInfo.SessionId.ToString()).SendAsync(
                SessionEvents.SkipQuestion.ToString(), skipInfo.Answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task RemoveFinalTheme(int number)
    {
        try
        {
            _logger.LogInformation("Удаление финальной темы");

            var removeInfo = _sessionService.RemoveFinalTheme(number, Context.ConnectionId);
            
            await Clients.Group(removeInfo.newSelectQuestionPlayer.SessionId.ToString()).SendAsync(
                SessionEvents.FinalThemeRemoved.ToString(), removeInfo.themes);
            
            await Clients.Group(removeInfo.newSelectQuestionPlayer.SessionId.ToString()).SendAsync(
                SessionEvents.ChangeSelectQuestionPlayer.ToString(), PlayerDto.Create(removeInfo.newSelectQuestionPlayer));
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }
    
    public async Task Pause()
    {
        try
        {
            _logger.LogInformation("Пауза игры");

            var sessionId = _sessionService.Pause(Context.ConnectionId);
            var session = _sessionService.GetSession(sessionId);
            
            await Clients.Group(sessionId.ToString()).SendAsync(SessionEvents.GamePaused.ToString(), SessionDto.Create(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task Resume()
    {
        try
        {
            _logger.LogInformation("Продолжение игры");

            var sessionId = _sessionService.Resume(Context.ConnectionId);
            var session = _sessionService.GetSession(sessionId);
            
            await Clients.Group(sessionId.ToString()).SendAsync(SessionEvents.GameResumed.ToString(), SessionDto.Create(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task SetScore(int playerId, int score)
    {
        try
        {
            _logger.LogInformation($"Попытка установки пользователю с ID '{playerId}' вот столько очков - '{score}'");

            var newUser = _sessionService.SetPlayerScore(playerId, score, Context.ConnectionId);
            
            await Clients.Group(newUser.SessionId.ToString()).SendAsync(SessionEvents.ScoreChanged.ToString(), PlayerDto.Create(newUser), newUser.Score);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task SetAdmin(int playerId)
    {
        try
        {
            _logger.LogInformation($"Изменения админа сесси на '{playerId}'");

            var newUser = _sessionService.SetAdmin(playerId, Context.ConnectionId);
            
            await Clients.Group(newUser.SessionId.ToString()).SendAsync(SessionEvents.AdminChanged.ToString(), PlayerDto.Create(newUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task ChangeSelectQuestionPlayer(int playerId)
    {
        try
        {
            _logger.LogInformation("Изменение игрока который может выбирать вопрос");

            var newUser = _sessionService.SetSelectQuestionPlayer(playerId, Context.ConnectionId);
            
            await Clients.Group(newUser.SessionId.ToString()).SendAsync(SessionEvents.ChangeSelectQuestionPlayer.ToString(), PlayerDto.Create(newUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var disconnectedPlayer = await _sessionService.PlayerNetworkDisconnected(Context.ConnectionId);
            
            await Clients.Group(disconnectedPlayer.SessionId.ToString())
                .SendAsync(SessionEvents.PlayerOffline.ToString(), PlayerDto.Create(disconnectedPlayer));

            var questionPlayer = await _sessionService.CheckSelectQuestionPlayer(Context.ConnectionId, disconnectedPlayer);

            if (questionPlayer is not null)
            {
                await Clients.Group(questionPlayer.SessionId.ToString()).SendAsync(SessionEvents.ChangeSelectQuestionPlayer.ToString(), PlayerDto.Create(questionPlayer));
            }
            
            _logger.LogInformation($"Пользователь '{disconnectedPlayer.Id}' отключен по сетевой ошибке");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }
}