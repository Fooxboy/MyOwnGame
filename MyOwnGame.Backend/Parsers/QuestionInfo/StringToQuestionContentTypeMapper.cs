using MyOwnGame.Backend.Models;

namespace MyOwnGame.Backend.Parsers.QuestionInfo;

public class StringToQuestionContentTypeMapper
{
    private static readonly Dictionary<string, QuestionContentType> _map = new()
    {
        { "image", QuestionContentType.Image },
        { "video", QuestionContentType.Video },
        { "voice", QuestionContentType.Audio },
        { "say", QuestionContentType.Say }

    };
    
    public static QuestionContentType Map(string type)
    {
        if (!_map.TryGetValue(type, out var result))
        {
            throw new Exception($"Не найден тип контета {type}");
        }

        return result;
    }
}