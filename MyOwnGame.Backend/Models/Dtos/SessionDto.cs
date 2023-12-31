﻿using MyOwnGame.Backend.Domain;

namespace MyOwnGame.Backend.Models.Dtos;

public class SessionDto
{
    public List<PlayerDto> Players { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public RoundInfo? CurrentRound { get;  set; }
    
    public GameInfo GameInfo { get; set; }
    
    public PlayerDto? RespondingPlayer { get;  set; }
    
    public PlayerDto? SelectQuestionPlayer { get; set; }
    
    public SessionState State { get;  set; }
    
    public List<PlayerDto> FinalAnswers { get; set; }

    public static SessionDto Create(Session session)
    {
        var players = session.Players.Select(PlayerDto.Create);

        return new SessionDto()
        {
            Players = players.ToList(), CurrentRound = session.CurrentRound,
            RespondingPlayer = PlayerDto.Create(session.RespondingPlayer), 
            SelectQuestionPlayer = PlayerDto.Create(session.SelectQuestionPlayer),
            GameInfo = session.GameInfo,
            State = session.State, 
            CreatedAt = session.CreatedAt,
            FinalAnswers = session.FinalAnswers.Select(x=> PlayerDto.Create(x.Player)).ToList(),
        };
    }
    
}