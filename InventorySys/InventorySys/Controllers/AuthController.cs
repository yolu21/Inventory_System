using InventorySys.Data;
using InventorySys.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(InventoryDbContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto data)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UseName == data.Username);
            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "帳號錯誤"
                });
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(
                data.Password,
                user.PasswordHash
                );

            if (!isValid)
            {
                return Unauthorized(new
                {
                    message = "密碼錯誤"
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UseName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                    )
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims:claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials:credentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "登入成功",
                token = tokenString
            });
        }
    }
}
