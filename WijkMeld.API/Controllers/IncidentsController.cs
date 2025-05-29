using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WijkMeld.API.Entities;
using WijkMeld.API.Entities.Enums;
using WijkMeld.API.Repositories.Incidents;
using System.Security.Claims;
using WijkMeld.API.Services;
using WijkMeld.API.Controllers.Dto;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore;
using WijkMeld.API.Data;
using WijkMeld.API.Repositories.IncidentPhotos;
using WijkMeld.API.Repositories.Users;
using Microsoft.AspNetCore.Authorization;

namespace WijkMeld.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IncidentService _incidentService;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserRepository _userRepository;

        public IncidentsController(IIncidentRepository incidentRepository, IncidentService incidentService, IWebHostEnvironment environment, IUserRepository userRepository)
        {
            _incidentRepository = incidentRepository;
            _incidentService = incidentService;
            _environment = environment;
            _userRepository = userRepository;
        }

        private async Task<IncidentResponseDto> MapToIncidentResponseDto(Incident incident)
        {
            return new IncidentResponseDto
            {
                Id = incident.Id,
                Name = incident.Name,
                Description = incident.Description,
                Location = new LocationDto
                {
                    Lat = incident.Location.Lat,
                    Long = incident.Location.Long,
                    Address = incident.Location.Address
                },
                Priority = incident.Priority,
                Status = incident.Status,
                Created = incident.Created,
                UserId = incident.UserId,
                UserName = incident.User?.UserName, // This will only work if User is included/loaded
                StatusUpdateIds = incident.StatusUpdates?.Select(su => su.Id).ToList() ?? new List<Guid>(),
                PhotoFilePaths = incident.Photos?.Select(p => p.FilePath).ToList() ?? new List<string>()
            };
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncidentResponseDto>>> GetAllIncidents()
        {
            var incidents = await _incidentRepository.GetAllAsync();
            var incidentDtos = new List<IncidentResponseDto>();
            foreach (var incident in incidents)
            {
                incidentDtos.Add(await MapToIncidentResponseDto(incident));
            }
            return Ok(incidentDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentResponseDto>> GetIncidentById(Guid id)
        {
            var incident = await _incidentRepository.GetByIdAsync(id); 
            if (incident == null)
            {
                return NotFound();
            }
            return Ok(await MapToIncidentResponseDto(incident));
        }

        [HttpPost]
        public async Task<ActionResult<Incident>> CreateIncident([FromBody] CreateIncidentDto incidentDto)
        {
            Guid? userId = Guid.Parse("d623c606-4d31-4ec3-8cfc-080659179e3c"); // voor test

            if (User.Identity?.IsAuthenticated == true && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid parsedUserId))
            {
                userId = parsedUserId;
            }

            var incident = new Incident
            {
                Name = incidentDto.Name,
                Description = incidentDto.Description,
                Location = new Location { Lat = incidentDto.Latitude, Long = incidentDto.Longitude },
                Priority = incidentDto.Priority,
                Status = Status.GEMELD, 
                Created = DateTime.UtcNow,
                UserId = userId
            };

            await _incidentRepository.AddAsync(incident);
            return CreatedAtAction(nameof(GetIncidentById), new { id = incident.Id }, incident);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncident(Guid id, [FromBody] UpdateIncidentDto incidentDto)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            incident.Name = incidentDto.Name;
            incident.Description = incidentDto.Description;
            incident.Location.Lat = incidentDto.Latitude;
            incident.Location.Long = incidentDto.Longitude;
            // Status en Priority worden NIET direct bijgewerkt via deze PUT

            try
            {
                await _incidentRepository.UpdateAsync(incident);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _incidentRepository.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncident(Guid id)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            await _incidentRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateIncidentStatus(Guid id, [FromBody] UpdateIncidentStatusDto statusUpdateDto)
        {
            var changedByUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(changedByUserIdString) || !Guid.TryParse(changedByUserIdString, out Guid changedByUserId))
            {
                return Unauthorized("Gebruiker ID kon niet worden opgehaald uit de claims.");
            }

            var success = await _incidentService.UpdateIncidentStatusAsync(
                id,
                changedByUserId,
                statusUpdateDto.NewStatus,
                statusUpdateDto.Note,
                statusUpdateDto.NewPriority
            );

            if (!success)
            {
                return NotFound("Incident niet gevonden of gebruiker niet gevonden.");
            }

            return Ok("Incident status succesvol bijgewerkt."); // 200 OK
        }

        [HttpPost("{id}/upload-photo")]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file, [FromServices] IWebHostEnvironment env, [FromServices] WijkMeldContext dbContext)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null)
                return NotFound("Incident niet gevonden.");

            if (file == null || file.Length == 0)
                return BadRequest("Geen geldig bestand ontvangen.");

            // 1. Pad maken
            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "incidents", id.ToString());
            Directory.CreateDirectory(uploadsFolder);

            // 2. Unieke bestandsnaam maken
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            // 3. Bestand opslaan
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Relatief pad voor toegang in frontend
            var relativePath = Path.Combine("/uploads/incidents", id.ToString(), fileName).Replace("\\", "/");

            // 5. Opslaan in database
            var incidentPhoto = new IncidentPhoto
            {
                IncidentId = id,
                FilePath = relativePath
            };

            dbContext.IncidentPhotos.Add(incidentPhoto);
            await dbContext.SaveChangesAsync();

            return Ok(new { photoUrl = relativePath });
        }

        [HttpGet("{id}/photos")]
        public async Task<IActionResult> GetIncidentPhotos(Guid id, [FromServices] IIncidentPhotoRepository photoRepo)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null) return NotFound("Incident niet gevonden.");

            var photos = await photoRepo.GetByIncidentIdAsync(id);
            return Ok(photos.Select(p => new { p.Id, p.FilePath }));
        }

    }

}
