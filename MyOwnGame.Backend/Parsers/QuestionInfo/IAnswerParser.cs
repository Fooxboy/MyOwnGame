using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo;

public interface IAnswerParser
{
    public AnswerBase ParseAnswer(Question question);
}