using System;
using System.Threading.Tasks;
using WijkMeld.App.Model;
using WijkMeld.App.Model.Enums;

namespace WijkMeld.App.Services
{
    public interface IAuthenticationService
    {
        bool IsLoggedIn { get; }
        string? CurrentUserId { get; }
        UserRole CurrentUserRole { get; }

        Task InitializeAsync();
        bool IsUserLoggedIn();
        Task<bool> LoginAsync(LoginRequest request);
        Task<string> GetTokenAsync();
        Task<string?> GetUserIdAsync();
        Task<string?> GetUserRoleAsync();
        Task<bool> LoginAsGuestAsync();
        Task<bool> RegisterUserAsync(RegisterRequest request);
        Task LogoutAsync();
    }
}
