using System.Text.RegularExpressions;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.PackQuestion;

public class CatQuestionParser : BasePackQuestionParser
{
    public override QuestionPackInfo ParseInfo(Question question)
    {
        var info = base.ParseInfo(question);

        if (info.Type != QuestionPackType.Cat || info.Type != QuestionPackType.SuperCat)
        {
            return info;
        }

        var catInfo = new CatInfo();

        var priceInfo = question.Type?.Param.FirstOrDefault(x => x.Name == "cost");

        if (priceInfo is null)
        {
            info.Type = QuestionPackType.Simple;

            return info;
        }

        if (int.TryParse(priceInfo.Text, out var price))
        {
            switch (price)
            {
                case 0:
                    catInfo.PriceType = QuestionPackPriceType.MaxOrMin;
                    break;
                case > 0:
                    catInfo.PriceType = QuestionPackPriceType.Fixed;
                    catInfo.FixedPrice = price;
                    break;
            }
        }
        else
        {
            catInfo.PriceType = priceInfo.Text.Contains('/') ? QuestionPackPriceType.SelectWithStep : QuestionPackPriceType.Select;
        }

        var regex = new Regex(@"\d+"); // Шаблон для поиска чисел

        var matches = regex.Matches(priceInfo.Text);

        if (matches.Count == 2)
        {
            var catPrice = new SelectPrice();

            catPrice.From = int.Parse(matches[0].Value);
            catPrice.To = int.Parse(matches[1].Value);

            catInfo.SelectPrice = catPrice;
        }

        if (matches.Count == 3)
        {
            var superCatPrice = new SelectPriceWithStep();

            superCatPrice.From = int.Parse(matches[0].Value);

            superCatPrice.To = int.Parse(matches[1].Value);

            superCatPrice.Step = int.Parse(matches[2].Value);

            catInfo.SelectPriceWithStep = superCatPrice;
        }

        catInfo.Theme = question.Type!.Param.FirstOrDefault(x => x.Name == "theme")?.Text ?? string.Empty;

        info.CatInfo = catInfo;

        return info;
    }
}