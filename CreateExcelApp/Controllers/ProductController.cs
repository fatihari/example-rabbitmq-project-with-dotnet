using CreateExcelApp.Models;
using CreateExcelApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreateExcelApp.Controllers;

    [Authorize]
    public class ProductController : Controller
    {

        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly RabbitMqProducer _rabbitMqProducer;
        public ProductController(AppDbContext context, UserManager<IdentityUser> userManager, RabbitMqProducer rabbitMqProducer)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMqProducer = rabbitMqProducer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await  _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();

            _rabbitMqProducer.Publish(new Shared.CreateExcelMessage() { FileId = userFile.Id});

            //frontendde "Files.cshmtl" içerisinde TempData["StartCreatingExcel"] yakalanacak.
            TempData["StartCreatingExcel"] = true;

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return View( await _context.UserFiles.Where(x=>x.UserId==user.Id).OrderByDescending(x=>x.Id).ToListAsync());
        }
    }
