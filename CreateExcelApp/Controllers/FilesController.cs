using CreateExcelApp.Hubs;
using CreateExcelApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CreateExcelApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _context;

    private readonly IHubContext<MyHub> _hubContext;

    public FilesController(AppDbContext context, IHubContext<MyHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, int fileId)
    {
        if (file is not { Length: > 0 }) return BadRequest();

        var userFile = await _context.UserFiles.FirstAsync(x => x.Id == fileId);

        var filePath = userFile.FileName + Path.GetExtension(file.FileName); //.xlsx

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

        await using FileStream stream = new(path, FileMode.Create);
        await file.CopyToAsync(stream);

        userFile.CreatedDate = DateTime.Now;
        userFile.FilePath = filePath;
        userFile.FileStatus = FileStatus.Completed;

        await _context.SaveChangesAsync();

        //SignalR notification (Hangi kullanıcı oluşturmuşsa, authenticate yapmış o kullanıcıya gönderilecek.
        //CompletedFile => mesajimiz. => frontend'de bu metota subscribe olunacak.
        await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");
        //Dinleme işlemini "Files" sayfasında değilsek dinleyemeyeceğimiz için tüm sayfalarda yapabilmek gerekiyor.
        //Onun için dinleme işlemini layout.cshtml'de yaparız. Oraya aspnet-signalr cdn js kütüphanesini import ederiz.

        return Ok();
    }
}

