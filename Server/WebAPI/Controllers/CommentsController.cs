using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using WebAPI.Auth;

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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto request)
    {
        int? userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            CommentDto created = await commentService.CreateAsync(request, userId.Value);
            return Created($"/comments/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetSingleComment([FromRoute] int id)
    {
        try
        {
            CommentDto comment = await commentService.GetSingleAsync(id);
            return comment;
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments(
        [FromQuery] int? postId,
        [FromQuery] int? userId,
        [FromQuery] string? userNameContains)
    {
        return Ok(commentService.GetMany(postId, userId, userNameContains));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDto>> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        Caller? caller = User.GetCaller();

        if (caller is null)
        {
            return Unauthorized();
        }

        try
        {
            CommentDto updated = await commentService.UpdateAsync(id, request, caller);
            return updated;
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, e.Message);
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        Caller? caller = User.GetCaller();

        if (caller is null)
        {
            return Unauthorized();
        }

        try
        {
            await commentService.DeleteAsync(id, caller);
            return NoContent();
        }
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, e.Message);
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }
}
