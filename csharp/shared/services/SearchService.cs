using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

namespace goqur.shared.services
{
    public class SearchService : ISearchService
    {
        private static SearchIndexClient _searchIndexClient = null;
        private static SearchClient _searchClient = null;
        private readonly string _searchEndpoint;
        private readonly string _searchKey;
        private readonly string _indexName;

        public SearchService(string searchEndpoint, string searchKey, string indexName)
        {
            this._searchEndpoint = searchEndpoint;
            this._searchKey = searchKey;
            this._indexName = indexName;
        }

        public async Task CreateIndex<T>() 
        {
            // Create the index using FieldBuilder.
            SearchIndex index = new SearchIndex(_indexName)
            {
                Fields = new FieldBuilder().Build(typeof(T)),
            };

            Console.WriteLine("Creating an index....");
            await GetSearchIndexClient().CreateIndexAsync(index);
        }

        public async Task DeleteIndex()
        {
            await GetSearchIndexClient().DeleteIndexAsync(_indexName);
        }

        public async Task UploadDocs<T>(List<T> docs)
        {
            // If index does not exist, auto-create itCreateIndex
            if (!GetSearchIndexClient().GetIndexes().Any(index => index.Name == _indexName)) 
            {
                SearchIndex index = new SearchIndex(_indexName)
                {
                    Fields = new FieldBuilder().Build(typeof(T)),
                };

                await CreateIndex<T>();
            }

            List<IndexDocumentsAction<T>> actions = new List<IndexDocumentsAction<T>>();
            foreach(T d in docs) 
            {
                actions.Add(IndexDocumentsAction.Upload(d));
            }

            IndexDocumentsBatch<T> batch = IndexDocumentsBatch.Create(actions.ToArray());
            IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
            await GetSearchClient().IndexDocumentsAsync<T>(batch, options);
        }

        private SearchIndexClient GetSearchIndexClient() 
        {
            if (_searchIndexClient == null)
            {
                // Create a service client
                Uri endpoint = new Uri(this._searchEndpoint);
                AzureKeyCredential credential = new AzureKeyCredential(this._searchKey);
                _searchIndexClient = new SearchIndexClient(endpoint, credential);
            }

            return _searchIndexClient;
        }

        private SearchClient GetSearchClient() 
        {
            if (_searchClient == null)
            {
                // Create a service client
                Uri endpoint = new Uri(this._searchEndpoint);
                AzureKeyCredential credential = new AzureKeyCredential(this._searchKey);
                _searchClient = new SearchClient(endpoint, _indexName, credential);
            }

            return _searchClient;
        }
    }
}

