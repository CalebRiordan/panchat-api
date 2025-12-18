using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PanChatApi.Data;
using PanChatApi.Models;

namespace PanChatApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext context): ControllerBase
{

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDto request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username)

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            return BadRequest("Invalid username or password");
        }

    }


    private string CreateCookie(User user)
    {
        
    }
}