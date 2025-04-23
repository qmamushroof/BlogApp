using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(IBlogService blogService, UserManager<ApplicationUser> userManager)
        {
            _blogService = blogService;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            var blog = await _blogService.GetBlogByIdAsync(model.BlogId);
            if (blog == null || blog.Status != ApprovalStatus.Approved)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var comment = new Comment
            {
                Content = model.Content,
                BlogId = model.BlogId,
                UserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _blogService.AddCommentAsync(comment);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var comments = await _blogService.GetBlogCommentsAsync(model.BlogId);
                return PartialView("_CommentsPartial", comments);
            }

            return RedirectToAction("Details", "Blog", new { id = model.BlogId });
        }
    }
}