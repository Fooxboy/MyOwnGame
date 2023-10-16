using System.Net.Mime;

namespace MyOwnGame.Backend.Models.Answers;

public class AnswerBase
{
    public QuestionContentType Type { get; set; }
    
    public string Text { get; set; }
    
    public string? Wrong { get; set; }
}