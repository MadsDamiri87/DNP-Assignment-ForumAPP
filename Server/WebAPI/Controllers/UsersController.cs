using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using WebAPI.Auth;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService userService;

    public UsersController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto request)
    {
        try
        {
            UserDto created = await userService.CreateAsync(request);
            return Created($"/users/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetSingleUser([FromRoute] int id)
    {
        try
        {
            UserDto user = await userService.GetSingleAsync(id);
            return user;
        }
        catch (InvalidOperationException e)
        {
            return NotFound(e.Message);
        }
    }

    [Authorize]
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers([FromQuery] string? userNameContains)
    {
        return Ok(userService.GetMany(userNameContains));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser([FromRoute] int id,
        [FromBody] UpdateUserDto request)
    {
        Caller? caller = User.GetCaller();

        if (caller == null)
        {
            return Unauthorized();
        }

        try
        {
            UserDto updated = await userService.UpdateAsync(id, request, caller);
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
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        Caller? caller = User.GetCaller();

        if (caller == null)
        {
            return Unauthorized();
        }

        try
        {
            await userService.DeleteAsync(id, caller);
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