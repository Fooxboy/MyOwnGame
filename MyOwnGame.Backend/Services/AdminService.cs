using Microsoft.AspNetCore.SignalR;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Hubs;
using MyOwnGame.Backend.Managers;

namespace MyOwnGame.Backend.Services;

public class AdminService
{
    private readonly SessionService _sessionService;

    private readonly SessionsManager _sessionsManager;

    private readonly SessionCallbackService _sessionCallbackService;
    

    public AdminService(SessionService sessionService, SessionsManager sessionsManager, SessionCallbackService sessionCallbackService)
    {
        _sessionService = sessionService;
        _sessionsManager = sessionsManager;
        _sessionCallbackService = sessionCallbackService;
    }

    public Dictionary<long, Session> GetSessions()
    {
        var sessions = _sessionsManager.GetSessions();

        return sessions;
    }

    public async Task<bool> CloseSession(long sessionId)
    {
        await _sessionCallbackService.SessionClosed(sessionId);
        _sessionsManager.CloseSession(sessionId);

        return true;
    }

    public List<Player> GetPlayersInSessions(long sessionId)
    {
        var session = _sessionsManager.GetSessionById(sessionId);

        return session.Players;
    }
    
    
}