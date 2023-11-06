using Microsoft.AspNetCore.Mvc;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Controllers;

public class AdminController : Controller
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
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
}