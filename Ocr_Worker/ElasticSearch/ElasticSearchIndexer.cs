using Nest;

namespace Ocr_Worker;

public class ElasticSearchIndexer
{
    private readonly ElasticClient _client;

    public ElasticSearchIndexer(ElasticClient client)
    {
        _client = client;
    }

    public async Task<bool> IndexDocumentAsync(string id, string content)
    {
        var document = new
        {
            Id = id,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        var response = await _client.IndexDocumentAsync(document);

        if (response.IsValid)
        {
            Console.WriteLine($"[x] Document with ID {id} indexed successfully.");
            return true;
        }
        else
        {
            Console.WriteLine($"[!] Failed to index document with ID {id}. Error: {response.OriginalException.Message}");
            return false;
        }
    }
}