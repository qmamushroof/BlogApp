namespace BlogApp.Models
{
    public enum ReactionType
    {
        Like,
        Dislike
    }
    public class Reaction
    {
        public int Id { get; set; }
        public ReactionType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

    }
}