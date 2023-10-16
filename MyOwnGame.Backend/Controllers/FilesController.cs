﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
        
        return File(System.IO.File.OpenRead(filePath), "image/jpeg");
    }

    [HttpGet("/content/{sessionId}/{filename}")]
    public async Task<IActionResult> GetContent(long sessionId, string filename)
    {
        var filePath = _filesService.GetSessionContent(sessionId, filename);
        
        new FileExtensionContentTypeProvider().TryGetContentType(filename, out string contentType);
        
        contentType = contentType ?? "application/octet-stream";
        
        return File(System.IO.File.OpenRead(filePath), contentType);
    }
}