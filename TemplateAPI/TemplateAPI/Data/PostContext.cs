using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using shared.Model;

namespace Data
{
    public class PostContext : DbContext
    {
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<User> Users => Set<User>();

        public PostContext(DbContextOptions<PostContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Eksempel på relationer:
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne<Post>()
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
