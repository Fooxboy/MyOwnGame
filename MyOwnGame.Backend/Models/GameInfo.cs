namespace MyOwnGame.Backend.Models;

public class GameInfo
{
    public string PackageName { get; set; }
    
    public List<string> Themes { get; set; }
    
    public DateTime PackageCreatedAt { get; set; }
}