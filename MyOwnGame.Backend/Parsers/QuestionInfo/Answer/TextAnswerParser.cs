using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.Answer;

public class TextAnswerParser : IAnswerParser
{
    public AnswerBase ParseAnswer(Question question)
    {
        var answer = new AnswerBase();

        answer.Type = QuestionContentType.Text;

        var right = question.Right;

        if (right is null)
        {
            throw new Exception("Не найдено значение для ноды ответа");
        }

        answer.Text = right.Answer;
        answer.Wrong = question.Wrong?.Answer ?? null;

        return answer;
    }
}