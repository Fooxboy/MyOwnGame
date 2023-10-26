using MyOwnGame.Backend.Models.Answers;

namespace MyOwnGame.Backend.Models;

public class AsnwerDto
{
    public QuestionContentType Type { get; set; }
    
    public string? Text { get; set; }
    
    public string? Wrong { get; set; }
    
    public string? Url { get; set; }


    public static AsnwerDto Create(AnswerBase answerBase)
    {
        var dto = new AsnwerDto();

        dto.Wrong = answerBase.Wrong;
        dto.Type = answerBase.Type;
        dto.Text = answerBase.Text;


        if (answerBase is MediaAnswer media)
        {
            dto.Url = media.Url;
        }

        return dto;
    }
}