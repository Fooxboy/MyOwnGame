using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Handlers;

public class FreeQuestionHandler : SimpleQuestionHandler
{
    public FreeQuestionHandler(QuestionParser questionParser, SessionCallbackService callbackService) : base(questionParser, callbackService)
    {
    }

    public override QuestionPackType HandlerType => QuestionPackType.FreeQuestion;
    
    public override Task HandleRejectAnswer(Session session, Player player)
    {
        return Task.CompletedTask;
    }
}