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
    public async Task<IResult> CreateSubForum([FromBody] CreateSubForumDto request)
    {
        try
        {
            SubForumDto created = await subForumService.CreateAsync(request);
            return Results.Created($"/subforums/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> GetSingleSubForum([FromRoute] int id)
    {
        try
        {
            SubForumDto subForum = await subForumService.GetSingleAsync(id);
            return Results.Ok(subForum);
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }

    [HttpGet]
    public IResult GetSubForums([FromQuery] string? nameContains, [FromQuery] int? creatorUserId)
    {
        return Results.Ok(subForumService.GetMany(nameContains, creatorUserId));
    }

    [HttpGet("{id:int}/posts")]
    public async Task<IResult> GetPostsForSubForum([FromRoute] int id)
    {
        try
        {
            await subForumService.GetSingleAsync(id);
            return Results.Ok(postService.GetMany(subForumId: id));
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IResult> UpdateSubForum([FromRoute] int id, [FromBody] UpdateSubForumDto request)
    {
        try
        {
            SubForumDto updated = await subForumService.UpdateAsync(id, request);
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
    public async Task<IResult> DeleteSubForum([FromRoute] int id)
    {
        try
        {
            await subForumService.DeleteAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }
}
