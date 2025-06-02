using devtrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using devtrack.AppDBContext;
using Microsoft.EntityFrameworkCore;


namespace devtrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User login)
        {
            var user = _context.Users.Include(u => u.Role)
                .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            if (user == null)
                return Unauthorized();

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Nama),
            new Claim(ClaimTypes.Role, user.Role.RoleName),
            new Claim("UserId", user.UserId.ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256));

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
