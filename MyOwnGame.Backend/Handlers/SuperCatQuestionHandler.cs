using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Handlers;

public class SuperCatQuestionHandler : CatQuestionHandler
{
    private readonly SessionCallbackService _callbackService;
    
    public SuperCatQuestionHandler(QuestionParser questionParser, SessionCallbackService callbackService) : base(questionParser, callbackService)
    {
        _callbackService = callbackService;
    }
    
    public override QuestionPackType HandlerType => QuestionPackType.SuperCat;

    public override async Task HandleForwardQuestion(Session session, Player forwardPlayer)
    {
        var question = session.CurrentQuestion;

        var max = 0;
        var min = 0;
        var step = 0;

        switch (question.QuestionPackInfo.CatInfo.PriceType)
        {
            case QuestionPackPriceType.Fixed:
                max = question.QuestionPackInfo.SuperCatInfo.FixedPrice.Value;
                min = question.QuestionPackInfo.SuperCatInfo.FixedPrice.Value;
                step = 0;
                break;
            case QuestionPackPriceType.Select:
                max = question.QuestionPackInfo.SuperCatInfo.SelectPrice.To;
                min = question.QuestionPackInfo.SuperCatInfo.SelectPrice.From;
                step = 1;
                break;
            case QuestionPackPriceType.MaxOrMin:
                max = session.CurrentRound.Themes[question.ThemeNumber].Prices.Max(x => x.Price);
                min = session.CurrentRound.Themes[question.ThemeNumber].Prices.Min(x => x.Price);
                step = 0;
                break;
            case QuestionPackPriceType.SelectWithStep:
                max = question.QuestionPackInfo.SuperCatInfo.SelectPriceWithStep.To;
                min = question.QuestionPackInfo.SuperCatInfo.SelectPriceWithStep.From;
                step = question.QuestionPackInfo.SuperCatInfo.SelectPriceWithStep.Step.Value;
                break;
        }

        await _callbackService.NeedSetQuestionPrice(forwardPlayer.SessionId, forwardPlayer, min, max, step);

        await _callbackService.QuestionForwarded(forwardPlayer.SessionId, forwardPlayer);
        
        session.ChangeRespondingPlayer(forwardPlayer);
    }
}