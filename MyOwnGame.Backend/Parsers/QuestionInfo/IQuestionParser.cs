using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo;

public interface IQuestionParser
{
    public QuestionBase ParseQuestion(Question question);
    
    public List<QuestionBase> ParseQuestions(Question question);

}