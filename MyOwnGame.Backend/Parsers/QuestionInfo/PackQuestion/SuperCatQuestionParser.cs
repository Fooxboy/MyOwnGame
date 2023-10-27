using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.PackQuestion;

public class SuperCatQuestionParser : CatQuestionParser
{
    public override QuestionPackInfo ParseInfo(Question question)
    {
        var info = base.ParseInfo(question);

        var superCat = new SuperCatInfo();

        superCat.SelectPrice = info.CatInfo.SelectPrice;
        superCat.Theme = info.CatInfo.Theme;
        superCat.FixedPrice = info.CatInfo.FixedPrice;
        superCat.PriceType = info.CatInfo.PriceType;
        superCat.SelectPriceWithStep = info.CatInfo.SelectPriceWithStep;

        var toSelf = question.Type.Param?.FirstOrDefault(x => x.Name == "self").Text;

        superCat.CanGiveToSelf = toSelf is not null && bool.Parse(toSelf);

        info.CatInfo = null;
        info.SuperCatInfo = superCat;

        return info;
    }
}