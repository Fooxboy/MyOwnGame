using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.Questions;

namespace MyOwnGame.Backend.Models;

public class QuestionInfo
{
    public required QuestionBase Question { get;  set; }
    
    public required AnswerBase Answer { get; set; }
    
    
    public required int Price { get; set; }
    
}