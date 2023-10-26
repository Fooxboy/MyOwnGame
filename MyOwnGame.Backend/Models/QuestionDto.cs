using MyOwnGame.Backend.Models.Questions;

namespace MyOwnGame.Backend.Models;

public class QuestionDto
{
    public QuestionContentType Type { get; set; }
    
    public string? Url { get; set; }
    
    public string? Text { get; set; }

    public static QuestionDto Create(QuestionBase questionBase)
    {
        var dto = new QuestionDto();

        dto.Type = questionBase.Type;

        if (questionBase is TextQuestion text)
        {
            dto.Text = text.Text;
        }

        if (questionBase is MediaQuestion media)
        {
            dto.Url = media.Url;
        }

        return dto;
    }
}