using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Models;

public class RoundInfo
{
    public string Name { get;  set; }
    
    public bool IsFinal { get; set; }
    
    public List<RoundTheme> Themes { get; set; }
    
    public int Number { get; set; }

    public static RoundInfo Parse(Round roundSiq)
    {
        var round = new RoundInfo();

        round.Themes = new List<RoundTheme>();

        foreach (var theme in roundSiq.Themes.Theme)
        {
            var roundTheme = new RoundTheme();

            roundTheme.Prices = theme.Questions.Question
                .Select(x => new PriceInfo() { IsAnswered = false, Price = x.Price }).ToList();
            
            roundTheme.Name = theme.Name;
            
            round.Themes.Add(roundTheme);
        }

        round.Name = roundSiq.Name;
        round.IsFinal = roundSiq.Type == "final";


        return round;

    }
}