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
        private readonly ILogger<CommentController> _logger;

        public CommentController(IBlogService blogService, UserManager<ApplicationUser> userManager, ILogger<CommentController> logger)
        {
            _blogService = blogService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            try
            {
                if (model.Content == null)
                {
                    ModelState.AddModelError(string.Empty, "Comment content cannot be empty.");
                    return View(model);
                }

                var blog = await _blogService.GetBlogByIdAsync(model.BlogId);
                if (blog == null || blog.Status != ApprovalStatus.Approved)
                {
                    ModelState.AddModelError(string.Empty, "Blog not found or not approved.");
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
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while adding the comment.");
                _logger.Log(LogLevel.Error, "Create of CommentController failed.");
            }

            return RedirectToAction("Details", "Blog", new { id = model.BlogId });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var comment = await _blogService.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    ModelState.AddModelError(string.Empty, "Comment not found.");
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (comment.UserId != currentUser.Id && !User.IsInRole("Admin"))
                {
                    ModelState.AddModelError(string.Empty, "You do not have permission to edit this comment.");
                    return Forbid();
                }

                var model = new CommentViewModel
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    BlogId = comment.BlogId
                };

                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while retrieving the comment.");
                _logger.Log(LogLevel.Error, "Edit of CommentController failed.");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CommentViewModel model)
        {
            try
            {
                if (model.Content == null)
                {
                    ModelState.AddModelError(string.Empty, "Comment content cannot be empty.");
                    return View(model);
                }

                var comment = await _blogService.GetCommentByIdAsync(model.Id);
                if (comment == null)
                {
                    ModelState.AddModelError(string.Empty, "Comment not found.");
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (comment.UserId != currentUser.Id && !User.IsInRole("Admin"))
                {
                    ModelState.AddModelError(string.Empty, "You do not have permission to edit this comment.");
                    return Forbid();
                }

                comment.Content = model.Content;

                await _blogService.UpdateCommentAsync(comment);
                TempData["Message"] = "Your comment has been updated.";
                return RedirectToAction("Details", "Blog", new { id = comment.BlogId });
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the comment.");
                _logger.Log(LogLevel.Error, "Edit of CommentController failed.");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var comment = await _blogService.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    ModelState.AddModelError(string.Empty, "Comment not found.");
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (comment.User.Id != currentUser.Id)
                {
                    ModelState.AddModelError(string.Empty, "You do not have permission to delete this comment.");
                    return Forbid();
                }

                await _blogService.DeleteCommentAsync(comment.Id);
                TempData["Message"] = "Your comment has been deleted.";
                return RedirectToAction("Details", "Blog", new { id = comment.BlogId });
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the comment.");
                _logger.Log(LogLevel.Error, "Delete of CommentController failed.");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}