using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using goqur.shared.models;

namespace goqur.shared.services
{
    public class EnricherService : IEnricherService
    {
        public Task<List<Ayah>> Translate(List<Ayah> ayahs)
        {
            return Task.FromResult<List<Ayah>>(ayahs);
        }
    }
}
