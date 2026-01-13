using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PanChatApi.Data;
using PanChatApi.Models;

namespace PanChatApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext context, IConfiguration config): ControllerBase
{

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDto request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            return BadRequest("Invalid username or password");
        }

        return Ok(new { token = CreateToken(user)});
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto request)
    {
        // Check that user doesn't already exist
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest("Username already taken");
        }

        var user = new User()
        {
            Username = request.Username,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok(new { token = CreateToken(user)});
    }


    private string CreateToken(User user)
    {
        var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

       var tokenDescriptor = new JwtSecurityToken(
        claims: [new Claim(ClaimTypes.Name, user.Username)],
        expires: DateTime.Now.AddDays(30),
        signingCredentials: creds
       );

       return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}