using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;

namespace MyOwnGame.Backend.Models;

public class QuestionInfo
{
    public required List<QuestionBase> Questions { get;  set; }
    
    public QuestionPackInfo? QuestionPackInfo { get; set; }
    
    public required AnswerBase Answer { get; set; }
    
    public required int Price { get; set; }
}