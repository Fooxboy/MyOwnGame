using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Handlers;

public class FinalQuestionHandler : BaseQuestionHandler
{
    private readonly SessionCallbackService _callbackService;
    
    public FinalQuestionHandler(QuestionParser questionParser, SessionCallbackService callbackService) : base(questionParser, callbackService)
    {
        _callbackService = callbackService;
    }

    public override QuestionPackType HandlerType => QuestionPackType.Final;

    public override Task HandleSelectQuestion(Session session, Player player, Player admin,  QuestionInfo questionInfo)
    {
        return Task.CompletedTask;
    }

    public override async Task HandleAcceptQuestion(Session session, Player player)
    {
        await base.HandleAcceptQuestion(session, player);
        
        var price = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id)!.Price;
        
        player.AddScore(price);
            
        await _callbackService.ScoreChanged(player.SessionId, player, player.Score);
        
        await _callbackService.AcceptAnswer(player.SessionId, player, player.Score, null);

        if (session.FinalAnswers.Count > 0)
        {
            var finalAnswer = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id);

            session.FinalAnswers.Remove(finalAnswer);

            var nextPlayer = session.FinalAnswers.FirstOrDefault().Player;

            session.ChangeRespondingPlayer(nextPlayer);

            await sessionService.ShowFinalAnswer(nextPlayer.Id, nextPlayer.SessionId);
        }
    }

    public override async Task HandleRejectAnswer(Session session, Player player)
    {
        var price = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id)!.Price;
            
        player.RemoveScore(price);
        
        await _callbackService.ScoreChanged(player.SessionId, player, player.Score);

        if (session.FinalAnswers.Count > 0)
        {
            var finalAnswer = session.FinalAnswers.FirstOrDefault(x => x.Player.Id == player.Id);

            session.FinalAnswers.Remove(finalAnswer);

            var nextPlayer = session.FinalAnswers.FirstOrDefault().Player;

            await sessionService.ShowFinalAnswer(nextPlayer.Id, nextPlayer.SessionId);
        }
    }

    public override Task HandleSetQuestionPrice(Session session, Player player, int price)
    {
        return Task.CompletedTask;
    }
}