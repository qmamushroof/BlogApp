using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IBlogService blogService, UserManager<ApplicationUser> userManager)
        {
            _blogService = blogService;
            _userManager = userManager;

        }

        public async Task<IActionResult> BlogApproval()
        {
            var pendingBlogs = await _blogService.GetPendingBlogsAsync();
            return View(pendingBlogs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBlog(int id)
        {
            await _blogService.ApproveBlogAsync(id);
            return RedirectToAction(nameof(BlogApproval));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBlog(int id)
        {
            await _blogService.RejectBlogAsync(id);
            return RedirectToAction(nameof(BlogApproval));
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthorizeUsers(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsBlocked = !user.IsBlocked;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MostLikedBlogs()
        {
            int quantity = 5;
            var topBlogs = await _blogService.GetMostLikedBlogsAsync(quantity);
            return View("Views/Admin/TopBlogs.cshtml", topBlogs);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MostCommentedBlogs()
        {
            int quantity = 5;
            var topBlogs = await _blogService.GetMostCommentedBlogsAsync(quantity);
            return View("Views/Admin/TopBlogs.cshtml", topBlogs);
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
            catch (Exception ex) { }
            return RedirectToAction("Index", "Home");
        }
    }
}