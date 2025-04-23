using BlogApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Reaction>()
                .HasIndex(reaction => new { reaction.UserId, reaction.BlogId })
                .IsUnique();

            builder.Entity<Blog>()
                .HasOne(blog => blog.User)
                .WithMany(user => user.Blogs)
                .HasForeignKey(blog => blog.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(comment => comment.Blog)
                .WithMany(blog => blog.Comments)
                .HasForeignKey(comment => comment.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(comment => comment.User)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Reaction>()
                .HasOne(reaction => reaction.Blog)
                .WithMany(blog => blog.Reactions)
                .HasForeignKey(reaction => reaction.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reaction>()
                .HasOne(reaction => reaction.User)
                .WithMany(user => user.Reactions)
                .HasForeignKey(reaction => reaction.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}