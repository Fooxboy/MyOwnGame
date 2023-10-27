using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.PackQuestion;

public class BasePackQuestionParser : IPackQuestionParser
{
    public virtual QuestionPackInfo ParseInfo(Question question)
    {
        var questionType = question.Type;

        var info = new QuestionPackInfo();

        if (questionType is null)
        {
            info.Type = QuestionPackType.Simple;

            return info;
        }

        info.Type = questionType.Name switch
        {
            "auction" => QuestionPackType.Auction,
            "bagcat" => QuestionPackType.SuperCat,
            "sponsored" => QuestionPackType.FreeQuestion,
            "cat" => QuestionPackType.Cat,
            "Другой" => QuestionPackType.Other,
            _ => QuestionPackType.Simple
        };

        return info;
    }
}