namespace MyOwnGame.Backend.Models.QuestionsAdditionalInfo;

public class CatInfo
{
    public string? Theme { get; set; }
    
    public int? FixedPrice { get; set; }
    
    public SelectPrice? SelectPrice { get; set; }
    
    public SelectPriceWithStep? SelectPriceWithStep { get; set; }
    
    public QuestionPackPriceType? PriceType { get; set; }
}