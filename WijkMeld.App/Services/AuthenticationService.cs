using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model;
using Microsoft.Maui.Storage;

namespace WijkMeld.App.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private bool _isLoggedIn;
        private string? _currentUserId;

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set => _isLoggedIn = value;
            
        }

        public string? CurrentUserId => _currentUserId;

        public AuthenticationService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient"); 

            _isLoggedIn = false;
            System.Diagnostics.Debug.WriteLine("AuthenticationService constructor voltooid.");

        }

          public async Task InitializeAsync()
        {
            System.Diagnostics.Debug.WriteLine("AuthenticationService InitializeAsync gestart.");
            var token = await SecureStorage.GetAsync("jwt_token");
            var userId = await SecureStorage.GetAsync("user_id");


            _isLoggedIn = !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId);
            _currentUserId = userId;
            System.Diagnostics.Debug.WriteLine($"AuthenticationService initialisatie voltooid. IsLoggedIn: {IsLoggedIn}");
        }

        public bool IsUserLoggedIn()
        {
            return IsLoggedIn && !string.IsNullOrEmpty(CurrentUserId);
        }

        // dit kan waarschijnlijk weg
        //private async Task<bool> CheckStoredLoginStatusAsync()
        //{


        //    try
        //    {
        //        var token = await SecureStorage.GetAsync("jwt_token");
        //        var userId = await SecureStorage.GetAsync("user_id");
        //        return !string.IsNullOrEmpty(token);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception if SecureStorage access fails
        //        System.Diagnostics.Debug.WriteLine($"Error checking stored login status: {ex.Message}");
        //        return false;
        //    }
        //}

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
                    System.Diagnostics.Debug.WriteLine("API Login mislukt: Token of UserId is null in respons.");
                    return false;
                }

                await SecureStorage.SetAsync("jwt_token", result.Token);
                await SecureStorage.SetAsync("user_id", result.UserId);
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
                System.Diagnostics.Debug.WriteLine($"API Login exception: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetTokenAsync()
        {
            return await SecureStorage.GetAsync("jwt_token");
        }

        public async Task<string?> GetUserIdAsync()
        {
            
            if (string.IsNullOrEmpty(_currentUserId))
            {
                _currentUserId = await SecureStorage.GetAsync("user_id");
            }
            return _currentUserId;
        }


        public async Task LogoutAsync()
        {
            SecureStorage.Remove("jwt_token");
            SecureStorage.Remove("user_id");
            IsLoggedIn = false;
            _currentUserId = null;
        }
        
    }
}
