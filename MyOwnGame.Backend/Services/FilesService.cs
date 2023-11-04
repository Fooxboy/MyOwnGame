using MyOwnGame.Backend.Managers;

namespace MyOwnGame.Backend.Services;

public class FilesService
{
    private IConfiguration _configuration;

    private SessionsManager _sessionsManager;

    public FilesService(IConfiguration configuration, SessionsManager sessionsManager)
    {
        _configuration = configuration;
        _sessionsManager = sessionsManager;
    }

    public string? GetAvatarPathByName(string fileName)
    {
        var avatarsPath = _configuration.GetValue<string>("avatarsPath");

        var filePath = Path.Combine(avatarsPath, fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }
        
        return filePath;
    }

    public string? GetSessionContent(long sessionId, string filename)
    {
        var session = _sessionsManager.GetSessionById(sessionId);

        if (session is null)
        {
            throw new Exception("Сессия не найдена!!");
        }

        var packagesPath = _configuration.GetValue<string>("packagesPath");

        var sessionResourcesPath = Path.Combine(packagesPath, session.PackageHash, filename);
        
        if (!File.Exists(sessionResourcesPath))
        {
            return null;
        }


        return sessionResourcesPath;
    }

    public string GetBackground()
    {
        var path = _configuration.GetValue<string>("backgroundPath");

        return path;
    }
}