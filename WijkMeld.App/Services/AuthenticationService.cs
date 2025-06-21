using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model;
using Microsoft.Maui.Storage;
using WijkMeld.App.Model.Enums;
using System.Diagnostics;

namespace WijkMeld.App.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private bool _isLoggedIn;
        private string? _currentUserId;
        private UserRole _currentUserRole;

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set => _isLoggedIn = value;
            
        }

        public string? CurrentUserId => _currentUserId;
        public UserRole CurrentUserRole => _currentUserRole;

        public AuthenticationService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _currentUserRole = UserRole.GUEST;
            _isLoggedIn = false;
            Debug.WriteLine("AuthenticationService constructor voltooid.");

        }

          public async Task InitializeAsync()
        {
            Debug.WriteLine("AuthenticationService InitializeAsync gestart.");
            var token = await SecureStorage.GetAsync("jwt_token");
            var userId = await SecureStorage.GetAsync("user_id");
            var userRole = await SecureStorage.GetAsync("user_role");


            Debug.WriteLine($"[DEBUG] Token: {token}");
            Debug.WriteLine($"[DEBUG] UserId: {userId}");
            Debug.WriteLine($"[DEBUG] UserRole (raw): {userRole}");

            _isLoggedIn = !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId);
            _currentUserId = userId;

            if (!string.IsNullOrEmpty(userRole) && Enum.TryParse(userRole, out UserRole parsedRole))
            {
                _currentUserRole = parsedRole;
            }
            else
            {
                _currentUserRole = UserRole.GUEST; 
            }
            
            System.Diagnostics.Debug.WriteLine($"AuthenticationService initialisatie voltooid. IsLoggedIn: {IsLoggedIn}");
        }

        public bool IsUserLoggedIn()
        {
            return IsLoggedIn && !string.IsNullOrEmpty(CurrentUserId);
        }

      

        public async Task<bool> LoginAsync(LoginRequest request)
        {


//            // --- START SIMULATIE VOOR DEBUG MODUS ---
//#if DEBUG
//            // Simuleer een succesvolle login voor specifieke credentials in debug modus
//            if (request.Email == "test@test.com" && request.Password == "password")
//            {
//                await Task.Delay(1000); // Simuleer netwerkvertraging
//                await SecureStorage.SetAsync("jwt_token", "dummy_jwt_token_for_debug");
//                IsLoggedIn = true;
//                System.Diagnostics.Debug.WriteLine("DEBUG: Gesimuleerde login succesvol.");
//                return true;
//            }
//            else if (request.Email == "fail@example.com")
//            {
//                await Task.Delay(500); // Simuleer netwerkvertraging
//                IsLoggedIn = false;
//                System.Diagnostics.Debug.WriteLine("DEBUG: Gesimuleerde login mislukt voor 'fail@example.com'.");
//                return false;
//            }
//#endif


            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Authentication/login", request);
                if (!response.IsSuccessStatusCode)
                {
                    IsLoggedIn = false;
                    _currentUserId = null;
                    SecureStorage.Remove("jwt_token");
                    SecureStorage.Remove("user_id");
                    SecureStorage.Remove("user_role");
                    System.Diagnostics.Debug.WriteLine($"API Login mislukt: {response.StatusCode}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Token is null || result?.UserId is null)
                {
                    IsLoggedIn = false;
                    _currentUserId = null;
                    SecureStorage.Remove("jwt_token");
                    SecureStorage.Remove("user_id");
                    SecureStorage.Remove("user_role");
                    System.Diagnostics.Debug.WriteLine("API Login mislukt: Token of UserId is null in respons.");
                    return false;
                }

                await SecureStorage.SetAsync("jwt_token", result.Token);
                await SecureStorage.SetAsync("user_id", result.UserId);
                await SecureStorage.SetAsync("user_role", result.UserRole);
                _currentUserId = result.UserId;
                IsLoggedIn = true;
                System.Diagnostics.Debug.WriteLine("API Login succesvol.");
                return true;
            }
            catch (Exception ex)
            {
                IsLoggedIn = false;
                _currentUserId = null;
                SecureStorage.Remove("jwt_token");
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_role");
                System.Diagnostics.Debug.WriteLine($"API Login exception: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetTokenAsync()
        {
            var token = await SecureStorage.GetAsync("jwt_token");
            Debug.WriteLine($"AuthenticationService: GetTokenAsync opgeroepen. Token is: {(!string.IsNullOrEmpty(token) ? "Aanwezig" : "NIET Aanwezig")}");

            return token;
        }

        public async Task<string?> GetUserIdAsync()
        {
            
            if (string.IsNullOrEmpty(_currentUserId))
            {
                _currentUserId = await SecureStorage.GetAsync("user_id");
            }
            return _currentUserId;
        }

        public async Task<string?> GetUserRoleAsync()
        {
            return await SecureStorage.GetAsync("user_role");
        }


        public async Task<bool> LoginAsGuestAsync()
        {
            try
            {
                await SecureStorage.SetAsync("jwt_token", "ANONYMOUS_GUEST_TOKEN"); 
                await SecureStorage.SetAsync("user_id", Guid.NewGuid().ToString()); 
                await SecureStorage.SetAsync("user_role", UserRole.GUEST.ToString());

                _currentUserId = await SecureStorage.GetAsync("user_id");
                _currentUserRole = UserRole.GUEST;
                IsLoggedIn = true;

                Debug.WriteLine($"AuthenticationService: Anonieme login succesvol. Rol: {CurrentUserRole}, UserId: {CurrentUserId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AuthenticationService: Fout bij anonieme login: {ex.Message}");
                // Zorg ervoor dat de status correct is bij een fout
                IsLoggedIn = false;
                _currentUserId = null;
                _currentUserRole = UserRole.GUEST;
                SecureStorage.Remove("jwt_token");
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_role");
                return false;
            }
        }

        public async Task<bool> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;

                string registerPayload = System.Text.Json.JsonSerializer.Serialize(request);
                Debug.WriteLine($"[MAUI-DEBUG] AuthenticationService: Verzenden van RegisterUserAsync request. JSON payload: {registerPayload}");

                var response = await _httpClient.PostAsJsonAsync("api/Users", request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API Registratie mislukt: {response.StatusCode}. Response: {errorContent}");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine("API Registratie succesvol.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij registratie: {ex.Message}");
                throw;
            }
        }
        public async Task LogoutAsync()
        {
            await Task.Run(() =>
            {
                SecureStorage.Remove("jwt_token");
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_role");

            });

            IsLoggedIn = false;
            _currentUserId = null;
        }
        
    }
}
