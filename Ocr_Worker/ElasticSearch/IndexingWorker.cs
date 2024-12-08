namespace Ocr_Worker;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class IndexingWorker
{
    private readonly IModel _channel;
    private readonly ElasticSearchIndexer _indexer;

    public IndexingWorker(IModel channel, ElasticSearchIndexer indexer)
    {
        _channel = channel;
        _indexer = indexer;

        // Declare the queue
        _channel.QueueDeclare(queue: "ocr_result_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Start()
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var parts = message.Split('|');

            if (parts.Length == 2)
            {
                var id = parts[0];
                var content = parts[1];

                Console.WriteLine($"[x] Received OCR result for ID: {id}");

                // Index the content in Elasticsearch
                var success = await _indexer.IndexDocumentAsync(id, content);

                if (success)
                {
                    Console.WriteLine($"[x] Successfully indexed document with ID: {id}");
                }
                else
                {
                    Console.WriteLine($"[!] Failed to index document with ID: {id}");
                }
            }
            else
            {
                Console.WriteLine("[!] Invalid message format.");
            }
        };

        _channel.BasicConsume(queue: "ocr_result_queue", autoAck: true, consumer: consumer);
    }
}