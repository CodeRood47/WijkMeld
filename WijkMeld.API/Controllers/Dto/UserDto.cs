using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Controllers.Dto
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }

    
        public string Email { get; set; }
        public UserRole Role { get; set; }

        public List<Guid> IncidentIds { get; set; } = new List<Guid>();

    }

    public class CreateUserDto
    {
        [Required]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }
        [Required]
      
        public UserRole Role { get; set; } = UserRole.USER;
    }

    public class UpdateUserDto
    {
        [Required]
        public required string  UserName { get; set; }
        [Required]
        //[EmailAddress]
        public required string Email { get; set; }
    }
}
