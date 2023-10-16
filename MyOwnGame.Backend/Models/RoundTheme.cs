using System.Text.Json.Serialization;

namespace MyOwnGame.Backend.Models;

public class RoundTheme
{
    public string Name { get; set; }
    
    public List<PriceInfo> Prices { get; set; }
}