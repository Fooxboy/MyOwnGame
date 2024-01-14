using MyOwnGame.Backend.Domain;

namespace MyOwnGame.Backend.Models;

public class FinalPrice
{
    public Player Player { get; set; }
    
    public int Price { get; set; }
}