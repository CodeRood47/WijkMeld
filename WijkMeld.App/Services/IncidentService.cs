using System.Net.Http.Headers;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model;

namespace WijkMeld.App.Services
{
    public class IncidentService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationService _authenticationService;



        public IncidentService(HttpClient httpClient, AuthenticationService authenticationService)
        {
            _httpClient = httpClient;
            _authenticationService = authenticationService;
        }

        public async Task<List<Incident>?> GetUserIncidentsAsync()
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan incidenten niet ophalen.");
                return null;
            }

            var token = await _authenticationService.GetTokenAsync();
            var userId = await _authenticationService.GetUserIdAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: JWT token niet gevonden, kan incidenten niet ophalen.");

                return null;
            }

            if (string.IsNullOrEmpty(userId))
            {
            }


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync($"api/Users/{userId}/Incidents");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Incident>>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: Fout bij het ophalen van incidenten.");

                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await _authenticationService.LogoutAsync();
                }
                return null;


            }



        }
    }
}
