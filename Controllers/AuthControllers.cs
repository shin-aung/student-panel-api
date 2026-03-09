using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Models;
using StudentPortal.API.Services;
namespace StudentPortal.API.Controllers;

using Microsoft.AspNetCore.Cors;

[ApiController]
[Route("api/auth")]
[EnableCors("ReactApp")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var result = await _auth.Login(req);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        try
        {
            var result = await _auth.Register(req);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Protected route example — requires valid JWT
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(new
        {
            id = claims.GetValueOrDefault("sub"),
            email = claims.GetValueOrDefault("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"),
            studentId = claims.GetValueOrDefault("studentId"),
            firstName = claims.GetValueOrDefault("firstName"),
            lastName = claims.GetValueOrDefault("lastName"),
        });
    }
}