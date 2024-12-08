using Nest;
using RabbitMQ.Client;

namespace Ocr_Worker;

class Program
{
    static void Main(string[] args)
    {
        // var worker = new OcrWorker();
        // worker.Start();
        //
        // Console.WriteLine("OCR Worker is running. Press Ctrl+C to exit.");
        //
        // while (true)
        // {
        //     Thread.Sleep(1000);
        // }
        //
        
        Console.WriteLine("[x] Starting OCR and Indexing Workers...");

        // Setup RabbitMQ connection
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq_dms",
            UserName = "user",
            Password = "password"
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Setup Elasticsearch client
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var elasticClient = new ElasticClient(settings);

        // Start OCR Worker
        var ocrWorker = new OcrWorker(); // Assuming this is the OCR worker logic
        var ocrThread = new Thread(() =>
        {
            ocrWorker.Start(); // OCR logic that publishes to RabbitMQ
        });
        ocrThread.Start();

        // Start Indexing Worker
        var indexer = new ElasticSearchIndexer(elasticClient);
        var indexingWorker = new IndexingWorker(channel, indexer);
        var indexingThread = new Thread(() =>
        {
            indexingWorker.Start(); // Indexing logic that consumes from RabbitMQ
        });
        indexingThread.Start();

        Console.WriteLine("Workers are running. Press Ctrl+C to exit.");

        while (true)
        {
            Thread.Sleep(1000); // Keep the application alive
        }
    }
}