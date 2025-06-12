using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkMeld.App.Configuration
{
    public static class HttpClientConfigurator
    {
        public static void Configure(IServiceCollection services, string baseUrl)
        {
            services.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });
#endif
            ; 
        }
    }
}
