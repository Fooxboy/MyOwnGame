using Microsoft.AspNetCore.Mvc;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Controllers;

public class SessionController : Controller
{
    private readonly SessionService _sessionService;
    private readonly IConfiguration _configuration;
    private readonly UsersService _usersService;

    public SessionController(IConfiguration configuration, SessionService sessionService, UsersService usersService)
    {
        _configuration = configuration;
        _sessionService = sessionService;
        _usersService = usersService;
    }

    [HttpPost("sessions/createSession")]
    [RequestSizeLimit(200_000_000)]
    public async Task<ActionResult> CreateSession(IFormFile? package)
    {
        if (package == null || package.Length == 0)
        {
            return BadRequest("слыш передай файл");
        }

        var tempPathToZip = _configuration.GetValue<string>("filesPath");

        var packageFullPath = Path.Combine(tempPathToZip, $"{Guid.NewGuid()}.zip");

        using (var fileStream = new FileStream(packageFullPath, FileMode.Create))
        {
            await package.CopyToAsync(fileStream);
        }

        var sessionId = Random.Shared.Next(1111, 9999);
        var session = _sessionService.CreateSession(packageFullPath, sessionId);

        if (session is null)
        {
            return Problem("bruh");
        }
        
        return Ok(new CreateSessionDto() { SessionId = sessionId, Session = session});
    }

    [HttpPost("users/create")]
    public async Task<ActionResult> CreateUser(IFormFile? image, string name)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("слыш передай файл");
        }

        var pathToUsersImages = _configuration.GetValue<string>("avatarsPath");

        var pathToUserAvatar = Path.Combine(pathToUsersImages, $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}");

        await using var fileStream = new FileStream(pathToUserAvatar, FileMode.Create);
        await image.CopyToAsync(fileStream);

        var user = await _usersService.CreateUser(pathToUserAvatar, name);


        return Ok(user);
    }

    [HttpPost("users/updateName")]
    public async Task<ActionResult> UpdateName(long userId, string newName)
    {
        var newUser = await _usersService.UpdateName(userId, newName);

        return Ok(newUser);
    }

    [HttpPost("users/updateAvatar")]
    public async Task<ActionResult> UpdateAvatar(long userId, IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("слыш передай файл");
        }

        var pathToUsersImages = _configuration.GetValue<string>("avatarsPath");

        var pathToUserAvatar = Path.Combine(pathToUsersImages, $"{Guid.NewGuid()}.jpg");

        await using var fileStream = new FileStream(pathToUserAvatar, FileMode.Create);
        await image.CopyToAsync(fileStream);

        var newUser = await _usersService.UpdateAvatar(userId, pathToUserAvatar);

        return Ok(newUser);
    }
}