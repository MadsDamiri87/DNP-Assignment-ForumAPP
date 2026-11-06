using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using WebAPI.Auth;

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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto request)
    {
        int? userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            PostDto created = await postService.CreateAsync(request, userId.Value);
            return Created($"/posts/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSinglePost([FromRoute] int id)
    {
        try
        {
            PostDto post = await postService.GetSingleAsync(id);
            return post;
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> GetPosts(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? userNameContains,
        [FromQuery] int? subForumId)
    {
        return Ok(postService.GetMany(titleContains, userId, userNameContains, subForumId));
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost([FromRoute] int id)
    {
        try
        {
            await postService.GetSingleAsync(id);
            return Ok(commentService.GetMany(postId: id));
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDto>> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        Caller? caller = User.GetCaller();

        if (caller is null)
        {
            return Unauthorized();
        }

        try
        {
            PostDto updated = await postService.UpdateAsync(id, request, caller);
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
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        Caller? caller = User.GetCaller();

        if (caller is null)
        {
            return Unauthorized();
        }

        try
        {
            await postService.DeleteAsync(id, caller);
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
