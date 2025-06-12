using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace WijkMeld.App.Services
{
    public partial class GeolocationService
    {
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                if (status == PermissionStatus.Granted)
                {
                    var location = await Geolocation.GetLastKnownLocationAsync();

                    if (location == null)
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                        location = await Geolocation.GetLocationAsync(request);
                    }

                    if (location != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"GeolocationService: Locatie opgehaald - Lat: {location.Latitude}, Lng: {location.Longitude}");
                        return location;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("GeolocationService: Geen locatie beschikbaar (GetLocationAsync retourneerde null).");
                    }


                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("GeolocationService: Locatiepermissies niet verleend.");
                }

            }
            catch (FeatureNotSupportedException fnsEx)
            {
                System.Diagnostics.Debug.WriteLine($"GeolocationService: Locatie niet ondersteund op dit apparaat: {fnsEx.Message}");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // :ocation not enabled on the device
                System.Diagnostics.Debug.WriteLine($"GeolocationService: Locatie uitgeschakeld op apparaat: {fneEx.Message}");
            }
            catch (PermissionException pEx)
            {
                //location permission was denied
                System.Diagnostics.Debug.WriteLine($"GeolocationService: Locatiepermissie geweigerd: {pEx.Message}");
            }
            catch (Exception ex)
            {
                //general errors
                System.Diagnostics.Debug.WriteLine($"GeolocationService: Algemene fout bij ophalen locatie: {ex.Message}");
            }

            return null;

        }
    }
}
