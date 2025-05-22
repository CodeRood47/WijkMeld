using Microsoft.AspNetCore.Mvc;
using WijkMeld.API.Controllers.Dto;
using WijkMeld.API.Entities;
using WijkMeld.API.Repositories;
using WijkMeld.API.Repositories.Users;

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
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll() // Verander return type
        {
            var users = await _repository.GetAllAsync(); // Deze haalt de EF User objecten op (met Includes!)

            // Map de EF User objecten naar UserResponseDto objecten
            var userDtos = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role,
                IncidentIds = u.Incidents?.Select(i => i.Id).ToList() ?? new List<Guid>() // Map alleen de incident IDs
            }).ToList();

            return Ok(userDtos); // Retourneer de DTO's
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetById(Guid id) // Verander return type
        {
            var user = await _repository.GetByIdAsync(id); // Haalt de EF User op (met Includes!)
            if (user == null) return NotFound();

            // Map het EF User object naar een UserResponseDto
            var userDto = new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                IncidentIds = user.Incidents?.Select(i => i.Id).ToList() ?? new List<Guid>()
            };

            return Ok(userDto); // Retourneer de DTO
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
        public async Task<IActionResult> Create(User user)
        {
            user.Id = Guid.NewGuid(); 
            await _repository.AddAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }


    }

}
