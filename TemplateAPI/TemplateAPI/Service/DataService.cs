using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using Data;
using shared.Model; // Ændret namespace — bruger dine Shared modeller

namespace Service;

public class DataService
{
    // Ændret: PostContext i stedet for BookContext
    private PostContext db { get; }

    // Ændret constructor
    public DataService(PostContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Seeder noget nyt data i databasen hvis det er nødvendigt.
    /// </summary>
    public void SeedData()
    {
        // Hvis der ikke findes brugere, posts og kommentarer, så tilføj nogle eksempler.
        if (!db.Users.Any())
        {
            var user1 = new User("Alice");
            var user2 = new User("Bob");
            var user3 = new User("Charlie");

            db.Users.AddRange(user1, user2, user3);
            db.SaveChanges(); // Gem først brugerne så de får Id’er
        }

        if (!db.Posts.Any())
        {
            var alice = db.Users.First(u => u.Username == "Alice");
            var bob = db.Users.First(u => u.Username == "Bob");

            var post1 = new Post(alice, "Velkommen til Mini-Reddit!", "Dette er den første post på sitet.", 10, 1);
            var post2 = new Post(bob, "Hvad synes I om C#?", "Jeg er begyndt at lære C# og vil gerne høre jeres erfaringer.", 7, 0);

            db.Posts.AddRange(post1, post2);
            db.SaveChanges();
        }

        if (!db.Comments.Any())
        {
            var post1 = db.Posts.First();
            var charlie = db.Users.First(u => u.Username == "Charlie");

            var comment1 = new Comment("Spændende projekt!", 3, 0, charlie);
            var comment2 = new Comment("Jeg elsker C#!", 5, 0, charlie);

            post1.Comments.Add(comment1);
            db.Posts.First(p => p.Id == post1.Id).Comments.Add(comment2);

            db.SaveChanges();
        }
    }

    // Henter alle posts inkl. relaterede data
    public List<Post> GetPosts()
    {
        return db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .ToList();
    }

    // Henter en enkelt post inkl. kommentarer
    public Post GetPost(int id)
    {
        return db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefault(p => p.Id == id);
    }

    // Henter alle brugere
    public List<User> GetUsers()
    {
        return db.Users.ToList();
    }

    // Henter en specifik bruger
    public User GetUser(int id)
    {
        return db.Users.FirstOrDefault(u => u.Id == id);
    }

    // Opretter en ny post
    public string CreatePost(string title, string content, int userId)
    {
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return "User not found";

        var post = new Post(user, title, content, 0, 0);
        db.Posts.Add(post);
        db.SaveChanges();
        return "Post created";
    }

    // Tilføjer en kommentar til en post
    public string AddComment(int postId, string content, int userId)
    {
        var post = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        var user = db.Users.FirstOrDefault(u => u.Id == userId);

        if (post == null) return "Post not found";
        if (user == null) return "User not found";

        var comment = new Comment(content, 0, 0, user);
        post.Comments.Add(comment);
        db.SaveChanges();
        return "Comment added";
    }
}
