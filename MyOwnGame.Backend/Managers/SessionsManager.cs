using MyOwnGame.Backend.Database;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Managers;

public class SessionsManager
{
    private readonly Dictionary<long, Session> _sessions = new();
    private readonly Dictionary<string, long> _usersConnections = new();

    public Session CreateSession(Package package, long number)
    {
        var session = new Session(package);
        
        _sessions.Add(number, session);

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

        return (null, null);
    }

    public Session ConnectToSession(long sessionId, User user, string connectionId)
    {
        var session = GetSessionById(sessionId);

        if (session is null)
        {
            throw new Exception("Не найдена сессия");
        }

        var reconnectPlayer = GetPlayer(sessionId, user.Id);
        
        if (reconnectPlayer is not null)
        {
            reconnectPlayer.ConnectionId = connectionId;
            return session;
        }
        
        var isAdmin = session.AdminCanConnect;
        
        var player = Player.Create(user, isAdmin, sessionId);

        player.ConnectionId = connectionId;
        session.AddPlayer(player);

        _usersConnections.Add(connectionId, sessionId);
        return session;
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

    public RoundInfo? ChangeRound(int roundPosition, long sessionId)
    {
        var session = GetSessionById(sessionId);

        if (session is null)
        {
            return null;
        }
        
        session.ChangeRound(roundPosition);

        return session.CurrentRound;

    }
    
}