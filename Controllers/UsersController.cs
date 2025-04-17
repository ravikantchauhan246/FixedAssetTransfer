// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUserById(string id)
    {
        var user = await _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpGet("by-role/{roleName}")]
    public async Task<ActionResult<List<UserResponseDto>>> GetUsersByRole(string roleName)
    {
        var users = await _userService.GetUsersByRole(roleName);
        return Ok(users);
    }

    [HttpPost("{userId}/roles")]
    public async Task<IActionResult> AssignRolesToUser(string userId, [FromBody] List<string> roleNames)
    {
        try
        {
            await _userService.AssignRolesToUser(userId, roleNames);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}