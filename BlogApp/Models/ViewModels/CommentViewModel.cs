using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BlogId { get; set; }
    }
}