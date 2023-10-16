using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.Answer;

public class MediaAnswerParser : IAnswerParser
{
    public AnswerBase ParseAnswer(Question question)
    {
        var answer = new MediaAnswer();
        var scenario = question.Scenario.Atom.LastOrDefault();

        if (scenario is null)
        {
            throw new Exception("Невозможно определить тип ответа для медиа контента");
        }

        var mediaType = StringToQuestionContentTypeMapper.Map(scenario.Type);

        answer.Type = mediaType;
        answer.Url = scenario.Text.Replace("@", string.Empty);
        answer.Text = question.Right.Answer;

        answer.Wrong = question.Wrong?.Answer ?? null;

        return answer;
    }
}