using BlogApp.Models;
using BlogApp.Models.ViewModels;

namespace BlogApp.Services
{
    public interface IBlogService
    {
        Task<List<BlogViewModel>> GetApprovedBlogsAsync(int skip, int take);
        Task<Blog> GetBlogByIdAsync(int id);
        Task<bool> CreateBlogAsync(Blog blog);
        Task<bool> UpdateBlogAsync(Blog blog);
        Task<bool> DeleteBlogAsync(int id);
        Task<bool> ApproveBlogAsync(int id);
        Task<bool> RejectBlogAsync(int id);
        Task<bool> AddCommentAsync(Comment comment);
        Task<List<CommentViewModel>> GetBlogCommentsAsync(int blogId);
        Task<bool> AddReactionAsync(Reaction reaction);
        Task<bool> UpdateReactionAsync(Reaction reaction);
        Task<Reaction> GetUserReactionAsync(int blogId, string userId);
        Task<List<Blog>> GetPendingBlogsAsync();
        Task<int> GetTotalApprovedBlogsCountAsync();
    }
}