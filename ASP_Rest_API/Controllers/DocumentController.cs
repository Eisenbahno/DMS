using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ASP_Rest_API.DTO;
using AutoMapper;
using DAL.Entities;
using RabbitMQ.Client;
using System.Text;

namespace ASP_Rest_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public DocumentController(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;

            // Stelle die Verbindung zu RabbitMQ her
            var factory = new ConnectionFactory() { HostName = "rabbitmq_dms", UserName = "user", Password = "password" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            // Deklariere die Queue
            _channel.QueueDeclare(queue: "file_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var client = _httpClientFactory.CreateClient("DAL");
            var response = await client.GetAsync("/api/document"); // Endpunkt des DAL

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<DocumentItem>>();
                var sortedItems = items.OrderBy(item => item.Id);
                var dtoItems = _mapper.Map<IEnumerable<DocumentItemDto>>(sortedItems);
                return Ok(dtoItems);
            }

            return StatusCode((int)response.StatusCode, "Error retrieving Todo items from DAL");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = _httpClientFactory.CreateClient("DAL");
            var response = await client.GetAsync($"/api/document/{id}");

            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<DocumentItem>();
                var dtoItem = _mapper.Map<DocumentItemDto>(item);
                if (item != null)
                {
                    return Ok(dtoItem);
                }
                return NotFound();
            }

            return StatusCode((int)response.StatusCode, "Error retrieving Todo item from DAL");
        }

        [HttpPost]
        public async Task<IActionResult> Create(DocumentItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("DAL");
            var item = _mapper.Map<DocumentItem>(itemDto);
            var response = await client.PostAsJsonAsync("/api/document", item);

            if (response.IsSuccessStatusCode)
            {
                return CreatedAtAction(nameof(GetById), new { id = item.Id }, itemDto);
            }

            return StatusCode((int)response.StatusCode, "Error creating Documnt in DAL");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DocumentItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != itemDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var client = _httpClientFactory.CreateClient("DAL");
            var item = _mapper.Map<DocumentItem>(itemDto);
            var response = await client.PutAsJsonAsync($"/api/document/{id}", item);

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            return StatusCode((int)response.StatusCode, "Error updating Document in DAL");
        }

        [HttpPut("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile? document)
        {
            if (document == null || document.Length == 0)
            {
                return BadRequest("Keine Datei hochgeladen.");
            }

            // Speichere die Datei
            var filePath = Path.Combine("uploads", document.FileName);
            Directory.CreateDirectory("uploads"); // Sicherstellen, dass der Ordner existiert

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(stream);
            }

            // Hole den Task vom DAL
            var client = _httpClientFactory.CreateClient("DAL");
            var response = await client.GetAsync($"/api/document/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound($"Fehler beim Abrufen des Documents mit ID {id}");
            }

            var todoItem = await response.Content.ReadFromJsonAsync<DocumentItemDto>();
            if (todoItem == null)
            {
                return NotFound($"Task mit ID {id} nicht gefunden.");
            }

            var todoItemDto = _mapper.Map<DocumentItem>(todoItem);
            todoItemDto.Name = document.FileName;

            // Aktualisiere den Task im DAL
            var updateResponse = await client.PutAsJsonAsync($"/api/document/{id}", todoItemDto);
            if (!updateResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)updateResponse.StatusCode, $"Fehler beim Speichern des Dateinamens {id}");
            }

            // Nachricht an RabbitMQ senden
            try
            {
                SendToMessageQueue(id, filePath); // Nachricht mit ID und Pfad senden
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fehler beim Senden der Nachricht an RabbitMQ: {ex.Message}");
            }

            return Ok(new { message = $"Dateiname {document.FileName} für Task {id} erfolgreich gespeichert." });
        }
        
        private void SendToMessageQueue(int id, string filePath)
        {
            // Nachricht im richtigen Format erstellen
            var message = $"{id}|{filePath}"; // Beispiel: "1|/app/uploads/example.pdf"

            // Nachricht in Bytes umwandeln
            var body = Encoding.UTF8.GetBytes(message);

            // Nachricht an RabbitMQ senden
            _channel.BasicPublish(exchange: "", routingKey: "file_queue", basicProperties: null, body: body);
            Console.WriteLine($@"[x] Sent: {message}");
        }

        [HttpPost("consume-ocr-results")]
        public IActionResult ConsumeAndSaveResults()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq_dms",
                UserName = "user",
                Password = "password"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Stelle sicher, dass die Queue existiert
            channel.QueueDeclare(queue: "ocr_result_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var result = channel.BasicGet(queue: "ocr_result_queue", autoAck: true);

            if (result == null)
            {
                return NotFound("Keine OCR-Ergebnisse in der Warteschlange gefunden.");
            }

            // Nachricht parsen
            var message = Encoding.UTF8.GetString(result.Body.ToArray());
            var parts = message.Split('|');

            if (parts.Length != 2)
            {
                return BadRequest("Ungültiges Nachrichtenformat in der Warteschlange.");
            }

            var id = parts[0];
            var extractedText = parts[1];

            // TXT-Datei speichern
            var outputPath = Path.Combine("ocr_results", $"{id}.txt");
            Directory.CreateDirectory("ocr_results"); // Sicherstellen, dass der Ordner existiert

            System.IO.File.WriteAllText(outputPath, extractedText);

            Console.WriteLine($"[x] Ergebnis für ID {id} als {outputPath} gespeichert.");

            return Ok(new { message = $"OCR-Ergebnis gespeichert unter {outputPath}", id });
        }

        
        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("DAL");
            var response = await client.DeleteAsync($"/api/document/{id}");

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            return StatusCode((int)response.StatusCode, "Error deleting Todo item from DAL");
        }
    }
}
