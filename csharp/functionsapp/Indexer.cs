using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using goqur.shared.models;
using goqur.shared.services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace goqur.functions
{
    public class Indexer
    {
        private ISearchService _searchService = null;
        private IEnricherService _enricherService = null;
        private ILogger<Indexer> _logger = null;

        public Indexer(ISearchService searchService, IEnricherService enrService, ILogger<Indexer> logger) 
        {
            _searchService = searchService;
            _enricherService = enrService;
            _logger = logger;
        }

        [Function(nameof(Indexer))]
        public async Task Run([QueueTrigger("ayah-items", Connection = "AzureWebJobsStorage")] string message,
            FunctionContext context)
        {
            //_logger.LogInformation($"Processing {message}");
            List<Ayah> ayahs = JsonConvert.DeserializeObject<List<Ayah>>(message);
            // _logger.LogInformation($"Processing {ayahs.Count} ayahs from the `ayah-items` queue");
            // foreach (Ayah ayah in ayahs) 
            // {
            //     _logger.LogInformation($"Indexer - ID: {ayah.Id} - ayah number: {ayah.AyahNumber} - english nouns: {ayah.EnglishNouns.Count} - english concepts: {ayah.EnglishConcepts.Count}");
            // }

            //WARNING: Add additional enrichments if needed 
            ayahs = await _enricherService.Translate(ayahs);
            await _searchService.UploadDocs<Ayah>(ayahs);
            _logger.LogInformation($"Done processing ayahs");
        }
    }
}
