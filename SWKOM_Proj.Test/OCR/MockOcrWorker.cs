using System.Text;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SWKOM_Proj.Test.OCR;

public class MockOcrWorker
{
    [Fact]
    public void ShouldConsumeMessageFromQueue()
    {
        // Arrange
        var mockChannel = new Mock<IModel>();
        var message = "5|This is the OCR result";
        var eventArgs = new BasicDeliverEventArgs
        {
            Body = Encoding.UTF8.GetBytes(message)
        };

        var consumer = new EventingBasicConsumer(mockChannel.Object);
        string processedMessage = null;

        consumer.Received += (model, ea) =>
        {
            processedMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
        };

        // Act
        consumer.HandleBasicDeliver(
            consumerTag: "",
            deliveryTag: 1,
            redelivered: false,
            exchange: "",
            routingKey: "",
            properties: null,
            body: eventArgs.Body
        );

        // Assert
        Assert.Equal(message, processedMessage);
    }
    
    [Fact]
    public void ShouldSaveOcrResultAsTxtFile()
    {
        // Arrange
        var id = "5";
        var extractedText = "This is the OCR result.";
        var directoryPath = "ocr_results";
        var filePath = Path.Combine(directoryPath, $"{id}.txt");

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }

        // Act
        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(filePath, extractedText);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(extractedText, File.ReadAllText(filePath));

        // Cleanup
        File.Delete(filePath);
        Directory.Delete(directoryPath);
    }

    [Fact]
    public void ShouldCreateDirectoryIfNotExists()
    {
        // Arrange
        var directoryPath = "ocr_results";

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }

        // Act
        Directory.CreateDirectory(directoryPath);

        // Assert
        Assert.True(Directory.Exists(directoryPath));

        // Cleanup
        Directory.Delete(directoryPath);
    }

    [Fact]
    public void ShouldParseMessageCorrectly()
    {
        // Arrange
        var message = "5|This is the OCR result";
        var parts = message.Split('|');

        // Act
        var id = parts[0];
        var extractedText = parts[1];

        // Assert
        Assert.Equal("5", id);
        Assert.Equal("This is the OCR result", extractedText);
    }

    [Fact]
    public void ShouldHandleInvalidMessageGracefully()
    {
        // Arrange
        var invalidMessage = "InvalidMessageFormat";
        var parts = invalidMessage.Split('|');

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            var id = parts[0];
            var extractedText = parts[1];
        });
    }
}