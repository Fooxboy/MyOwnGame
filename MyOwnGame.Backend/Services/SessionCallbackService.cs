using Microsoft.AspNetCore.SignalR;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Hubs;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.Dtos;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.QuestionsAdditionalInfo;

namespace MyOwnGame.Backend.Services;

public class SessionCallbackService
{
    private readonly IHubContext<SessionHub> _hubContext;

    public SessionCallbackService(IHubContext<SessionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SubscribeUserToEvents(long sessionId, string connectionId)
    {
        await _hubContext.Groups.AddToGroupAsync(connectionId, sessionId.ToString());
    }

    public async Task UnsubscribeUserFromEvents(long sessionId, string connectionId)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, sessionId.ToString());
    }

    public async Task PlayerConnectedToSession(long sessionId, Player player)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.PlayerConnectedToSession.ToString(), PlayerDto.Create(player));
    }

    public async Task PlayerDisconnectedFromSession(long sessionId, Player player)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.PlayerDisconnectedFromSession.ToString(), PlayerDto.Create(player));
    }

    public async Task AdminChanged(long sessionId, Player newAdmin)
    {
          await _hubContext.Clients.Group(sessionId.ToString())
                            .SendAsync(SessionEvents.AdminChanged.ToString(), PlayerDto.Create(newAdmin));
    }

    public async Task RoundChanged(long sessionId, RoundInfo roundInfo)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.RoundChanged.ToString(), roundInfo);
    }

    public async Task ChangeSelectQuestionPlayer(long sessionId, Player player)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.ChangeSelectQuestionPlayer.ToString(), PlayerDto.Create(player));
    }

    public async Task QuestionSelected(long sessionId, List<QuestionBase> questions, QuestionPackInfo? questionPackInfo, int delayTime,
        int themePosition, int pricePosition)
    {
        var questionDtos = questions.Select(x => QuestionDto.Create(x)).ToList();
        
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.QuestionSelected.ToString(), 
                questionDtos, 
                questionPackInfo, 
                delayTime,
                new QuestionSelectedPosition() {QuestionNumber = pricePosition, ThemeNumber = themePosition});
    }

    public async Task QuestionSelectedAdmin(string adminConnectionId, AnswerBase answer)
    {
        await _hubContext.Clients.Client(adminConnectionId).SendAsync(SessionEvents.QuestionSelectedAdmin.ToString(),
            AsnwerDto.Create(answer));
    }

    public async Task PlayerAnswer(long sessionId, Player player)
    {
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync(SessionEvents.PlayerAnswer.ToString(),
            PlayerDto.Create(player));
    }

    public async Task PlayerTriedAnswer(long sessionId, Player player)
    {
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync(SessionEvents.PlayerTriedAnswer.ToString(),
            PlayerDto.Create(player));
    }

    public async Task AcceptAnswer(long sessionId, Player player, int newScore, AnswerBase answer)
    {
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync(
            SessionEvents.AcceptAnswer.ToString(), PlayerDto.Create(player), newScore,
            AsnwerDto.Create(answer));
    }

    public async Task ScoreChanged(long sessionId, Player player, int newScore)
    {
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync(
            SessionEvents.ScoreChanged.ToString(), PlayerDto.Create(player), newScore);
    }

    public async Task RejectAnswer(long sessionId, Player player, int newScore)
    {
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync(
            SessionEvents.RejectAnswer.ToString(), PlayerDto.Create(player), newScore);
    }

    public async Task SkipQuestion(long sessionId, AnswerBase answer)
    {
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync(
            SessionEvents.SkipQuestion.ToString(), AsnwerDto.Create(answer));
    }

    public async Task FinalThemeRemoved(long sessionId, List<RoundTheme> themes)
    {
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync(
            SessionEvents.FinalThemeRemoved.ToString(), themes);
    }
    

    public async Task GamePaused(long sessionId, Session session)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.GamePaused.ToString(), SessionDto.Create(session));
    }

    public async Task GameResumed(long sessionId, Session session)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.GameResumed.ToString(), SessionDto.Create(session));
    }

    public async Task FinalQuestionResponsed(long session, Player player)
    {
        await _hubContext.Clients.Group(session.ToString())
            .SendAsync(SessionEvents.FinalQuestionResponsed.ToString(), PlayerDto.Create(player));
    }
    
    public async Task UserFinalAnswer(long session, FinalAnswer finalAnswer)
    {
        await _hubContext.Clients.Group(session.ToString()).SendAsync(SessionEvents.UserFinalAnswer.ToString(), 
            PlayerDto.Create(finalAnswer.Player), finalAnswer);
    }

    public async Task PlayerOffline(long sessionId, Player player)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.PlayerOffline.ToString(), PlayerDto.Create(player));
    }

    public async Task PlayerCanAnswer(long sessionId)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.PlayerCanAnswer.ToString());
    }

    public async Task NeedSetQuestionPrice(long sessionId, Player player, int min, int max, int step)
    {
        await _hubContext.Clients.Client(player.ConnectionId).SendAsync(SessionEvents.NeedSetQuestionPrice.ToString(),
            new SelectPriceWithStep() {From = min, To = max, Step = step});
        
        await _hubContext.Clients.Group(player.SessionId.ToString()).SendAsync(SessionEvents.PlayerInstallingQuestionPrice.ToString(), PlayerDto.Create(player));
    }

    public async Task QuestionPriceInstalled(long sessionId, Player player, int? score)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync(SessionEvents.QuestionPriceInstalled.ToString(), PlayerDto.Create(player), score);
    }
}