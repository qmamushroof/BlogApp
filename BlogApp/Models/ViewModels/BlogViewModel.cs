using System;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    public class BlogViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ApprovalStatus Status { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public bool UserCanEdit { get; set; }
        public bool UserHasReacted { get; set; }
        public bool? UserReactionIsLike { get; set; }
    }
}