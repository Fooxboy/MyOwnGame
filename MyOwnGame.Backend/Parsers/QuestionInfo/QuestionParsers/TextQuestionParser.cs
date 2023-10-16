using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.QuestionParsers;

public class TextQuestionParser : IQuestionParser
{
    public QuestionBase ParseQuestion(Question question)
    {
        var questionInfo = new TextQuestion();
        
        var scenario = question.Scenario.Atom.FirstOrDefault();

        if (scenario is null)
        {
            throw new Exception("не найден вопрос што");
        }

        questionInfo.Type = QuestionContentType.Text;
        questionInfo.Text = scenario.Text;
        
        return questionInfo;
    }
}