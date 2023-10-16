namespace MyOwnGame.Backend.Models;

public enum SessionState
{
    None,
    
    Created,
    
    Question,
    
    Answer,
    
    Table,
    
    Pause
}