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
            
            _logger.LogInformation($"Пользователь '{removingPlayer.Id}' отключился от сессии");

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
            
            await _sessionService.ChangeRound(roundNumber, Context.ConnectionId);
            
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
            await _sessionService.GetQuestionInfo(themeNumber, priceNumber, Context.ConnectionId);
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
            
            await _sessionService.GiveAnswer(time, Context.ConnectionId);
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
            await _sessionService.AcceptAnswer(Context.ConnectionId);
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

            await _sessionService.RejectAnswer(Context.ConnectionId);
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

            await _sessionService.SkipQuestion(Context.ConnectionId);
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

            await _sessionService.RemoveFinalTheme(number, Context.ConnectionId);
            
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
            
            //todo: пока нема
            
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
            
            //todo: пока нема
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

            await _sessionService.SetPlayerScore(playerId, score, Context.ConnectionId);
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

            await _sessionService.SetAdmin(playerId, Context.ConnectionId);
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

            await _sessionService.SetSelectQuestionPlayer(playerId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task SendFinalAnswer(string text, int price)
    {
        try
        {
            _logger.LogInformation($"Отправка финального ответа от пользователя  '{Context.ConnectionId}' '{text}'");

            await _sessionService.SendFinalAnswer(text, price, Context.ConnectionId);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task ShowFinalAnswer(int playerId)
    {
        try
        {
            _logger.LogInformation("Показ финального вопроса всем пользователям");

            await _sessionService.ShowFinalAnswer(playerId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task SetQuestionPrice(int price)
    {
        try
        {
            await _sessionService.SetQuestionPrice(price, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var disconnectedPlayer = await _sessionService.PlayerNetworkDisconnected(Context.ConnectionId);
            
            _logger.LogInformation($"Пользователь '{disconnectedPlayer.Id}' отключен по сетевой ошибке");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }
}