using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BlogApp.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlogController(IBlogService blogService, UserManager<ApplicationUser> userManager)
        {
            _blogService = blogService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _blogService.GetBlogByIdAsync(id);
            //if (blog == null || blog.Status != ApprovalStatus.Approved)
            if (blog == null || blog.Status == ApprovalStatus.Rejected)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var comments = await _blogService.GetBlogCommentsAsync(id);

            var viewModel = new BlogViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                AuthorName = blog.User.UserName,
                CreatedAt = blog.CreatedAt,
                Status = blog.Status,
                LikesCount = blog.LikesCount,
                DislikesCount = blog.DislikesCount,
                UserCanEdit = currentUser != null && (blog.UserId == currentUser.Id || User.IsInRole("Admin"))
            };

            if (currentUser != null)
            {
                var reaction = await _blogService.GetUserReactionAsync(id, currentUser.Id);
                if (reaction != null)
                {
                    viewModel.UserHasReacted = true;
                    viewModel.UserReactionIsLike = reaction.Type == ReactionType.Like;
                }
            }

            ViewBag.Comments = comments;
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;

            return View(viewModel);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogViewModel model)
        {
            if (model.Content == null || model.Title == null)
            {
                return View(model);
            }
            var currentUser = await _userManager.GetUserAsync(User);

            var blog = new Blog
            {
                Title = model.Title,
                Content = model.Content,
                UserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow,
                Status = ApprovalStatus.Pending
            };

            await _blogService.CreateBlogAsync(blog);

            TempData["Message"] = "Your blog post has been submitted for approval.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _blogService.GetBlogByIdAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (blog.UserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var model = new BlogViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BlogViewModel model)
        {
            if (model.Content == null || model.Title == null)
            {
                return View(model);
            }

            var blog = await _blogService.GetBlogByIdAsync(model.Id);
            if (blog == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (blog.UserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            blog.Title = model.Title;
            blog.Content = model.Content;
            blog.UpdatedAt = DateTime.UtcNow;

            await _blogService.UpdateBlogAsync(blog);
            TempData["Message"] = "Your blog post has been updated.";
            return RedirectToAction("Details", new { id = blog.Id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _blogService.GetBlogByIdAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (blog.UserId != currentUser.Id)
            {
                return Forbid();
            }

            await _blogService.DeleteBlogAsync(blog.Id);
            TempData["Message"] = "Your blog post has been deleted.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> React(int blogId, ReactionType type)
        {
            var blog = await _blogService.GetBlogByIdAsync(blogId);
            if (blog == null || blog.Status != ApprovalStatus.Approved)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var existingReaction = await _blogService.GetUserReactionAsync(blogId, currentUser.Id);
            var existingReaction2 = blog.Reactions.Where(x => x.UserId == currentUser.Id);

            if (existingReaction == null)
            {
                var reaction = new Reaction
                {
                    BlogId = blogId,
                    UserId = currentUser.Id,
                    Type = type,
                    CreatedAt = DateTime.UtcNow
                };
                await _blogService.AddReactionAsync(reaction);
            }
            else if (existingReaction.Type == type)
            {
                await _blogService.RemoveReactionAsync(existingReaction);
            }
            else
            {
                existingReaction.Type = type;
                await _blogService.UpdateReactionAsync(existingReaction);
            }

            return Json(new { likes = blog.LikesCount, dislikes = blog.DislikesCount });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AllBlogs()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var allBlogs = await _blogService.GetBlogsByUserAsync(currentUser);

            return View(allBlogs);
        }

        [Authorize]
        public async Task<IActionResult> BlogsByStatus(ApprovalStatus status)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var blogs = await _blogService.GetBlogsByUserAsync(currentUser, status);
            
            ViewBag.SelectedStatus = status;
            ViewData["status"] = status;

            return View(nameof(AllBlogs), blogs);
        }

    }
}