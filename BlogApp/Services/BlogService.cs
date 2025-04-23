using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BlogViewModel>> GetApprovedBlogsAsync(int skip, int take)
        {
            return await _context.Blogs
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
                    LikesCount = blog.Reactions.Count(reaction => reaction.IsLike),
                    DislikesCount = blog.Reactions.Count(reaction => !reaction.IsLike)
                })
                .ToListAsync();
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
    }
}