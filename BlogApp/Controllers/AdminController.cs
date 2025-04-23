using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBlogService _blogService;

        public AdminController(IBlogService blogService)
        {
            _blogService = blogService;
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
    }
}