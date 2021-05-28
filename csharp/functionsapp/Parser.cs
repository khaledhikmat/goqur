using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using goqur.shared.models;
using goqur.shared.services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using goqur.shared.utils;

namespace goqur.functions
{
    public class Parser
    {
        private static int AYAS_CHUNK_SIZE = 100;
        private ISearchService _searchService = null;
        private IEnricherService _enricherService = null;
        private INotificationService _notificationService = null;
        private ILogger<Parser> _logger = null;

        public Parser(ISearchService searchService, IEnricherService enrService, INotificationService notService, ILogger<Parser> logger) 
        {
            _searchService = searchService;
            _enricherService = enrService;
            _notificationService = notService;
            _logger = logger;
        }

        [Function(nameof(Parser))]
        public async Task Run(
            [BlobTrigger("files/{name}", Connection = "AzureWebJobsStorage")] string blobCsv, string name,
            FunctionContext context)
        {
            //_logger.LogInformation($"Parser trigger function Processed blob Name: {name} \n Data: {blobCsv}");
            List<Ayah> ayahs = Ayah.ParseToList(blobCsv, true, true, ",");
            //TODO: After parsing, run in parallel to enrich and upload by chunking the data to 100 ayahs in each chunk
            //WARNING: Not sure, from a Functions point of view, whether to run in parallel like this or call different Functions
            //Suggestions:
            // 0. It does not look it is safe to do this in a Function (https://stackoverflow.com/questions/47375598/calling-hundreds-of-azure-functions-in-parallel) 
            // as there could be timeouts and consumption issues 
            // 1. Enqueue each document to a queue so it can trigger a process that will process it one at a time
            // 2. Durable functions handle this properly: https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-cloud-backup?tabs=csharp
            // 3. But Durable functions are not supported in .NET 5.x
            List<Task> tasksList = new List<Task>();
            _logger.LogInformation($"Parsed {ayahs.Count} ayahs....");
            List<List<Ayah>> chunks = ayahs.ChunkBy(AYAS_CHUNK_SIZE);
            int index = 0;
            foreach (List<Ayah> chunk in chunks)
            {
                _logger.LogInformation($"Enqueuing ayahs chunk: {index}");
                var task = Task.Run(() => ProcessAyahsAsync(chunk));
                index++;
                tasksList.Add(task);
            }

            await Task.WhenAll(tasksList);
            _logger.LogInformation($"Processed all ayah chunks: {chunks.Count}");
        }

        private async Task ProcessAyahsAsync(List<Ayah> ayahs) 
        {
            _logger.LogInformation($"Processing chunk with {ayahs.Count} ayahs");
            await _notificationService.EnqueueAyahsList(ayahs);
        }
   }
}
