﻿using MyOwnGame.Backend.Domain;

namespace MyOwnGame.Backend.Models.Dtos;

public class CreateSessionDto
{
    public Session Session { get; set; }
    
    public long SessionId { get; set; }
}