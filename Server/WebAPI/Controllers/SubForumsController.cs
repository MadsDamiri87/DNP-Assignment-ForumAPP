using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using WebAPI.Auth;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SubForumsController : ControllerBase
{
    private readonly ISubForumService subForumService;
    private readonly IPostService postService;

    public SubForumsController(ISubForumService subForumService, IPostService postService)
    {
        this.subForumService = subForumService;
        this.postService = postService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SubForumDto>> CreateSubForum([FromBody] CreateSubForumDto request)
    {
        int? userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            SubForumDto created = await subForumService.CreateAsync(request, userId.Value);
            return Created($"/subforums/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubForumDto>> GetSingleSubForum([FromRoute] int id)
    {
        try
        {
            SubForumDto subForum = await subForumService.GetSingleAsync(id);
            return subForum;
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<SubForumDto>> GetSubForums([FromQuery] string? nameContains, [FromQuery] int? creatorUserId)
    {
        return Ok(subForumService.GetMany(nameContains, creatorUserId));
    }

    [HttpGet("{id:int}/posts")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsForSubForum([FromRoute] int id)
    {
        try
        {
            await subForumService.GetSingleAsync(id);
            return Ok(postService.GetMany(subForumId: id));
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SubForumDto>> UpdateSubForum([FromRoute] int id, [FromBody] UpdateSubForumDto request)
    {
        Caller? caller = User.GetCaller();

        if (caller is null)
        {
            return Unauthorized();
        }

        try
        {
            SubForumDto updated = await subForumService.UpdateAsync(id, request, caller);
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
    public async Task<ActionResult> DeleteSubForum([FromRoute] int id)
    {
        Caller? caller = User.GetCaller();

        if (caller is null)
        {
            return Unauthorized();
        }

        try
        {
            await subForumService.DeleteAsync(id, caller);
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
