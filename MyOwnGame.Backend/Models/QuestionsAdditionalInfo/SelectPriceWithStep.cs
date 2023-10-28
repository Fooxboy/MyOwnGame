namespace MyOwnGame.Backend.Models.QuestionsAdditionalInfo;

public class SelectPriceWithStep
{
    public int From { get; set; }
    
    public int To { get; set; }
    
    public int? Step { get; set; }
}