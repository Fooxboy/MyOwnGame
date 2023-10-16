using MyOwnGame.Backend.Domain;

namespace MyOwnGame.Backend.Models.Dtos;

public class SessionDto
{
    public List<PlayerDto> Players { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public RoundInfo? CurrentRound { get;  set; }
    
    public GameInfo GameInfo { get; set; }
    
    public Player? RespondingPlayer { get;  set; }
    
    public SessionState State { get;  set; }


    public static SessionDto Create(Session session)
    {
        var players = session.Players.Select(PlayerDto.Create);

        return new SessionDto()
        {
            Players = players.ToList(), CurrentRound = session.CurrentRound,
            RespondingPlayer = session.RespondingPlayer, GameInfo = session.GameInfo, State = session.State, CreatedAt = session.CreatedAt
        };
    }
    
}