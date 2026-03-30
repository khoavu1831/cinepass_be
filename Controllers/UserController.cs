using CinePass_be.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinePass_be;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserAsync()
    {
        try
        {
            var result = await _userService.GetAllUsersAsync();
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest("loi get list: " + e);
        }
        
    }
}
