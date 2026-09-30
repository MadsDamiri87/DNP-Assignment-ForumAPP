using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

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

    [HttpPost]
    public async Task<ActionResult<SubForumDto>> CreateSubForum([FromBody] CreateSubForumDto request)
    {
        try
        {
            SubForumDto created = await subForumService.CreateAsync(request);
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SubForumDto>> UpdateSubForum([FromRoute] int id, [FromBody] UpdateSubForumDto request)
    {
        try
        {
            SubForumDto updated = await subForumService.UpdateAsync(id, request);
            return updated;
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteSubForum([FromRoute] int id)
    {
        try
        {
            await subForumService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }
}
