using Microsoft.EntityFrameworkCore;
using Data;
using Service;
using shared.Model;

var builder = WebApplication.CreateBuilder(args);

var AllowSomeStuff = "_AllowSomeStuff";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSomeStuff, builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<PostContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextSQLite")));

builder.Services.AddScoped<DataService>();

var app = builder.Build();

// Sikrer database findes og seeds data
using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
    dataService.SeedData();

    var db = scope.ServiceProvider.GetRequiredService<PostContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseCors(AllowSomeStuff);

// Alle responses = JSON
app.Use(async (context, next) =>
{
    context.Response.ContentType = "application/json; charset=utf-8";
    await next(context);
});

// GET alle posts
app.MapGet("/api/posts", (DataService service) =>
{
    return service.GetPosts().Select(p => new
    {
        p.Id,
        p.Title,
        p.Content,
        p.Upvotes,
        p.Downvotes,
        p.User,
        Comments = p.Comments.Select(c => new
        {
            c.Id,
            c.Content,
            c.Upvotes,
            c.Downvotes,
            c.User
        })
    });
});

// GET specifik post
app.MapGet("/api/posts/{id}", (DataService service, int id) =>
{
    var post = service.GetPost(id);
    if (post == null)
        return Results.NotFound(new { message = "Post not found" });

    return Results.Ok(new
    {
        post.Id,
        post.Title,
        post.Content,
        post.Upvotes,
        post.Downvotes,
        post.User,
        Comments = post.Comments.Select(c => new
        {
            c.Id,
            c.Content,
            c.Upvotes,
            c.Downvotes,
            c.User
        })
    });
});

// POST - Opret ny post
app.MapPost("/api/posts", (DataService service, NewPostData data) =>
{
    string result = service.CreatePost(data.Title, data.Content, data.User);
    return new { message = result };
});

// POST - Tilføj kommentar
app.MapPost("/api/comments", (DataService service, NewCommentData data) =>
{
    string result = service.AddComment(data.PostId, data.Content, data.User);
    return new { message = result };
});

//PUT - Upvote post
app.MapPut("/api/upvotepost/{postid}", (DataService service, int postid) =>
{
    var result = service.UpvotePost(postid);
    return new { message = result };
});

//PUT - Downvote post
app.MapPut("/api/downvotepost/{postid}", (DataService service, int postid) =>
{
    var result = service.DownvotePost(postid);
    return new { message = result };
});

//PUT - Upvote comment
app.MapPut("/api/upvotecomment/{postid}/{commentid}", (DataService service, int postid, int commentid) =>
{
    var result = service.UpvoteComment(postid, commentid);
    return new { message = result };
});


//PUT - Downvote comment
app.MapPut("/api/downvotecomment/{postid}/{commentid}", (DataService service, int postid, int commentid) =>
{
    var result = service.DownvoteComment(postid, commentid);
    return new { message = result };
});


app.Run();

// Brugeren angiver blot et navn
record NewPostData(string Title, string Content, string User);
record NewCommentData(int PostId, string Content, string User);
