using MyOwnGame.Backend.Database;

namespace MyOwnGame.Backend.Domain;

public class Player : User
{
    public required bool IsAdmin { get; init; }
    
    public int Score { get; private set; }
    
    public string ConnectionId { get; set; }
    
    public long SessionId { get; set; }


    public bool IsDisconnected { get; private set; } = false;
    
    public static Player Create(User user, bool isAdmin, long sessionId)
    {
        return new Player { Id = user.Id,
            AvatarImage = user.AvatarImage, 
            Name = user.Name,
            Score = 0, 
            IsAdmin = isAdmin,
            SessionId = sessionId
        };
    }

    public void Disconnect()
    {
        IsDisconnected = true;
    }

    public void Connect()
    {
        IsDisconnected = false;
    }
    
    public void AddScore(int score)
    {
        if (score < 0)
        {
            throw new ArgumentException("Нельзя прибавить отрицательный баланс лол");
        }

        Score += score;
    }

    public void RemoveScore(int score)
    {
        if (score < 0)
        {
            throw new ArgumentException("Нельзя отнимать отрицательный баланс лол");
        }

        Score -= score;
    }
}