using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace WijkMeld.App.Model
{
    public class RegisterRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("passwordHash")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public int Role { get; set; }

        public RegisterRequest(string userName, string passwordHash, string email, int role = 0)
        {
            UserName = userName;
            Email = email;
            Password = passwordHash;
            Role = role; // Default role, is guest
        }
        public RegisterRequest() { } 
    }
}
