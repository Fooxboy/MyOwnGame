using MyOwnGame.Backend.Handlers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;

namespace MyOwnGame.Backend.Helpers;

public class QuestionHandlerFactory
{
    private readonly IEnumerable<IQuestionHandler> _handlers;

    public QuestionHandlerFactory(IEnumerable<IQuestionHandler> handlers)
    {
        _handlers = handlers;
    }

    public IQuestionHandler? GetHandler(QuestionInfo questionInfo)
    {
        if (questionInfo.QuestionPackInfo is null || questionInfo.QuestionPackInfo.Type == QuestionPackType.Simple)
        {
            return _handlers.FirstOrDefault(x => x.HandlerType == QuestionPackType.Simple);
        }

        return _handlers.FirstOrDefault(x => x.HandlerType == questionInfo.QuestionPackInfo.Type);
    }
}