using CinePass_be.DTOS;
using CinePass_be.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinePass_be
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Loi: Du lieu request khong hop le. Auth Controller");

                var result = _authService.ResigterAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Loi: LOI HE THONG!" + ex.Message
                });
            }
        }

    }
}
