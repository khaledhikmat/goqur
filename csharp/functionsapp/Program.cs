using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Configuration;
using goqur.shared.services;
using System;

namespace functionsapp
{
    public class Program
    {
        private static string AYAS_INDEX_NAME = "gq-ayahs-managed-index";
        private static string AYAS_QUEUE_NAME = "ayah-items";
        private static string SEARCH_SVC_ENDPOINT = "SEARCH_SVC_ENDPOINT";
        private static string SEARCH_SVC_API_KEY = "SEARCH_SVC_API_KEY";
        public static async Task Main(string [] args)
        {
            string searchEndpoint = GetEnvironmentVariable(SEARCH_SVC_ENDPOINT);
            string searchKey = GetEnvironmentVariable(SEARCH_SVC_API_KEY);
            string storageConnectionString = GetEnvironmentVariable("AzureWebJobsStorage");

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices( s => 
                {
                    // Inject functionapp-wide services
                    s.AddSingleton<ISearchService>(x => new SearchService(searchEndpoint, searchKey, AYAS_INDEX_NAME));
                    s.AddSingleton<INotificationService>(x => new NotificationService(storageConnectionString, AYAS_QUEUE_NAME));
                    s.AddSingleton<IEnricherService, EnricherService>();
                })
                .Build();

            await host.RunAsync();
        }

        private static string GetEnvironmentVariable(string key) 
        {
            return System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}
