using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BlogApp.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BlogController> _logger;

        public BlogController(IBlogService blogService, UserManager<ApplicationUser> userManager, ILogger<BlogController> logger)
        {
            _blogService = blogService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(id);
                //if (blog == null || blog.Status != ApprovalStatus.Approved)
                if (blog == null)
                {
                    _logger.Log(LogLevel.Error, "Blog with specified id not found.");
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
            catch (Exception)
            {
                _logger.Log(LogLevel.Error, "Error occurred while fetching blog details in BlogController.");
            }
            return RedirectToAction("Index", "Home");
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
            try
            {
                if (model.Content == null || model.Title == null)
                {
                    ModelState.AddModelError(string.Empty, "Title and Content are required.");
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
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the blog.");
                _logger.Log(LogLevel.Error, "Error occurred while creating blog in BlogController.");
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(id);
                if (blog == null)
                {
                    _logger.Log(LogLevel.Error, "Blog with specified id not found.");
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (blog.UserId != currentUser.Id && !User.IsInRole("Admin"))
                {
                    _logger.Log(LogLevel.Error, "User not authorized to edit this blog.");
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
            catch (Exception)
            {
                _logger.Log(LogLevel.Error, "Error occurred while fetching blog for editing in BlogController.");
            }

            return RedirectToAction("Details", "Blog", new { id = id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BlogViewModel model)
        {
            try
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

                if (!(await _blogService.UpdateBlogAsync(blog)))
                {
                    TempData["Message"] = "Blog not found deleted.";
                    return RedirectToAction("Index", "Home");
                }

                TempData["Message"] = "Your blog post has been updated.";
                return RedirectToAction("Details", new { id = blog.Id });
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the blog.");
                _logger.Log(LogLevel.Error, "Error occurred while updating blog in BlogController.");
            }
            return RedirectToAction("Details", "Blog", new { id = model.Id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(id);
                if (blog == null)
                {
                    _logger.Log(LogLevel.Error, "Blog with specified id not found.");
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (blog.UserId != currentUser.Id)
                {
                    _logger.Log(LogLevel.Error, "User not authorized to delete this blog.");
                    return Forbid();
                }

                if (!(await _blogService.DeleteBlogAsync(blog.Id)))
                {
                    TempData["Message"] = "Blog not found or already deleted.";
                    return RedirectToAction("Index", "Home");
                }

                TempData["Message"] = "Your blog post has been deleted.";
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the blog.");
                _logger.Log(LogLevel.Error, "Error occurred while deleting blog in BlogController.");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> React(int blogId, ReactionType type)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(blogId);
                if (blog == null || blog.Status != ApprovalStatus.Approved)
                {
                    _logger.Log(LogLevel.Error, "Blog with specified id not found or not approved.");
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
            catch (Exception)
            {
                _logger.Log(LogLevel.Error, "Error occurred while reacting to blog in BlogController.");
            }
            return RedirectToAction("Details", "Blog", new { id = blogId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AllBlogs()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var allBlogs = await _blogService.GetBlogsByUserAsync(currentUser);

                return View(allBlogs);
            }
            catch (Exception)
            {
                _logger.Log(LogLevel.Error, "Error occurred while fetching all blogs in BlogController.");
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> BlogsByStatus(ApprovalStatus status)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var blogs = await _blogService.GetBlogsByUserAsync(currentUser, status);

                ViewBag.SelectedStatus = status;
                ViewData["status"] = status;

                return View(nameof(AllBlogs), blogs);
            }
            catch (Exception)
            {
                _logger.Log(LogLevel.Error, "Error occurred while fetching blogs by status in BlogController.");
            }

            return RedirectToAction("Index", "Home");
        }

    }
}