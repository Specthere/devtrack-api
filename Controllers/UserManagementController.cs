using devtrack.AppDBContext;
using devtrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace devtrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserManagementController(AppDbContext context) => _context = context;

        [Authorize(Roles = "developer")]
        [HttpPost("add-mandor")]
        public IActionResult AddMandor([FromBody] User mandor)
        {
            if (_context.Users.Any(u => u.Email == mandor.Email))
                return BadRequest("Email sudah digunakan.");

            var role = _context.Roles.FirstOrDefault(r => r.RoleName.ToLower() == "mandor");
            if (role == null) return BadRequest("Role mandor tidak ditemukan");

            mandor.RoleId = role.RoleId;
            mandor.Password = BCrypt.Net.BCrypt.HashPassword(mandor.Password);
            mandor.Role = null;

            _context.Users.Add(mandor);
            var saveUser = _context.SaveChanges();

            if (saveUser == 0) return BadRequest("Gagal menyimpan user mandor.");

            _context.MandorProjects.Add(new MandorProject { UserId = mandor.UserId });
            _context.SaveChanges();

            return Ok(new { message = "Mandor berhasil dibuat", userId = mandor.UserId });
        }



        [Authorize(Roles = "mandor")]
        [HttpPut("edit-password")]
        public IActionResult EditPassword([FromBody] ChangePasswordDto model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var mandor = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (mandor == null || mandor.Role.RoleName != "mandor") return Unauthorized();

            mandor.Password = model.NewPassword;
            _context.SaveChanges();

            return Ok(new { message = "Password updated successfully" });
        }

        public class ChangePasswordDto
        {
            public string NewPassword { get; set; }
        }
    }
}
