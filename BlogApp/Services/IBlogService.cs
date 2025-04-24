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
        Task<bool> RemoveReactionAsync(Reaction reaction);
        ///
        Task<Comment> GetCommentByIdAsync(int id);
        Task<bool> DeleteCommentAsync(int blogId);
        Task<bool> UpdateCommentAsync(Comment comment);

        Task<bool> UpdateReactionAsync(Reaction reaction);
        Task<Reaction> GetUserReactionAsync(int blogId, string userId);
        Task<List<Blog>> GetPendingBlogsAsync();
        Task<int> GetTotalApprovedBlogsCountAsync();

        Task<List<Blog>> GetBlogsByUserAsync(ApplicationUser user);
        Task<List<Blog>> GetBlogsByUserAsync(ApplicationUser user, ApprovalStatus status);
        Task<List<Blog>> GetMostLikedBlogsAsync(int quantity);
        Task<List<Blog>> GetMostCommentedBlogsAsync(int quantity);
        Task<List<Blog>> GetTopBlogsAsync(int quantity);
    }
}