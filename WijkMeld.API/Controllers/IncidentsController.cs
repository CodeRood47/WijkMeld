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

namespace WijkMeld.API.Controllers
{
    [ApiController]
    [Route("Api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IncidentService _incidentService;

        public IncidentsController(IIncidentRepository incidentRepository, IncidentService incidentService)
        {
            _incidentRepository = incidentRepository;
            _incidentService = incidentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incident>>> GetAllIncidents()
        {
            var incidents = await _incidentRepository.GetAllAsync();
            return Ok(incidents);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Incident>> GetIncidentById(Guid id)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null)
            {
                return NotFound();
            }
            return Ok(incident);
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

    }

}
