namespace MyOwnGame.Backend.BackgroundTasks;

public class PackageCleaner : IBackgroundTask
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageCleaner> _logger;

    public PackageCleaner(IConfiguration configuration, ILogger<PackageCleaner> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public int Timeout => 54000;

    public Task Invoke()
    {
        _logger.LogWarning("Начало удаления пакетов");
        
        var pathToPackages = _configuration.GetValue<string>("packagesPath");

        var packageFiles = Directory.GetFiles(pathToPackages);

        foreach (var packageFile in packageFiles) 
        {
            _logger.LogWarning($"Удален пакет '{Path.GetFileName(packageFile)}'");
            File.Delete(packageFile);
        }

        return Task.CompletedTask;
    }
}