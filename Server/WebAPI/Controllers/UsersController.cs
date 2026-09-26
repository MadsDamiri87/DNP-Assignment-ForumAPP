using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

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
    public async Task<IResult> CreateUser([FromBody] CreateUserDto request)
    {
        try
        {
            UserDto created = await userService.CreateAsync(request);
            return Results.Created($"/users/{created.Id}", created);
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> GetSingleUser([FromRoute] int id)
    {
        try
        {
            UserDto user = await userService.GetSingleAsync(id);
            return Results.Ok(user);
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }

    [HttpGet]
    public IResult GetUsers([FromQuery] string? userNameContains)
    {
        return Results.Ok(userService.GetMany(userNameContains));
    }

    [HttpPut("{id:int}")]
    public async Task<IResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        try
        {
            UserDto updated = await userService.UpdateAsync(id, request);
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
    public async Task<IResult> DeleteUser([FromRoute] int id)
    {
        try
        {
            await userService.DeleteAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }
}
