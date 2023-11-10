using Microsoft.AspNetCore.Mvc;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Controllers;

public class AdminController : Controller
{
    private readonly AdminService _adminService;
    private readonly SessionService _sessionService;

    public AdminController(AdminService adminService, SessionService sessionService)
    {
        _adminService = adminService;
        _sessionService = sessionService;
    }

    [HttpGet("/admin/getSessions")]
    public async Task<Dictionary<long, Session>> GetSessions()
    {
        return _adminService.GetSessions();
    }

    [HttpGet("/admin/CloseSession")]
    public async Task<bool> CloseSession(long sessionId)
    {
        return await _adminService.CloseSession(sessionId);
    }

    [HttpGet("/admin/getPlayers")]
    public async Task<List<Player>> GetPlayersInSessions(long sessionId)
    {
        return _adminService.GetPlayersInSessions(sessionId);
    }
    
    [HttpGet("/admin/changeRound")]
    public async Task ChangeRound(long sessionId, int round)
    {
        await _sessionService.ChangeRound(round, $"admin_{sessionId}");
    }
    
    [HttpGet("/admin/acceptAnswer")]
    public async Task AcceptAnswer(long sessionId)
    {
        await _sessionService.AcceptAnswer($"admin_{sessionId}");
    }
    
    [HttpGet("/admin/rejectAnswer")]
    public async Task RejectAnswer(long sessionId)
    {
        await _sessionService.RejectAnswer($"admin_{sessionId}");
    }
    
    [HttpGet("/admin/skipQuestion")]
    public async Task SkipQuestion(long sessionId)
    {
        await _sessionService.SkipQuestion($"admin_{sessionId}");
    }
    
    [HttpGet("/admin/removeFinalTheme")]
    public async Task RemoveFinalTheme(long sessionId, int index)
    {
        await _sessionService.RemoveFinalTheme(index, $"admin_{sessionId}");
    }
    
    [HttpGet("/admin/setScore")]
    public async Task SetScore(long sessionId, int playerId, int newScore)
    {
        await _sessionService.SetPlayerScore(playerId, newScore, $"admin_{sessionId}");
    }
    
    [HttpGet("/admin/setAdmin")]
    public async Task SetAdmin(long sessionId, int playerId)
    {
        await _sessionService.SetAdmin(playerId, $"admin_{sessionId}");
    }
    
    [HttpGet("/admin/changeSelectQuestionPlayer")]
    public async Task ChangeSelectQuestionPlayer(long sessionId, int playerId)
    {
        await _sessionService.SetSelectQuestionPlayer(playerId, $"admin_{sessionId}");
    }
}