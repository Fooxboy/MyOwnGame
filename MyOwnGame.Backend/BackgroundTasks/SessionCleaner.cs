using MyOwnGame.Backend.Managers;

namespace MyOwnGame.Backend.BackgroundTasks;

public class SessionCleaner : IBackgroundTask
{
    private readonly SessionsManager _sessionsManager;
    
    private readonly ILogger<SessionCleaner> _logger;

    public SessionCleaner(SessionsManager sessionsManager, ILogger<SessionCleaner> logger)
    {
        _sessionsManager = sessionsManager;
        _logger = logger;
    }

    public int Timeout => 3600;

    public Task Invoke()
    {
        _logger.LogInformation("Поиск сессий, где больше нет игроков");
        
        var sessions = _sessionsManager
            .GetSessions()
            .Where(s=> s.Value.Players.Count(p=> !p.IsDisconnected) == 0)
            .ToList();

        foreach (var session in sessions)
        {
            _logger.LogInformation($"Уничтожена сессия {session.Key}");
            _sessionsManager.CloseSession(session.Key);
        }

        return Task.CompletedTask;
    }
}