using Microsoft.EntityFrameworkCore;
using shared.Model;

namespace Data
{
    public class PostContext : DbContext
    {
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();

        public string DbPath { get; }

        public PostContext()
        {
            DbPath = "bin/Post.db";
        }

        public PostContext(DbContextOptions<PostContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relation mellem Post og Comment
            modelBuilder.Entity<Comment>()
                .HasOne<Post>()
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
