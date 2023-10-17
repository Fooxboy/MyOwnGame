using Microsoft.AspNetCore.Mvc;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Controllers;

public class SessionController : Controller
{
    private readonly SessionService _sessionService;
    private readonly IConfiguration _configuration;
    private readonly UsersService _usersService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(IConfiguration configuration, SessionService sessionService, UsersService usersService, ILogger<SessionController> logger)
    {
        _configuration = configuration;
        _sessionService = sessionService;
        _usersService = usersService;
        _logger = logger;
    }

    [HttpPost("sessions/createSession")]
    [RequestSizeLimit(200_000_000)]
    public async Task<ActionResult> CreateSession(IFormFile? package)
    {
        var tempPathToZip = _configuration.GetValue<string>("filesPath");

        var fileName = $"{Guid.NewGuid()}.zip";
        
        _logger.LogInformation($"Создание сессии с пакетом {fileName}");
        
        var packageFullPath = Path.Combine(tempPathToZip, fileName);

        try
        {
            if (package == null || package.Length == 0)
            {
                _logger.LogInformation("Ошибка: не передан файл пакета");
                
                return BadRequest("слыш передай файл");
            }

            using (var fileStream = new FileStream(packageFullPath, FileMode.Create))
            {
                await package.CopyToAsync(fileStream);
            }

            var sessionId = Random.Shared.Next(1111, 9999);
            var session = _sessionService.CreateSession(packageFullPath, sessionId);

            if (session is null)
            {
                _logger.LogError("Ошибка при создании сессии");

                return BadRequest("Ошибка при создании сессии");
            }
            
            _logger.LogInformation($"Создана сессия с id: {sessionId}");


            return Ok(new CreateSessionDto() { SessionId = sessionId, Session = session });
        }
        catch (Exception ex)
        {
            System.IO.File.Delete(packageFullPath);

            _logger.LogError(ex.Message, ex);
            
            throw;
        }
    }

    [HttpPost("users/create")]
    public async Task<ActionResult> CreateUser(IFormFile? image, string name)
    {
        if (image == null || image.Length == 0)
        {
            _logger.LogError("Не был передан файл изображения при создании пользователя");
            return BadRequest("слыш передай файл");
        }

        var pathToUsersImages = _configuration.GetValue<string>("avatarsPath");

        var pathToUserAvatar = Path.Combine(pathToUsersImages, $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}");

        _logger.LogInformation($"Чтение файла аватарки: {image.FileName}");
        await using var fileStream = new FileStream(pathToUserAvatar, FileMode.Create);
        await image.CopyToAsync(fileStream);
        
        var user = await _usersService.CreateUser(pathToUserAvatar, name);

        if (user is null)
        {
            return BadRequest("Ошибка при создании нового пользователя");
        }

        _logger.LogInformation($"Зарегистрован новый пользователь - {name}");

        return Ok(user);
    }

    [HttpPost("users/updateName")]
    public async Task<ActionResult> UpdateName(long userId, string newName)
    {
        _logger.LogInformation($"Изменение имени для пользователя с ID '{userId}' на имя '{newName}'");
        
        var newUser = await _usersService.UpdateName(userId, newName);

        if (newUser is null)
        {
            return BadRequest("Ошибка при изменении имени пользотваеля");
        }
        
        _logger.LogInformation($"Изменено имя пользоваетеля с ID '{userId}' на имя '{newName}'");

        return Ok(newUser);
    }

    [HttpPost("users/updateAvatar")]
    public async Task<ActionResult> UpdateAvatar(long userId, IFormFile? image)
    {
        if (image == null || image.Length == 0)
        {
            _logger.LogError("Не передан файл для изменения аватарки");
            return BadRequest("слыш передай файл");
        }
        
        _logger.LogInformation($"Изменение аватарки пользовователя с ID: {userId}");

        var pathToUsersImages = _configuration.GetValue<string>("avatarsPath");

        var pathToUserAvatar = Path.Combine(pathToUsersImages, $"{Guid.NewGuid()}.jpg");

        await using var fileStream = new FileStream(pathToUserAvatar, FileMode.Create);
        await image.CopyToAsync(fileStream);

        var newUser = await _usersService.UpdateAvatar(userId, pathToUserAvatar);

        if (newUser is null)
        {
            _logger.LogError("Ошибка при обновлении аватарки пользователя");

            BadRequest("Произошла ошибка при обновлении аватарки пользователя");
        }
        
        _logger.LogInformation($"Изменена аватарка пользовователя с ID: {userId}");


        return Ok(newUser);
    }
}