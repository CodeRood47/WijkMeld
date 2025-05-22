using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Controllers.Dto
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }

        public List<Guid> IncidentIds { get; set; } = new List<Guid>();

    }

    public class CreateUserDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public UserRole Role { get; set; } = UserRole.USER;
    }

    public class UpdateUserDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
