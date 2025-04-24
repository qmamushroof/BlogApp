using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Blogs = new HashSet<Blog>();
            Comments = new HashSet<Comment>();
            Reactions = new HashSet<Reaction>();
        }

        public bool IsAdmin { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public virtual ICollection<Blog> Blogs { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Reaction> Reactions { get; set; }
    }
}