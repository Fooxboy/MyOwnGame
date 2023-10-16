using Microsoft.AspNetCore.SignalR;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
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
            var session = await _sessionService.ConnectToSession(sessionId, userId, Context.ConnectionId);

            var player = _sessionService.GetPlayer(sessionId, userId);
            
            await Clients.Group(sessionId.ToString())
                .SendAsync(SessionEvents.PlayerConnectedToSession.ToString(), player);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());

            return SessionDto.Create(session);
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
            var round = _sessionService.ChangeRound(roundNumber, Context.ConnectionId);

            await Clients.Group(round.Item2.ToString()).SendAsync(SessionEvents.RoundChanged.ToString(), round.Item1);
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
            var questionInfoResponse = _sessionService.GetQuestionInfo(themeNumber, priceNumber, Context.ConnectionId);

            await Clients.Group(questionInfoResponse.Item2.ToString()).SendAsync(SessionEvents.QuestionSelected.ToString(),
                questionInfoResponse.Item1.Question);

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
            var acceptInfo = _sessionService.AcceptAnswer(Context.ConnectionId);

            await Clients.Group(acceptInfo.Player.SessionId.ToString()).SendAsync(
                SessionEvents.AcceptAnswer.ToString(), PlayerDto.Create(acceptInfo.Player), acceptInfo.NewScore,
                acceptInfo.Answer);

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

    public async Task Pause()
    {
        try
        {
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
}