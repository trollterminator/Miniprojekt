using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;

using Data;
using Service;

var builder = WebApplication.CreateBuilder(args);

// Sætter CORS så API'en kan bruges fra andre domæner
var AllowSomeStuff = "_AllowSomeStuff";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSomeStuff, builder => {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Ændret: Bruger PostContext i stedet for BookContext
builder.Services.AddDbContext<PostContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextSQLite")));

// Tilføj DataService så den kan bruges i endpoints
builder.Services.AddScoped<DataService>();

// Dette kode kan bruges til at fjerne "cykler" i JSON objekterne.
/*
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = 
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
*/

var app = builder.Build();

// Seed data hvis nødvendigt.
using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
    dataService.SeedData(); // Fylder data på, hvis databasen er tom. Ellers ikke.

    // Sørger for at databasen bliver oprettet, hvis den ikke findes
    var db = scope.ServiceProvider.GetRequiredService<PostContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseCors(AllowSomeStuff);

// Middlware der kører før hver request. Sætter ContentType for alle responses til "JSON".
app.Use(async (context, next) =>
{
    context.Response.ContentType = "application/json; charset=utf-8";
    await next(context);
});


// DataService fås via "Dependency Injection" (DI)
app.MapGet("/", (DataService service) =>
{
    return new { message = "Hello World!" };
});

// POSTS

// Henter alle posts (inkl. bruger og kommentarer)
app.MapGet("/api/posts", (DataService service) =>
{
    return service.GetPosts().Select(p => new {
        p.Id,
        p.Title,
        p.Content,
        p.Upvotes,
        p.Downvotes,
        User = p.User != null ? new { p.User.Id, p.User.Username } : null,
        Comments = p.Comments.Select(c => new {
            c.Id,
            c.Content,
            c.Upvotes,
            c.Downvotes,
            User = c.User != null ? new { c.User.Id, c.User.Username } : null
        })
    });
});

// Henter en specifik post (inkl. kommentarer)
app.MapGet("/api/posts/{id}", (DataService service, int id) =>
{
    var post = service.GetPost(id);
    if (post == null) return Results.NotFound(new { message = "Post not found" });

    return Results.Ok(new
    {
        post.Id,
        post.Title,
        post.Content,
        post.Upvotes,
        post.Downvotes,
        User = post.User != null ? new { post.User.Id, post.User.Username } : null,
        Comments = post.Comments.Select(c => new {
            c.Id,
            c.Content,
            c.Upvotes,
            c.Downvotes,
            User = c.User != null ? new { c.User.Id, c.User.Username } : null
        })
    });
});

// Opretter en ny post
app.MapPost("/api/posts", (DataService service, NewPostData data) =>
{
    string result = service.CreatePost(data.Title, data.Content, data.UserId);
    return new { message = result };
});



// COMMENTS

// Tilføjer en kommentar til en post
app.MapPost("/api/comments", (DataService service, NewCommentData data) =>
{
    string result = service.AddComment(data.PostId, data.Content, data.UserId);
    return new { message = result };
});


// USERS

// Henter alle brugere
app.MapGet("/api/users", (DataService service) =>
{
    return service.GetUsers().Select(u => new { u.Id, u.Username });
});

// Henter en specifik bruger
app.MapGet("/api/users/{id}", (DataService service, int id) =>
{
    var user = service.GetUser(id);
    if (user == null) return Results.NotFound(new { message = "User not found" });
    return Results.Ok(new { user.Id, user.Username });
});

app.Run();

record NewPostData(string Title, string Content, int UserId);
record NewCommentData(int PostId, string Content, int UserId);