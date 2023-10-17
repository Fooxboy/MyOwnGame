using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.QuestionParsers;

public class MediaQuestionParser : IQuestionParser
{
    public QuestionBase ParseQuestion(Question question)
    {
        var questionInfo = new MediaQuestion();

        var scenario = question.Scenario.Atom.FirstOrDefault();

        if (scenario is null)
        {
            throw new Exception("Не найден вопрос што");
        }

        questionInfo.Type = StringToQuestionContentTypeMapper.Map(scenario.Type);

        questionInfo.Url = scenario.Text.Replace("@", string.Empty);

        return questionInfo;
    }

    public List<QuestionBase> ParseQuestions(Question question)
    {
        throw new NotImplementedException();
    }
}