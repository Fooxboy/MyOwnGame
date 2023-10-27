using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo;

public interface IPackQuestionParser
{
    public QuestionPackInfo ParseInfo(Question question);
}