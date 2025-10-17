using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using shared.Model;

namespace Service;

public class DataService
{
    private PostContext db { get; }

    public DataService(PostContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Seeder noget testdata i databasen hvis den er tom.
    /// </summary>
    public void SeedData()
    {
        if (!db.Posts.Any())
        {
            var post1 = new Post(
                title: "Velkommen til Mini-Reddit!",
                content: "Dette er den første post på sitet.",
                user: "Alice",
                upvotes: 10,
                downvotes: 1
            );

            var post2 = new Post(
                title: "Hvad synes I om C#?",
                content: "Jeg er begyndt at lære C# og vil gerne høre jeres erfaringer.",
                user: "Bob",
                upvotes: 7,
                downvotes: 0
            );

            db.Posts.AddRange(post1, post2);
            db.SaveChanges();

            var comment1 = new Comment("Spændende projekt!", user: "Charlie", upvotes: 3);
            var comment2 = new Comment("Jeg elsker C#!", user: "Charlie", upvotes: 5);

            post1.Comments.Add(comment1);
            post2.Comments.Add(comment2);

            db.SaveChanges();
        }
    }

    // Henter alle posts inkl. kommentarer
    public List<Post> GetPosts()
    {
        return db.Posts
            .Include(p => p.Comments)
            .ToList();
    }

    // Henter en specifik post inkl. kommentarer
    public Post GetPost(int id)
    {
        return db.Posts
            .Include(p => p.Comments)
            .FirstOrDefault(p => p.Id == id);
    }

    // Opretter en ny post
    public string CreatePost(string title, string content, string user)
    {
        if (string.IsNullOrWhiteSpace(user))
        {
            user = GenerateRandomUserName();
        }

        var post = new Post(title: title, content: content, user: user);
        db.Posts.Add(post);
        db.SaveChanges();

        return "Post created";
    }

    // Tilføjer en kommentar til en post
    public string AddComment(int postId, string content, string user)
    {
        var post = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        if (post == null) return "Post not found";

        if (string.IsNullOrWhiteSpace(user))
        {
            user = GenerateRandomUserName();
        }

        var comment = new Comment(content: content, user: user);
        post.Comments.Add(comment);
        db.SaveChanges();

        return "Comment added";
    }

    public string UpvotePost(int postId)
    {
        var postToUpvote = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        if (postToUpvote == null)
        {
            return "Post not found";
        }

        postToUpvote.Upvotes++;
        db.SaveChanges();

        return "Post upvoted!";
    }

    public string DownvotePost(int postId)
    {
        var postToDownvote = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        if (postToDownvote == null)
        {
            return "Post not found";
        }

        postToDownvote.Upvotes--;
        db.SaveChanges();

        return "Post downvoted!";
    }

    public string UpvoteComment(int postId, int commentId)
    {
        var postWithComment = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        if (postWithComment == null)
        {
            return "Post not found";
        }
        var commentToUpvote = postWithComment.Comments.FirstOrDefault(c => c.Id == commentId);

        if (commentToUpvote == null)
        {
            return "Comment not found";
        }

        commentToUpvote.Upvotes++;
        db.SaveChanges();
        return "Comment upvoted!";
    }

    public string DownvoteComment(int postId, int commentId)
    {
        var postWithComment = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        if (postWithComment == null)
        {
            return "Post not found";
        }
        var commentToDownvote = postWithComment.Comments.FirstOrDefault(c => c.Id == commentId);

        if (commentToDownvote == null)
        {
            return "Comment not found";
        }

        commentToDownvote.Upvotes--;
        db.SaveChanges();
        return "Comment downvoted!";
    }

    // Genererer et tilfældigt navn, hvis brugeren ikke indtaster et
    private static string GenerateRandomUserName()
    {
        string[] names = { "AnonymousFox", "BlueTiger", "SilentPanda", "GreenFrog", "LazySloth" };
        var rand = new Random();
        return names[rand.Next(names.Length)];
    }
}
