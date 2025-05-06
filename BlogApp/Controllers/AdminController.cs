using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IBlogService blogService, UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
        {
            _blogService = blogService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> BlogApproval()
        {
            try
            {
                var pendingBlogs = await _blogService.GetPendingBlogsAsync();
                return View(pendingBlogs);

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "BlogApproval of AdminController failed.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBlog(int id)
        {
            if (!(await _blogService.ApproveBlogAsync(id)))
            {
                TempData["Message"] = "Blog not found or already dealt with.";
                _logger.Log(LogLevel.Error, "ApproveBlog of AdminController couldn't find blog with specified id.");
            }
            return RedirectToAction(nameof(BlogApproval));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBlog(int id)
        {
            if (!(await _blogService.RejectBlogAsync(id)))
            {
                TempData["Message"] = "Blog not found or already dealt with.";
                _logger.Log(LogLevel.Error, "RejectBlog of AdminController couldn't find blog with specified id.");
            }
            return RedirectToAction(nameof(BlogApproval));
        }

        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "ManageUsers of AdminController failed.");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthorizeUsers(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.IsBlocked = !user.IsBlocked;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    TempData["Message"] = "User not found.";
                    _logger.Log(LogLevel.Error, "AuthorizeUsers of AdminController couldn't find user with specified userId.");
                }
                return RedirectToAction(nameof(ManageUsers));
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while authorizing the user.";
                _logger.Log(LogLevel.Error, "AuthorizeUsers of AdminController failed.");
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MostLikedBlogs()
        {
            try
            {
                int quantity = 5;
                var topBlogs = await _blogService.GetMostLikedBlogsAsync(quantity);
                return View("Views/Admin/TopBlogs.cshtml", topBlogs);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while retrieving the most liked blogs.";
                _logger.Log(LogLevel.Error, "MostLikedBlogs of AdminController couldn't view most liked blogs.");
            }

            return View("Views/Admin/TopBlogs.cshtml");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MostCommentedBlogs()
        {
            try
            {
                int quantity = 5;
                var topBlogs = await _blogService.GetMostCommentedBlogsAsync(quantity);
                return View("Views/Admin/TopBlogs.cshtml", topBlogs);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while retrieving the most commented blogs.";
                _logger.Log(LogLevel.Error, "MostCommentedBlogs of AdminController couldn't view most commented blogs.");
            }

            return View("Views/Admin/TopBlogs.cshtml");
        }

        public async Task<IActionResult> TopBlogs()
        {
            try
            {
                int quantity = 5;

                int blogsCount = await _blogService.GetTotalApprovedBlogsCountAsync();
                if (blogsCount >= 5)
                {
                    var topBlogs = await _blogService.GetTopBlogsAsync(quantity);
                    return View(topBlogs);
                }
                else
                {
                    var topBlogs = await _blogService.GetTopBlogsAsync(blogsCount);
                    return View(topBlogs);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "TopBlogs of AdminController couldn't view top blogs.");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}