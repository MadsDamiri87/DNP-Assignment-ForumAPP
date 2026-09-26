using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService commentService;

    public CommentsController(ICommentService commentService)
    {
        this.commentService = commentService;
    }

    [HttpPost]
    public async Task<IResult> CreateComment([FromBody] CreateCommentDto request)
    {
        try
        {
            CommentDto created = await commentService.CreateAsync(request);
            return Results.Created($"/comments/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> GetSingleComment([FromRoute] int id)
    {
        try
        {
            CommentDto comment = await commentService.GetSingleAsync(id);
            return Results.Ok(comment);
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }

    [HttpGet]
    public IResult GetComments(
        [FromQuery] int? postId,
        [FromQuery] int? userId,
        [FromQuery] string? userNameContains)
    {
        return Results.Ok(commentService.GetMany(postId, userId, userNameContains));
    }

    [HttpPut("{id:int}")]
    public async Task<IResult> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        try
        {
            CommentDto updated = await commentService.UpdateAsync(id, request);
            return Results.Ok(updated);
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IResult> DeleteComment([FromRoute] int id)
    {
        try
        {
            await commentService.DeleteAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }
}
