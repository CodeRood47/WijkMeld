using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WijkMeld.API.Data;
using WijkMeld.API.Entities;
using WijkMeld.API.Services;

namespace WijkMeld.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly WijkMeldContext _context;
        private readonly JwtService _jwtService;

        public AuthenticationController(WijkMeldContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null) return Unauthorized("User not found");

            var hash = ComputeHash(loginRequest.Password);
            if (user.PasswordHash != hash) return Unauthorized("Invalid password");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token, userId = user.Id.ToString() });
        }
        private string ComputeHash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

}
