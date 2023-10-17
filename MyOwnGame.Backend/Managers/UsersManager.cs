using LiteDB;
using MyOwnGame.Backend.Database;

namespace MyOwnGame.Backend.Managers;

public class UsersManager
{
    private readonly ILogger<UsersManager> _logger;

    public UsersManager(ILogger<UsersManager> logger)
    {
        _logger = logger;
    }

    public User CreateUser(string name, string pathImage)
    {
        _logger.LogTrace("Открытие подключения к БД");
        using var db = new LiteDatabase("database.db");
        var users = db.GetCollection<User>();

        var user = new User() { AvatarImage = Path.GetFileName(pathImage), Name = name };

        users.Insert(user);
        
        _logger.LogTrace("Закрытие подключения к БД");

        return user;
    }

    public User? GetUser(long id)
    {
        using var db = new LiteDatabase("database.db");
        var users = db.GetCollection<User>();
        var user = users.FindById(id);

        return user;
    }

    public User UpdateUser(long id, string name, string pathImage)
    {
        using var db = new LiteDatabase("database.db");

        var users = db.GetCollection<User>();
        var user = users.FindById(id);

        user.AvatarImage = pathImage;
        user.Name = name;

        users.Update(user);

        return user;
    }
}