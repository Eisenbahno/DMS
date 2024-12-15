using Nest;
using System;
using System.Threading.Tasks;

namespace Ocr_Worker
{
    public class ElasticSearchClient
    {
        private readonly ElasticClient _client;

        public ElasticSearchClient(string uri)
        {
            var settings = new ConnectionSettings(new Uri(uri))
                .DefaultIndex("ocr_results"); 
            _client = new ElasticClient(settings);
        }

        public async Task IndexDocumentAsync<T>(T document) where T : class
        {
            var response = await _client.IndexDocumentAsync(document);

            Console.WriteLine(response.IsValid
                ? $"Document indexed successfully. ID: {response.Id}"
                : $"Failed to index document. Error: {response.ServerError?.Error?.Reason}");
        }


        public async Task SearchDocumentsAsync(string query)
        {
            var searchResponse = await _client.SearchAsync<object>(s => s
                .Query(q => q.QueryString(d => d.Query(query))));

            if (searchResponse.IsValid)
            {
                Console.WriteLine("Search results:");
                foreach (var hit in searchResponse.Hits)
                {
                    Console.WriteLine($"ID: {hit.Id}, Source: {hit.Source}");
                }
            }
            else
            {
                Console.WriteLine($"Search failed. Error: {searchResponse.ServerError}");
            }
        }

        public async Task GetDocumentByIdAsync(string id)
        {
            var response = await _client.GetAsync<object>(id);

            if (response.Found)
            {
                Console.WriteLine($"Document found: ID: {id}, Source: {response.Source}");
            }
            else
            {
                Console.WriteLine($"Document with ID {id} not found.");
            }
        }

        public async Task DeleteDocumentByIdAsync(string id)
        {
            var response = await _client.DeleteAsync<object>(id);

            if (response.IsValid)
            {
                Console.WriteLine($"Document with ID {id} deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to delete document. Error: {response.ServerError}");
            }
        }
    }
}
