using System.IO.Compression;
using System.Net;
using System.Xml.Serialization;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers;

public class SiqPackageParser
{

    private readonly IConfiguration _configuration;
    private readonly ILogger<SiqPackageParser> _logger;

    public SiqPackageParser(IConfiguration configuration, ILogger<SiqPackageParser> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string UnpackPackage(string pathToZip, string hash)
    {
        var pathToExtractedPackaged = Path.Combine(_configuration.GetValue<string>("packagesPath"), hash);

        Directory.CreateDirectory(pathToExtractedPackaged);

        using var archive = ZipFile.Open(pathToZip, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            var normalizedName = WebUtility.UrlDecode(entry.Name);
            
            _logger.LogTrace($"Распаковка файла {normalizedName}");
            
            entry.ExtractToFile(Path.Combine(pathToExtractedPackaged, normalizedName), true);
        }

        return pathToExtractedPackaged;
    }
    
    public Package? ParsePackage(string pathToXml)
    {
        var fileStringContent = File.ReadAllText(pathToXml).Replace("http://vladimirkhil.com/ygpackage3.0.xsd", string.Empty)
            .Replace("http://ur-quan1986.narod.ru/ygpackage3.0.xsd", string.Empty);
        
        var serializer = new XmlSerializer(typeof(Package));

        using var reader = new StringReader(fileStringContent);
        var package = serializer.Deserialize(reader) as Package;
        
        _logger.LogError("Файл content.xml успешно распаршен");

        return package;
    }
}