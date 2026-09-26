using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService postService;
    private readonly ICommentService commentService;

    public PostsController(IPostService postService, ICommentService commentService)
    {
        this.postService = postService;
        this.commentService = commentService;
    }

    [HttpPost]
    public async Task<IResult> CreatePost([FromBody] CreatePostDto request)
    {
        try
        {
            PostDto created = await postService.CreateAsync(request);
            return Results.Created($"/posts/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            return Results.BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> GetSinglePost([FromRoute] int id)
    {
        try
        {
            PostDto post = await postService.GetSingleAsync(id);
            return Results.Ok(post);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
            return Results.NotFound(e.Message);
        }
    }

    [HttpGet]
    public IResult GetPosts(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? userNameContains,
        [FromQuery] int? subForumId)
    {
        return Results.Ok(postService.GetMany(titleContains, userId, userNameContains, subForumId));
    }

    [HttpGet("{id:int}/comments")]
    public async Task<IResult> GetCommentsForPost([FromRoute] int id)
    {
        try
        {
            await postService.GetSingleAsync(id);
            return Results.Ok(commentService.GetMany(postId: id));
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
            return Results.NotFound(e.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        try
        {
            PostDto updated = await postService.UpdateAsync(id, request);
            return Results.Ok(updated);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            return Results.BadRequest(e.Message);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
            return Results.NotFound(e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IResult> DeletePost([FromRoute] int id)
    {
        try
        {
            await postService.DeleteAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
            return Results.NotFound(e.Message);
        }
    }
}
