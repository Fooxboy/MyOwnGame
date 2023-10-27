namespace MyOwnGame.Backend.Models.QuestionsAdditionalInfo;

public class QuestionPackInfo
{
    public QuestionPackType Type { get; set; }
    
    public CatInfo? CatInfo { get; set; }
    
    public SuperCatInfo? SuperCatInfo { get; set; }
}