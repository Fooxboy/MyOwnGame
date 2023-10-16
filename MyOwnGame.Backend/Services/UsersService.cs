using System.Runtime.InteropServices.ComTypes;
using MyOwnGame.Backend.Database;
using MyOwnGame.Backend.Managers;

namespace MyOwnGame.Backend.Services;

public class UsersService
{
    private readonly UsersManager _usersManager;

    public UsersService(UsersManager usersManager)
    {
        _usersManager = usersManager;
    }

    public async Task<User> CreateUser(string filePath, string name)
    {
        return _usersManager.CreateUser(name, filePath);
    }

    public async Task<User> UpdateName(long id, string name)
    {
        var user = _usersManager.GetUser(id);

        if (user is null)
        {
            throw new Exception("Не найден пользователь");
        }

        var newUser = _usersManager.UpdateUser(id, name, user.AvatarImage);

        return newUser;
    }

    public async Task<User> UpdateAvatar(long id, string newAvatarFilePath)
    {
        var user = _usersManager.GetUser(id);

        if (user is null)
        {
            throw new Exception("Не найден пользователь");
        }

        var newUser = _usersManager.UpdateUser(id, user.Name, newAvatarFilePath);

        return newUser;
    }

    public async Task<User?> GetUser(long id)
    {
        return _usersManager.GetUser(id);
    }
}