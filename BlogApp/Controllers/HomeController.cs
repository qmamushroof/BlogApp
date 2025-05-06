using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogService _blogService;


        public HomeController(ILogger<HomeController> logger, IBlogService blogService)
        {
            _logger = logger;
            _blogService = blogService;
        }

        public async Task<IActionResult> Index(int page = 0)
        {
            try
            {
                int pageSize = 5;

                var blogs = await _blogService.GetApprovedBlogsAsync(page * pageSize, pageSize);
                var totalCount = await _blogService.GetTotalApprovedBlogsCountAsync();

                ViewBag.CurrentPage = page;
                ViewBag.HasMorePages = (page + 1) * pageSize < totalCount;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_BlogPartial", blogs);
                }

                return View(blogs);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while loading the blogs.");
                _logger.Log(LogLevel.Error, "Index of HomeController couldn't load blogs.");
            }

            return View();
        }
    }
}