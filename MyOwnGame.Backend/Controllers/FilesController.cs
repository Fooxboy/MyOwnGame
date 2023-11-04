using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend.Controllers;

public class FilesController : Controller
{
    private readonly FilesService _filesService;

    public FilesController(FilesService filesService)
    {
        _filesService = filesService;
    }

    [HttpGet("/avatars/{filename}")]
    public async Task<IActionResult> GetAvatar(string filename)
    {
        var filePath = _filesService.GetAvatarPathByName(filename);
        
        if (filePath is null)
        {
            NotFound(filename);
        }
        
        Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + (60 * 60 * 24);
        
        return File(System.IO.File.OpenRead(filePath), "image/jpeg");
    }

    [HttpGet("/content/{sessionId}/{filename}")]
    public async Task<IActionResult> GetContent(long sessionId, string filename)
    {
        var filePath = _filesService.GetSessionContent(sessionId, filename);

        if (filePath is null)
        {
            NotFound(filename);
        }
        
        new FileExtensionContentTypeProvider().TryGetContentType(filename, out string contentType);
        
        contentType = contentType ?? "application/octet-stream";
        
        Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + (60 * 60 * 24);
        
        return File(System.IO.File.OpenRead(filePath), contentType);
    }

    [HttpGet("/content/background.jpg")]
    public async Task<IActionResult> GetBackground()
    {
        var path = _filesService.GetBackground();
        
        if (path is null)
        {
            NotFound(path);
        }
        
        Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + (60 * 60 * 24);
        
        return File(System.IO.File.OpenRead(path), "image/jpeg");
    }
}