using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Blog
    {
        public Blog()
        {
            Comments = new HashSet<Comment>();
            Reactions = new HashSet<Reaction>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Reaction> Reactions { get; set; }

        public int LikesCount => Reactions?.Count(reaction => reaction.Type == ReactionType.Like) ?? 0;
        public int DislikesCount => Reactions?.Count(reaction => reaction.Type == ReactionType.Dislike) ?? 0;
    }
}