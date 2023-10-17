using System.Runtime.InteropServices.ComTypes;
using MyOwnGame.Backend.Database;
using MyOwnGame.Backend.Managers;

namespace MyOwnGame.Backend.Services;

public class UsersService
{
    private readonly UsersManager _usersManager;

    private readonly ILogger<UsersService> _logger;

    public UsersService(UsersManager usersManager, ILogger<UsersService> logger)
    {
        _usersManager = usersManager;
        _logger = logger;
    }

    public async Task<User?> CreateUser(string filePath, string name)
    {
        try
        {
            return _usersManager.CreateUser(name, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка при создании нового пользователя");
            _logger.LogError(ex, ex.Message);

            return null;
        }
    }

    public async Task<User?> UpdateName(long id, string name)
    {
        try
        {
            var user = _usersManager.GetUser(id);

            if (user is null)
            {
                _logger.LogError($"Не найден пользоваетель с ID '{id}'");

                return null;
            }

            var newUser = _usersManager.UpdateUser(id, name, user.AvatarImage);

            return newUser;
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка при изменении имени пользователя");
            _logger.LogError(ex, ex.Message);

            return null;
        }
    }

    public async Task<User?> UpdateAvatar(long id, string newAvatarFilePath)
    {
        var user = _usersManager.GetUser(id);

        if (user is null)
        {
            _logger.LogError($"Не найден пользователь с ID '{id}'");
            
            return null;
        }

        var newUser = _usersManager.UpdateUser(id, user.Name, newAvatarFilePath);

        return newUser;
    }

    public async Task<User?> GetUser(long id)
    {
        return _usersManager.GetUser(id);
    }
}