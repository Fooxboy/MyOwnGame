using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Handlers;

public abstract class BaseQuestionHandler : IQuestionHandler
{
    private readonly QuestionParser _questionParser;
    private readonly SessionCallbackService _callbackService;

    protected BaseQuestionHandler(QuestionParser questionParser, SessionCallbackService callbackService)
    {
        _questionParser = questionParser;
        _callbackService = callbackService;
    }

    public abstract QuestionPackType HandlerType { get; }

    public virtual Task HandleSelectQuestion(Session session, Player player, Player admin, QuestionInfo questionInfo)
    {
        if (session.CurrentRound is null)
        {
            throw new Exception("Текущий рауд в сессии null");
        }

        session.ResetAuctionPrice();

        session.SelectCurrentQuestion(questionInfo);

        return Task.CompletedTask;
    }

    public virtual Task HandleAcceptQuestion(Session session, Player player)
    {
        session.ChangeStateToTable();
        session.SetSelectQuestionPlayer(player);

        return Task.CompletedTask;
    }

    public virtual async Task HandleRejectAnswer(Session session, Player player)
    {
        session.ResetRespondingPlayer();
            
        session.ChangeStateToQuestion();

        await _callbackService.RejectAnswer(player.SessionId, player, player.Score);
    }

    protected async Task PostHandleAcceptQuestion(Session session, Player player)
    {
        await _callbackService.AcceptAnswer(player.SessionId, player, player.Score, session.CurrentQuestion!.Answer);

        await _callbackService.ChangeSelectQuestionPlayer(player.SessionId, player);
    }

    public virtual Task HandleSetQuestionPrice(Session session, Player player, int price)
    {
        //nothing.

        return Task.CompletedTask;
    }

    public virtual Task HandleForwardQuestion(Session session, Player forwardPlayer)
    {
        return Task.CompletedTask;
    }

    protected async Task PostHandleSetQuestionPrice(Session session, Player player, int price)
    {
        await _callbackService.QuestionPriceInstalled(player.SessionId, player, price);
    }
}