using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.Maui.Model;

namespace WijkMeld.Maui.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;

        public bool IsLoggedIn { get; private set; }

        public AuthenticationService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            IsLoggedIn = CheckStoredLoginStatusAsync().Result;
        }



        public bool IsUserLoggedIn()
        {
            return IsLoggedIn;
        }


        private async Task<bool> CheckStoredLoginStatusAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("jwt_token");
                return !string.IsNullOrEmpty(token);
            }
            catch (Exception ex)
            {
                // Log the exception if SecureStorage access fails
                System.Diagnostics.Debug.WriteLine($"Error checking stored login status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Authentication/login", request);
                if (!response.IsSuccessStatusCode)
                {
                    IsLoggedIn = false;
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Token is null)
                {
                    IsLoggedIn = false;
                    return false;
                }

                await SecureStorage.SetAsync("jwt_token", result.Token);
                IsLoggedIn = true;
                return true;
            }
            catch (Exception ex)
            {
                IsLoggedIn = false;
                return false;
            }
        }

        public async Task<string> GetTokenAsync()
        {
            return await SecureStorage.GetAsync("jwt_token");
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Remove("jwt_token");
            IsLoggedIn = false;
        }
    }
}
