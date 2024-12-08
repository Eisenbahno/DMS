using Nest;

namespace Ocr_Worker;

public class ElasticSearchClient
{
    private readonly ElasticClient _client;

    public ElasticSearchClient(string uri)
    {
        var settings = new ConnectionSettings(new Uri(uri))
            .DefaultIndex("ocr_results"); 
        _client = new ElasticClient(settings);
    }

    public ElasticClient GetClient()
    {
        return _client;
    }
}