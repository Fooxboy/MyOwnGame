using MyOwnGame.Backend.Domain;

namespace MyOwnGame.Backend.Models;

public class FinalAnswer
{
    public Player Player { get; set; }
    
    public string Answer { get; set; }
    
    public int Price { get; set; }
}