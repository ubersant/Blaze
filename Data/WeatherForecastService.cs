using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph.Extensions;
using System.Net.Http.Headers;

namespace AdBlaze.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray());
        }


        public class AppSettingsFile
        {
            public AppSettings AppSettings { get; set; }

            public static AppSettings ReadFromJsonFile()
            {
                IConfigurationRoot Configuration;

                Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                return Configuration.Get<AppSettingsFile>().AppSettings;
            }
        }

        public class AppSettings
        {
            [JsonProperty(PropertyName = "TenantId")]
            public string TenantId { get; set; }

            [JsonProperty(PropertyName = "AppId")]
            public string AppId { get; set; }

            [JsonProperty(PropertyName = "ClientSecret")]
            public string ClientSecret { get; set; }

            [JsonProperty(PropertyName = "B2cExtensionAppClientId")]
            public string B2cExtensionAppClientId { get; set; }

            [JsonProperty(PropertyName = "UsersFileName")]
            public string UsersFileName { get; set; }

        }

        public async Task<User[]> GetUsersAsync()
        {
            // Read application settings from appsettings.json (tenant ID, app ID, client secret, etc.)
            AppSettings config = AppSettingsFile.ReadFromJsonFile();

            // Initialize the client credential auth provider
            //IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            //    .Create(config.AppId)
            //    .WithTenantId(config.TenantId)
            //    .WithClientSecret(config.ClientSecret)
            //    .Build();

         //ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials

            GraphServiceClient graphServiceClient =
                     new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => {


                         var confidentialClient = ConfidentialClientApplicationBuilder
    .Create("f0632886-04ea-484a-950d-454b05d9a65d").WithTenantId("AdBlaze.onmicrosoft.com")
    //.WithAuthority($"https://login.microsoftonline.com/$AdBlaze.onmicrosoft.com/v2.0")
    .WithClientSecret("L928jSUhN-6HhDebhvtf4.NVl~1d0ENNl~")
    .Build();
                         var scopes = new string[] { "https://graph.microsoft.com/.default" };


                         // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                         var authResult = await confidentialClient
            .AcquireTokenForClient(scopes)
            .ExecuteAsync();

        // Add the access token in the Authorization header of the API request.
             requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
    })
    );

            // Make a Microsoft Graph API query
            var users = await graphServiceClient.Users.Request().GetAsync();

            var rng = new Random();
            return users.Select(index => new User
            {
                Date = DateTime.Now.Date,
                Name = index.DisplayName,
                Email = index.Mail
            }).ToArray();


        }
    }
}
