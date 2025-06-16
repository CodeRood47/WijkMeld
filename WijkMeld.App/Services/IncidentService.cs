using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Model;
using WijkMeld.App.Model.Enums;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Diagnostics;

namespace WijkMeld.App.Services
{
    public class IncidentService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationService _authenticationService;



        public IncidentService(IHttpClientFactory httpClientFactory, AuthenticationService authenticationService)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
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

        public async Task<List<Incident>?> GetAllIncidentsAsync()
        {
            var token = await _authenticationService.GetTokenAsync();
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            try
            {
                // Pas dit endpoint aan naar het daadwerkelijke API endpoint voor alle incidenten
                // Bijvoorbeeld: "api/Incidents/all" of gewoon "api/Incidents" (als POST/GET zonder ID)
                var response = await _httpClient.GetAsync("Api/Incidents"); // Aanname: GET /api/Incidents geeft alle incidenten
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Incident>>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncidentService: Fout bij ophalen ALLE incidenten: {ex.Message}");

                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Alleen uitloggen als het een ingelogde gebruiker betreft, niet voor gasten
                    if (_authenticationService.IsUserLoggedIn())
                    {
                        await _authenticationService.LogoutAsync();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncidentService: Algemene fout bij ophalen ALLE incidenten: {ex.Message}");
                return null;
            }
        }






        public async Task<bool> CreateIncidentAsync(CreateIncidentRequest request)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan incident niet aanmaken.");
                return false;
            }
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: JWT token niet gevonden, kan incident niet aanmaken.");
                return false;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Incidents", request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncidentService: Fout bij het aanmaken van incident: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await _authenticationService.LogoutAsync();
                }
                return false;
            }
        }

        public async Task<Incident?> GetIncidentByIdAsync(Guid incidentId)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan incident niet ophalen.");
                return null;
            }
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: JWT token niet gevonden, kan incident niet ophalen.");
                return null;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var response = await _httpClient.GetAsync($"Api/Incidents/{incidentId}");
                Debug.WriteLine($"IncidentService: Aanroep GET /api/Incidents/{incidentId} met token {token}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Incident>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncidentService: Fout bij het ophalen van incident met ID {incidentId}: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await _authenticationService.LogoutAsync();
                }
                return null;
            }
        }
    }
}
