using Microsoft.AspNetCore.Mvc;
using System.Text;
using WijkMeld.API.Controllers.Dto;
using WijkMeld.API.Entities;
using WijkMeld.API.Repositories;
using WijkMeld.API.Repositories.Users;
using WijkMeld.API.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;


namespace WijkMeld.API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repository;



        public UsersController(IUserRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            var users = await _repository.GetAllAsync(); 

            var userDtos = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role,
                IncidentIds = u.Incidents?.Select(i => i.Id).ToList() ?? new List<Guid>() 
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetById(Guid id) 
        {
            var user = await _repository.GetByIdAsync(id); 
            if (user == null) return NotFound();

           
            var userDto = new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                IncidentIds = user.Incidents?.Select(i => i.Id).ToList() ?? new List<Guid>()
            };

            return Ok(userDto); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return Ok("User Deleted");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, User user)
        {
            if (id != user.Id) return BadRequest();
            await _repository.UpdateAsync(user);
            return Ok($"{user.UserName} Has been updated");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var hashedPassword = ComputeHash(dto.PasswordHash);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                Role = dto.Role
            };
            Debug.WriteLine($"Creating user: {user.UserName} with email: {user.Email} and role: {user.Role}");
            await _repository.AddAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);

        }
        [HttpGet("{id}/incidents")]
        public async Task<ActionResult<IEnumerable<IncidentResponseDto>>> GetIncidentsForUser(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return NotFound();

            var incidents = user.Incidents?.Select(i => new IncidentResponseDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Location = new LocationDto {
                    Lat = i.Location.Lat,
                    Long = i.Location.Long
                },

                Priority = i.Priority,
                Status = i.Status,
                Created = i.Created,
                UserId = user.Id,
                UserName = user.UserName,
                StatusUpdateIds = i.StatusUpdates?.Select(s => s.Id).ToList() ?? new(),
                PhotoFilePaths = i.Photos?.Select(p => p.FilePath).ToList() ?? new List<string>()
            }) ?? Enumerable.Empty<IncidentResponseDto>();

            return Ok(incidents);
        }

        private string ComputeHash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

    }

}
