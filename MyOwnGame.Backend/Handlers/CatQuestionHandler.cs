using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Handlers;

public class CatQuestionHandler : BaseQuestionHandler
{
    private readonly SessionCallbackService _callbackService;
    
    public CatQuestionHandler(QuestionParser questionParser, SessionCallbackService callbackService) : base(questionParser, callbackService)
    {
        _callbackService = callbackService;
    }

    public override QuestionPackType HandlerType => QuestionPackType.Cat;

    public override async Task HandleSelectQuestion(Session session, Player player, Player admin,  QuestionInfo questionInfo)
    {
        await base.HandleSelectQuestion(session, player, admin, questionInfo);
        
        await _callbackService.QuestionSelectedAdmin(admin.ConnectionId, session.CurrentQuestion!.Answer);

        await _callbackService.QuestionSelected(player.SessionId, new List<QuestionBase>(),
            session.CurrentQuestion.QuestionPackInfo, -1, questionInfo.ThemeNumber, questionInfo.PriceNumber);

        await _callbackService.NeedForwardQuestion(player.ConnectionId, player.SessionId);
                
        session.ChangeStateToForwardQuestion();
    }

    public override async Task HandleAcceptQuestion(Session session, Player player)
    {
        await base.HandleAcceptQuestion(session, player);
        
        var price = session.AuctionPrices!.FirstOrDefault(x => x.Player.Id == player.Id);
                
        player.AddScore(price!.Price);
        
        await _callbackService.ScoreChanged(player.SessionId, player, player.Score);

        await PostHandleAcceptQuestion(session, player);
    }
    
    public override async Task HandleRejectAnswer(Session session, Player player)
    {
        var price = session.AuctionPrices!.FirstOrDefault(x => x.Player.Id == player.Id)!.Price;
        player.RemoveScore(price);
        
        await base.HandleRejectAnswer(session, player);
    }

    public override async Task HandleSetQuestionPrice(Session session, Player player, int price)
    {
        await base.HandleSetQuestionPrice(session, player, price);
        
        session.AddAuctionPrice(player, price);
            
        await _callbackService.QuestionSelected(player.SessionId, session.CurrentQuestion!.Questions,
            session.CurrentQuestion.QuestionPackInfo, -1, session.CurrentQuestion.ThemeNumber, session.CurrentQuestion.PriceNumber);

        await PostHandleSetQuestionPrice(session, player, price);
    }

    public override async Task HandleForwardQuestion(Session session, Player forwardPlayer)
    {
        var question = session.CurrentQuestion;

        var max = 0;
        var min = 0;
        var step = 0;

        switch (question.QuestionPackInfo.CatInfo.PriceType)
        {
            case QuestionPackPriceType.Fixed:
                max = question.QuestionPackInfo.CatInfo.FixedPrice.Value;
                min = question.QuestionPackInfo.CatInfo.FixedPrice.Value;
                step = 0;
                break;
            case QuestionPackPriceType.Select:
                max = question.QuestionPackInfo.CatInfo.SelectPrice.To;
                min = question.QuestionPackInfo.CatInfo.SelectPrice.From;
                step = 1;
                break;
            case QuestionPackPriceType.MaxOrMin:
                max = session.CurrentRound.Themes[question.ThemeNumber].Prices.Max(x => x.Price);
                min = session.CurrentRound.Themes[question.ThemeNumber].Prices.Min(x => x.Price);
                step = 0;
                break;
            case QuestionPackPriceType.SelectWithStep:
                max = question.QuestionPackInfo.CatInfo.SelectPriceWithStep.To;
                min = question.QuestionPackInfo.CatInfo.SelectPriceWithStep.From;
                step = question.QuestionPackInfo.CatInfo.SelectPriceWithStep.Step.Value;
                break;
        }

        await _callbackService.NeedSetQuestionPrice(forwardPlayer.SessionId, forwardPlayer, min, max, step);

        await _callbackService.QuestionForwarded(forwardPlayer.SessionId, forwardPlayer);
        
        session.ChangeRespondingPlayer(forwardPlayer);
    }
}