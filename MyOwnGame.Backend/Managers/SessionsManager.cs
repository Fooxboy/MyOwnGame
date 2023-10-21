using MyOwnGame.Backend.Database;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Managers;

public class SessionsManager
{
    private readonly Dictionary<long, Session> _sessions = new();
    private readonly Dictionary<string, long> _usersConnections = new();

    private readonly ILogger<SessionsManager> _logger;

    public SessionsManager(ILogger<SessionsManager> logger)
    {
        _logger = logger;
    }

    public Session CreateSession(Package package, long number)
    {
        var session = new Session(package);
        
        _sessions.Add(number, session);
        
        
        _logger.LogInformation($"Добавлена новая сессия с номером '{number}'");
        return session;
    }

    public Session? GetSessionById(long id)
    {
        return !_sessions.TryGetValue(id, out var session) ? null : session;
    }
    
    public Session? GetSessionByConnection(string connectionId)
    {
        return !_usersConnections.TryGetValue(connectionId, out var sessionId) ? null : GetSessionById(sessionId);
    }
    
    public (Session?, long?) GetSessionInfoByConnection(string connectionId)
    {
        if (_usersConnections.TryGetValue(connectionId, out var sessionId))
        {
            var session = GetSessionById(sessionId);

            return (session, sessionId);
        }
        
        _logger.LogError($"Не найдена сессия по connectionId: {connectionId}");
        return (null, null);
    }

    public Session ConnectToSession(long sessionId, User user, string connectionId)
    {
        var session = GetSessionById(sessionId);

        if (session is null)
        {
            _logger.LogError($"Не найдена сессия с ID '{sessionId}'");
            
            throw new Exception("Не найдена сессия");
        }

        var reconnectPlayer = GetPlayer(sessionId, user.Id);
        
        if (reconnectPlayer is not null)
        {
            _logger.LogInformation("Пользотватель уже был в сессии. Возвращаем его информацию");
            
            reconnectPlayer.Connect();
            reconnectPlayer.ConnectionId = connectionId;
            _usersConnections.Add(connectionId, sessionId);

            return session;
        }
        
        var isAdmin = session.AdminCanConnect;
        
        var player = Player.Create(user, isAdmin, sessionId);

        player.ConnectionId = connectionId;
        session.AddPlayer(player);

        _logger.LogInformation($"Добавлен новый пользователь с ID подключения '{connectionId}' в сессию '{sessionId}'");
        
        _usersConnections.Add(connectionId, sessionId);
        return session;
    }

    public void DisconnectFromSession(string playerConnectionId)
    {
        var session = GetSessionByConnection(playerConnectionId);

        if (session is null)
        {
            _logger.LogError($"Не найдена сессия для удаления игрока с id подключения '{playerConnectionId}'");
            throw new Exception($"Не найдена сессия для удаления игрока с id подключения '{playerConnectionId}'");
        }

        _usersConnections.Remove(playerConnectionId);
        
        session.RemovePlayer(playerConnectionId);
    }

    public Player? GetPlayer(long sessionId, long userId)
    {
        var session = GetSessionById(sessionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия");
        }

        return session.Players.FirstOrDefault(p => p.Id == userId);
    }

    public Player? GetAdmin(long sessionId)
    {
        var admin = GetSessionById(sessionId).Players.FirstOrDefault(x => x.IsAdmin);

        return admin;
    }
    
    public Player? GetPlayer(string connectionId)
    {
        var session = GetSessionByConnection(connectionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия");
        }

        return session.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
    }

    public (RoundInfo? RoundInfo, Player QuestionPlayer) ChangeRound(int roundPosition, long sessionId)
    {
        var session = GetSessionById(sessionId);

        if (session is null)
        {
            _logger.LogError($"Не найдена сессия с ID '{sessionId}'");

            throw new Exception("Не найдена сессия");
        }
        
        var player = GetPlayerForSelectQuestion(session);

        if (player is null)
        {
            _logger.LogError("Не найден игрок, который может выбирать вопрос");

            throw new Exception("Не найден игрок, который может выбирать вопрос");
        }
        
        session.SetSelectQuestionPlayer(player);

        session.ChangeRound(roundPosition);
       
        return (session.CurrentRound, player);
    }

    public Player? GetPlayerForSelectQuestion(Session session)=> 
        session.Players.OrderBy(p => p.Score).FirstOrDefault(p => !p.IsAdmin && !p.IsDisconnected);
    
}