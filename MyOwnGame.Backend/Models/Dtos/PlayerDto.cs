using MyOwnGame.Backend.Domain;

namespace MyOwnGame.Backend.Models.Dtos;

public class PlayerDto
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public string AvatarImage { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public int Score { get; set; }
    
    public string ConnectionId { get; set; }
    
    public long SessionId { get; set; }

    public static PlayerDto Create(Player p)
    {
        return new() { ConnectionId = p.ConnectionId, SessionId = p.SessionId, IsAdmin = p.IsAdmin, Score = p.Score };
    }
}