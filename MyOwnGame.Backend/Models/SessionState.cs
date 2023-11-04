namespace MyOwnGame.Backend.Models;

public enum SessionState
{
    None,
    
    Created,
    
    Question,
    
    Answer,
    
    Table,
    
    ForwardQuestion,
    
    Pause
}