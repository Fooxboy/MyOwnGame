using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Handlers;

public class AuctionQuestionHandler : BaseQuestionHandler
{
    private readonly SessionCallbackService _callbackService;
    
    public AuctionQuestionHandler(QuestionParser questionParser, SessionCallbackService callbackService) : base(questionParser, callbackService)
    {
        _callbackService = callbackService;
    }

    public override QuestionPackType HandlerType => QuestionPackType.Auction;
    
    public override async Task HandleSelectQuestion(Session session, Player player, Player admin,  QuestionInfo questionInfo)
    {
        await _callbackService.QuestionSelectedAdmin(admin.ConnectionId, session.CurrentQuestion!.Answer);

        await _callbackService.QuestionSelected(player.SessionId, new List<QuestionBase>(),
            session.CurrentQuestion.QuestionPackInfo, -1, questionInfo.ThemeNumber, questionInfo.PriceNumber);

        await _callbackService.NeedSetQuestionPrice(player.SessionId, player, 1, player.Score, 1);
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
        session.AddAuctionPrice(player, price);

        var playersWithoutInstallPrices = session.Players.Where(p => !session.AuctionPrices!.Exists(x => x.Player.Id == p.Id) && !p.IsAdmin);

        var nextPlayer = playersWithoutInstallPrices.FirstOrDefault();

        if (nextPlayer is null)
        {
            var questionPlayer = session.AuctionPrices!.MaxBy(price => price.Price).Player;
            
            session.SetSelectQuestionPlayer(questionPlayer);

            await _callbackService.ChangeSelectQuestionPlayer(questionPlayer.SessionId, questionPlayer);

            var questionInfo = session.CurrentQuestion;
            
            session.CurrentRound!.Themes[questionInfo!.ThemeNumber].Prices[questionInfo.PriceNumber].IsAnswered = true;
            
            await _callbackService.QuestionSelected(questionPlayer.SessionId, questionInfo.Questions,
                questionInfo.QuestionPackInfo, -1, questionInfo.ThemeNumber, questionInfo.PriceNumber);
        }
        else
        {
            await _callbackService.NeedSetQuestionPrice(nextPlayer.SessionId, nextPlayer, 0, nextPlayer.Score, 1);
        }
            

        await PostHandleSetQuestionPrice(session, player, price);
    }
}