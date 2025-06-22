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
using System.Net;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.Storage;

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

         public Uri? GetApiBaseAddress()
        {

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return new Uri("http://10.0.2.2:5079");
            }
            else
            {
                return new Uri("https://localhost:7226");
            }
        }

        public async Task<List<Incident>?> GetUserIncidentsAsync()
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan incidenten niet ophalen.");
                return null;
            }

            var token = await _authenticationService.GetTokenAsync();
            var userId = await _authenticationService.GetUserIdAsync();
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("IncidentService: JWT token niet gevonden, kan incidenten niet ophalen.");

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
                Debug.WriteLine("IncidentService: Fout bij het ophalen van incidenten.");

                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await _authenticationService.LogoutAsync();
                }
                return null;


            }

        }

        //public async Task<List<Incident>?> GetAllIncidentsAsync()
        //{
        //    var token = await _authenticationService.GetTokenAsync();


        //    try
        //    {

        //        var response = await _httpClient.GetAsync("Api/Incidents"); 
        //        response.EnsureSuccessStatusCode();

        //        return await response.Content.ReadFromJsonAsync<List<Incident>>();
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        Debug.WriteLine($"IncidentService: Fout bij ophalen ALLE incidenten: {ex.Message}");

        //        if (ex.StatusCode == HttpStatusCode.Unauthorized)
        //        {
        //            // Alleen uitloggen als het een ingelogde gebruiker betreft, niet voor gasten
        //            if (_authenticationService.IsUserLoggedIn())
        //            {
        //                await _authenticationService.LogoutAsync();
        //            }
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"IncidentService: Algemene fout bij ophalen ALLE incidenten: {ex.Message}");
        //        return null;
        //    }
        //}

        public async Task<List<Incident>?> GetAllIncidentsAsync()
        {
            var token = await _authenticationService.GetTokenAsync();
          
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null; 
            }


            try
            {
               
                var response = await _httpClient.GetAsync("Api/Incidents");
                response.EnsureSuccessStatusCode();

          
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jsonContent) || jsonContent.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("IncidentService: Lege of null respons ontvangen voor ALLE incidenten.");
                    return new List<Incident>();
                }

                var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
                return incidents ?? new List<Incident>(); 
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncidentService: Fout bij ophalen ALLE incidenten: {ex.StatusCode} - {ex.Message}");

                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
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







        public async Task<Guid> CreateIncidentAsync(CreateIncidentRequest request) 
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan incident niet aanmaken.");
                return Guid.Empty; 
            }

            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("IncidentService: JWT token niet gevonden, kan incident niet aanmaken.");
                return Guid.Empty;
            }

          
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Incidents", request);
                response.EnsureSuccessStatusCode(); // Gooit een uitzondering voor 4xx/5xx statuscodes

                var result = await response.Content.ReadFromJsonAsync<CreateIncidentResponse>();

                if (result != null && result.Id != Guid.Empty)
                {
                    Debug.WriteLine($"Incident succesvol aangemaakt. Ontvangen ID: {result.Id}");
                    return result.Id; // return received guid from the response
                }
                else
                {
                    Debug.WriteLine("Incident aanmaken mislukt: ID ontbreekt in de respons of is leeg.");
                    return Guid.Empty; // return Guid.Empty if the ID is not present or empty
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"IncidentService: HttpRequestException bij aanmaken incident: {ex.Message}");

                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await _authenticationService.LogoutAsync();
                }
                return Guid.Empty; // Retourneer Guid.Empty bij een HttpRequestException
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncidentService: Onverwachte fout bij aanmaken incident: {ex.Message}");
                return Guid.Empty; // Retourneer Guid.Empty bij andere uitzonderingen
            }
        }

        public async Task<Incident?> GetIncidentByIdAsync(Guid incidentId)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan incident niet ophalen.");
                return null;
            }
            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("IncidentService: JWT token niet gevonden, kan incident niet ophalen.");
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
                Debug.WriteLine($"IncidentService: Fout bij het ophalen van incident met ID {incidentId}: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await _authenticationService.LogoutAsync();
                }
                return null;
            }
        }
        public async Task<bool> UploadIncidentPhotoAsync(IncidentPhoto photo, Guid incidentId)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan foto niet uploaden.");
                return false;
            }

            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("IncidentService: JWT token niet gevonden, kan foto niet uploaden.");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (photo.PhotoStream == null)
            {
                Debug.WriteLine("IncidentService: PhotoStream is null, kan foto niet uploaden.");
                return false;
            }

            try
            {
              
                var requestUrl = $"Api/Incidents/{incidentId}/photo"; 

                using (var content = new MultipartFormDataContent())
                {
                    
                    content.Add(new StreamContent(photo.PhotoStream), "file", photo.FileName);

                    var response = await _httpClient.PostAsync(requestUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"IncidentService: Fout bij uploaden foto voor incident {incidentId} (HTTP {response.StatusCode}): {errorContent}");

                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await _authenticationService.LogoutAsync();
                        }
                        return false; 
                    }

                    Debug.WriteLine($"Foto '{photo.FileName}' succesvol geüpload voor incident {incidentId}.");
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
         
                Debug.WriteLine($"IncidentService: HttpRequestException bij uploaden foto voor incident {incidentId}: {ex.Message}");

                if (ex.StatusCode == HttpStatusCode.Unauthorized) 
                {
                    await _authenticationService.LogoutAsync();
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncidentService: Onverwachte fout bij uploaden foto: {ex.Message}");
                return false;
            }
            finally
            {
               
                photo.PhotoStream?.Dispose();
            }
        }
        public async Task<bool> UpdateIncidentStatusAndPriorityAsync(Guid incidentId, Status? newStatus, Priority? newPriority, string? note)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                Debug.WriteLine("IncidentService: Gebruiker is niet ingelogd, kan incidentstatus niet bijwerken.");
                return false;
            }

            var token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("IncidentService: JWT token niet gevonden, kan incidentstatus niet bijwerken.");
                return false;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var requestBody = new UpdateIncidentRequest// Let op: Deze DTO moet overeenkomen met de API's DTO
                {
                    NewStatus = newStatus,
                    Note = note,
                    NewPriority = newPriority

                };

                Debug.WriteLine($"IncidentService: Verzenden PUT verzoek naar api/Incidents/{incidentId}/status met payload: {JsonSerializer.Serialize(requestBody)}");

                var response = await _httpClient.PutAsJsonAsync($"api/Incidents/{incidentId}/status", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"IncidentService: Fout bij bijwerken status/prioriteit (HTTP {response.StatusCode}): {errorContent}");
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await _authenticationService.LogoutAsync();
                    }
                    return false;
                }

                Debug.WriteLine($"IncidentService: Status/prioriteit van incident {incidentId} succesvol bijgewerkt.");
                return true;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"IncidentService: HttpRequestException bij bijwerken status/prioriteit voor incident {incidentId}: {ex.Message}");
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await _authenticationService.LogoutAsync();
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncidentService: Onverwachte fout bij bijwerken status/prioriteit voor incident {incidentId}: {ex.Message}");
                return false;
            }
        }
    }
}
