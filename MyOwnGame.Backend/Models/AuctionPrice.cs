using MyOwnGame.Backend.Domain;

namespace MyOwnGame.Backend.Models;

public class AuctionPrice
{
    public Player Player { get; set; }
    
    public int Price { get; set; }
}