using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Handlers;

public class SimpleQuestionHandler : BaseQuestionHandler
{
    private readonly QuestionParser _questionParser;
    private readonly SessionCallbackService _callbackService;

    public SimpleQuestionHandler(QuestionParser questionParser, SessionCallbackService callbackService) 
        : base(questionParser, callbackService)
    {
        _questionParser = questionParser;
        _callbackService = callbackService;
    }

    public override QuestionPackType HandlerType => QuestionPackType.Simple;

    public override async Task HandleSelectQuestion(Session session, Player player, Player admin, QuestionInfo questionInfo)
    {
        await base.HandleSelectQuestion(session, player, admin, questionInfo);
        
        var seconds = session.ChangeStateToQuestion(questionInfo!.Questions.Count);
        
        DelayTaskRunner.Run(seconds, () => _callbackService.PlayerCanAnswer(player.SessionId));

        session.CurrentRound!.Themes[questionInfo.ThemeNumber].Prices[questionInfo.PriceNumber].IsAnswered = true;
        
        await _callbackService.QuestionSelected(player.SessionId, questionInfo.Questions,
            questionInfo.QuestionPackInfo, seconds, questionInfo.ThemeNumber, questionInfo.PriceNumber);

        await _callbackService.QuestionSelectedAdmin(admin.ConnectionId, questionInfo.Answer);
    }

    public override async Task HandleAcceptQuestion(Session session, Player player)
    {
        await base.HandleAcceptQuestion(session, player);
        
        player.AddScore(session.CurrentQuestion!.Price);

        await PostHandleAcceptQuestion(session, player);
    }

    public override async Task HandleRejectAnswer(Session session, Player player)
    {
        var selectedQuestion = session.CurrentQuestion;
        
        player.RemoveScore(selectedQuestion!.Price);

        await base.HandleRejectAnswer(session, player);
    }

    public override Task HandleSetQuestionPrice(Session session, Player player, int price)
    {
        //nothing.

        return Task.CompletedTask;
    }
}