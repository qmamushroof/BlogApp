using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace BlogApp.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BlogService> _logger;

        public BlogService(ApplicationDbContext context, IMemoryCache cache, ILogger<BlogService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<BlogViewModel>> GetApprovedBlogsAsync(int skip, int take)
        {
            string cacheKey = $"blogsCacheKey_{skip}_{take}";

            if (_cache.TryGetValue(cacheKey, out List<BlogViewModel> blogs))
            {
                _logger.Log(LogLevel.Information, $"Blogs found in cache for key {cacheKey}");
                return blogs;
            }
            else
            {
                _logger.Log(LogLevel.Information, $"Blogs not found in cache. Attempting to load from Db for key {cacheKey}");
                var blogList = await _context.Blogs
                    .Where(blog => blog.Status == ApprovalStatus.Approved)
                    .OrderByDescending(blog => blog.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .Select(blog => new BlogViewModel
                    {
                        Id = blog.Id,
                        Title = blog.Title,
                        Content = blog.Content,
                        AuthorName = blog.User.UserName,
                        CreatedAt = blog.CreatedAt,
                        Status = blog.Status,

                        LikesCount = blog.Reactions.Count(reaction => reaction.Type == ReactionType.Like),
                        DislikesCount = blog.Reactions.Count(reaction => reaction.Type == ReactionType.Dislike)
                    })
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetPriority(CacheItemPriority.NeverRemove);

                _cache.Set(cacheKey, blogList, cacheEntryOptions);

                return blogList;
            }
        }

        public async Task<Blog> GetBlogByIdAsync(int id)
        {
            return await _context.Blogs
                .Include(blog => blog.User)
                .Include(blog => blog.Comments)
                    .ThenInclude(comment => comment.User)
                .Include(blog => blog.Reactions)
                .FirstOrDefaultAsync(blog => blog.Id == id);
        }

        public async Task<bool> CreateBlogAsync(Blog blog)
        {
            _context.Blogs.Add(blog);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateBlogAsync(Blog blog)
        {
            _context.Blogs.Update(blog);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBlogAsync(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return false;

            _context.Blogs.Remove(blog);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ApproveBlogAsync(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return false;

            blog.Status = ApprovalStatus.Approved;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectBlogAsync(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return false;

            blog.Status = ApprovalStatus.Rejected;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<CommentViewModel>> GetBlogCommentsAsync(int blogId)
        {
            return await _context.Comments
                .Where(comment => comment.BlogId == blogId)
                .OrderByDescending(comment => comment.CreatedAt)
                .Select(comment => new CommentViewModel
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    UserName = comment.User.UserName,
                    CreatedAt = comment.CreatedAt,
                    BlogId = comment.BlogId
                })
                .ToListAsync();
        }

        public async Task<bool> AddReactionAsync(Reaction reaction)
        {
            _context.Reactions.Add(reaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveReactionAsync(Reaction reaction)
        {
            _context.Reactions.Remove(reaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateReactionAsync(Reaction reaction)
        {
            _context.Reactions.Update(reaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Reaction> GetUserReactionAsync(int blogId, string userId)
        {
            return await _context.Reactions
                .FirstOrDefaultAsync(reaction => reaction.BlogId == blogId && reaction.UserId == userId);
        }

        public async Task<List<Blog>> GetPendingBlogsAsync()
        {
            return await _context.Blogs
                .Include(blog => blog.User)
                .Where(blog => blog.Status == ApprovalStatus.Pending)
                .OrderByDescending(blog => blog.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalApprovedBlogsCountAsync()
        {
            return await _context.Blogs
                .CountAsync(blog => blog.Status == ApprovalStatus.Approved);
        }

        public async Task<List<Blog>> GetBlogsByUserAsync(ApplicationUser user)
        {
            return await _context.Blogs
                .Include(blog => blog.User)
                .Where(blog => blog.User == user)
                .OrderByDescending(blog => blog.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Blog>> GetBlogsByUserAsync(ApplicationUser user, ApprovalStatus status)
        {
            return await _context.Blogs
                .Include(blog => blog.User)
                .Where(blog => blog.User == user && blog.Status == status)
                .OrderByDescending(blog => blog.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Blog>> GetMostLikedBlogsAsync(int quantity)
        {
            return (await _context.Blogs
                .Include(blog => blog.User)
                .Include(blog => blog.Reactions)
                .ToListAsync())
                .OrderByDescending(blog => blog.Reactions?.Count ?? 0)
                .Take(quantity)
                .ToList();
        }

        public async Task<List<Blog>> GetMostCommentedBlogsAsync(int quantity)
        {
            return (await _context.Blogs
                .Include(blog => blog.User)
                .Include(blog => blog.Comments)
                .ToListAsync())
                .OrderByDescending(blog => blog.Comments?.Count ?? 0)
                .Take(quantity)
                .ToList();
        }

        public async Task<List<Blog>> GetTopBlogsAsync(int quantity)
        {
            var mostLikedBlogs = await GetMostLikedBlogsAsync(quantity);
            var mostCommentedBlogs = await GetMostCommentedBlogsAsync(quantity);

            var topBlogs = mostLikedBlogs.Union(mostCommentedBlogs)
                .GroupBy(blog => blog.Id)
                .Select(group => group.First())
                .OrderByDescending(blog => blog.Reactions?.Count(reaction => reaction.Type == ReactionType.Like) ?? 0)
                .ThenByDescending(blog => blog.Comments?.Count ?? 0)
                .Take(quantity)
                .ToList();

            return topBlogs;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return false;

            _context.Comments.Remove(comment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(comment => comment.Id == id);
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}